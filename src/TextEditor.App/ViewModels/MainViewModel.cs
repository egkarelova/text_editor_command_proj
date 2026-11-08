using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextEditor.Core.Services;

namespace TextEditor.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public EditorViewModel Editor { get; }

    private readonly IThemeService _themeService;

    public MainViewModel(IThemeService themeService, EditorViewModel editor)
    {
        _themeService = themeService;
        Editor = editor;
    }

    [ObservableProperty]
    private string _statusText = "Ready";

    // --- File ---
    [RelayCommand]
    private Task NewAsync() => throw new NotImplementedException("Айдана");

    [RelayCommand]
    private Task OpenAsync() => throw new NotImplementedException("Айдана");

    [RelayCommand]
    private Task SaveAsync() => throw new NotImplementedException("Айдана");

    [RelayCommand]
    private Task SaveAsAsync() => throw new NotImplementedException("Айдана");

    [RelayCommand]
    private void Exit() => Application.Current.Shutdown();

    // --- View / Theme ---
    [RelayCommand]
    private Task ApplyLightThemeAsync() => _themeService.ApplyAsync("Light");

    [RelayCommand]
    private Task ApplyDarkThemeAsync() => _themeService.ApplyAsync("Dark");

    // --- Help ---
    [RelayCommand]
    private void ShowAbout()
    {
        var w = new Views.AboutWindow { Owner = Application.Current.MainWindow };
        w.ShowDialog();
    }
}