﻿namespace CinemaMobileClient.ViewModels
{
  public class ViewModelBase : BindableBase, INavigationAware, IDestructible
  {
    private string _title;

    public ViewModelBase(INavigationService navigationService)
    {
      NavigationService = navigationService;
    }

    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    protected INavigationService NavigationService { get; private set; }

    public virtual void Destroy()
    {
    }
    
    public virtual void OnNavigatedFrom(INavigationParameters parameters)
    {
    }
    
    public virtual void OnNavigatedTo(INavigationParameters parameters)
    {
    }
  }
}
