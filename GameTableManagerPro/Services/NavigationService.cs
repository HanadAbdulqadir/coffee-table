using System;
using System.Collections.Generic;

namespace GameTableManagerPro.Services;

public class NavigationService : INavigationService
{
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();
    private string _currentView = "Dashboard";

    public event EventHandler<string>? CurrentViewChanged;

    public string CurrentView
    {
        get => _currentView;
        private set
        {
            if (_currentView != value)
            {
                _currentView = value;
                CurrentViewChanged?.Invoke(this, value);
            }
        }
    }

    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;

    public void NavigateTo(string viewName)
    {
        if (string.IsNullOrEmpty(viewName))
            throw new ArgumentException("View name cannot be null or empty", nameof(viewName));

        if (CurrentView != viewName)
        {
            _backStack.Push(CurrentView);
            _forwardStack.Clear();
            CurrentView = viewName;
        }
    }

    public void NavigateToDashboard() => NavigateTo("Dashboard");
    public void NavigateToTableManagement() => NavigateTo("TableManagement");
    public void NavigateToDeployment() => NavigateTo("Deployment");
    public void NavigateToHealthMonitoring() => NavigateTo("HealthMonitoring");
    public void NavigateToAssetManagement() => NavigateTo("AssetManagement");
    public void NavigateToSettings() => NavigateTo("Settings");

    public void GoBack()
    {
        if (!CanGoBack)
            return;

        _forwardStack.Push(CurrentView);
        CurrentView = _backStack.Pop();
    }

    public void GoForward()
    {
        if (!CanGoForward)
            return;

        _backStack.Push(CurrentView);
        CurrentView = _forwardStack.Pop();
    }
}
