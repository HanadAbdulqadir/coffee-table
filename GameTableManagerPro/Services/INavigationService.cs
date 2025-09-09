using System;

namespace GameTableManagerPro.Services;

public interface INavigationService
{
    event EventHandler<string>? CurrentViewChanged;
    
    string CurrentView { get; }
    
    void NavigateTo(string viewName);
    void NavigateToDashboard();
    void NavigateToTableManagement();
    void NavigateToDeployment();
    void NavigateToHealthMonitoring();
    void NavigateToSettings();
    
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    void GoBack();
    void GoForward();
}
