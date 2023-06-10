using CommunityToolkit.Mvvm.ComponentModel;
using SharpDockerizer.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Events;

namespace SharpDockerizer.AvaloniaUI.Services;

/// <summary>
/// Service that allows navigation between different application pages
/// </summary>
public class AppNavigator
{
    #region Constructor

    public AppNavigator(IMessenger messenger)
    {
        _messenger = messenger;
        _currentViewModelType = typeof(MainApplicationViewViewModel);
        onNavigateTo?.Invoke(typeof(MainApplicationViewViewModel));

        // On app restart we need to reset AppNavigator state to default
        _messenger.Register<NeedAppRestartEvent>(this, (sender, message) =>
        {
            _currentViewModelType = typeof(MainApplicationViewViewModel);
            _history.Clear();
            _history.Push(typeof(MainApplicationViewViewModel));
        });
    }

    #endregion

    #region Fields

    public delegate void OnNavigateTo(Type type);

    public event OnNavigateTo? onNavigateTo;

    private readonly Stack<Type> _history = new Stack<Type>();
    private Type _currentViewModelType;
    private readonly IMessenger _messenger;

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