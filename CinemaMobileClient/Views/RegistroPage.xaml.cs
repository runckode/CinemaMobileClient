﻿using CinemaMobileClient.Controllers;
using System.Net.NetworkInformation;
using SkiaSharp;

namespace CinemaMobileClient.Views;

public partial class RegistroPage : ContentPage
{
    private ApiService _apiService;
    private bool isPasswordVisible = false;
    private bool isPasswordVisiblep = false;
    private string base64Foto;
    private string nameFoto;
    byte[] imageToSave;

    public RegistroPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }


    private async void btnCancelar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void TogglePasswordVisibility(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;

        // Cambia la visibilidad del texto de la contrase�a
        entryPassword.IsPassword = !isPasswordVisible;

        // Cambia el icono del bot�n para reflejar la visibilidad actual de la contrase�a
        if (isPasswordVisible)
        {
            TogglePasswordVisibilityIcon.Source = "clave.png";
        }
        else
        {
            TogglePasswordVisibilityIcon.Source = "noclave.png";
        }
    }

    private void TogglePasswordVisibilityIconp_Clicked(object sender, EventArgs e)
    {
        isPasswordVisiblep = !isPasswordVisiblep;

        // Cambia la visibilidad del texto de la contrase�a
        entryPassword.IsPassword = !isPasswordVisiblep;

        // Cambia el icono del bot�n para reflejar la visibilidad actual de la contrase�a
        if (isPasswordVisiblep)
        {
            TogglePasswordVisibilityIconp.Source = "clave.png";
        }
        else
        {
            TogglePasswordVisibilityIconp.Source = "noclave.png";
        }
    }



    private async void btnfoto_Clicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                imageToSave = null;
                string localizacion = System.IO.Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                using (Stream sourceStream = await photo.OpenReadAsync())
                {
                    var resizedStream = ResizeImage(sourceStream, 200, 200);
                    base64Foto = ConvertToBase64(resizedStream);

                    using (FileStream imagenLocal = File.OpenWrite(localizacion))
                    {
                        await resizedStream.CopyToAsync(imagenLocal);
                    }

                    resizedStream.Position = 0;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await resizedStream.CopyToAsync(memoryStream);
                        imageToSave = memoryStream.ToArray();
                    }

                    img.Source = ImageSource.FromStream(() => new MemoryStream(imageToSave));
                }

                nameFoto = photo.FileName;
                entryNombres.Focus();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Se ha generado el siguiente error al agregar la imagen: " + ex.Message, "Aceptar");
        }
    }

    private string ConvertToBase64(Stream stream)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                var imageBytes = memoryStream.ToArray();

                if (imageBytes.Length == 0)
                {
                    throw new Exception("La imagen est� vac�a.");
                }

                return Convert.ToBase64String(imageBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al convertir la imagen a Base64: {ex.Message}");
            throw;
        }
    }

    private Stream ResizeImage(Stream inputStream, int width, int height)
    {
        using (var original = SKBitmap.Decode(inputStream))
        {
            var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);

            if (resized == null)
            {
                throw new Exception("Error al redimensionar la imagen.");
            }

            var image = SKImage.FromBitmap(resized);
            var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);

            var resizedStream = new MemoryStream();
            data.SaveTo(resizedStream);
            resizedStream.Seek(0, SeekOrigin.Begin);

            return resizedStream;
        }
    }




    private async void btnRegistrar_Clicked(object sender, EventArgs e)
    {
        ShowLoadingDialog();

        if (string.IsNullOrEmpty(base64Foto))
        {
            await DisplayAlert("Alerta", "Por favor agregar una foto.", "OK");
            HideLoadingDialog();
            return;
        }



        if (string.IsNullOrEmpty(entryNombres.Text))
        {
            await DisplayAlert("Alerta", "Por favor ingrese el nombre", "OK");
            HideLoadingDialog();
            return;
        }


        var data = new
        {
            usuario = entryUsuario.Text,
            password = entryPassword.Text,
            nombres = entryNombres.Text,
            apellidos = entryApellidos.Text,
            telefono = entryTelefono.Text,
            correo = entryCorreo.Text,
            esAdministrador = false,
            imgBase64 = "",

        };

        bool isSuccess = await _apiService.PostGlobalSuccessAsync("/api/Usuarios", data);

        if (isSuccess)
        {
            HideLoadingDialog();
            await DisplayAlert("Guardado!", "Usuario Registrado con exito, proceda a Iniciar Sesion.", "OK");
            base64Foto = string.Empty;
            entryUsuario.Text = string.Empty;
            entryPassword.Text = string.Empty;
            entryCPassword.Text = string.Empty;
            entryNombres.Text = string.Empty;
            entryApellidos.Text = string.Empty;
            entryTelefono.Text = string.Empty;
            entryCorreo.Text = string.Empty;
            img.Source = "anadir.png";
            imageToSave = null;
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Error al guardar registro, intente de nuevo.", "OK");
        }

    }

    private void ShowLoadingDialog()
    {
        btnRegistrar.IsEnabled = false;
    }

    private void HideLoadingDialog()
    {
        btnRegistrar.IsEnabled = true;
    }


}