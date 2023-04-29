using CommunityToolkit.Mvvm.ComponentModel;
using SharpDockerizer.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;

namespace SharpDockerizer.AvaloniaUI.Services;

/// <summary>
/// Service that allows navigation between different application pages
/// </summary>
public class AppNavigator
{

    #region Constructor

    public AppNavigator()
    {
        _currentViewModelType = typeof(MainApplicationViewViewModel);
        onNavigateTo?.Invoke(typeof(MainApplicationViewViewModel));
    }

    #endregion

    #region Fields

    public delegate void OnNavigateTo(Type type);
    public event OnNavigateTo? onNavigateTo;

    private readonly Stack<Type> _history = new Stack<Type>();
    private Type _currentViewModelType;

    #endregion

    #region Methods

    /// <summary>
    /// Sets <typeparamref name="T"/> as current opened page and adds it to history.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void NavigateTo<T>() where T : ObservableObject
    {
        onNavigateTo?.Invoke(typeof(T));
        _history.Push(_currentViewModelType);
        _currentViewModelType = typeof(T);
    }

    /// <summary>
    /// Navigates to previously opened page if it exists.
    /// </summary>
    public void GoBackToPrevious()
    {
        if (_history.Count == 0)
            return;
        
        
        var targetViewModelType = _history.Pop();
        onNavigateTo?.Invoke(targetViewModelType);
        _currentViewModelType = targetViewModelType;
    }

    #endregion
}
