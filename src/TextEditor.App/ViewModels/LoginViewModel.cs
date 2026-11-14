using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextEditor.Core.Services;

namespace TextEditor.App.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _auth;

    public event Action? LoginSucceeded;

    public LoginViewModel(IAuthService auth) => _auth = auth;

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var result = await _auth.LoginAsync(Login, Password);
            HandleResult(result);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _auth.RegisterAsync(Login, Password);
            HandleResult(result);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task GuestAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var result = await _auth.EnterAsGuestAsync();
            HandleResult(result);
        }
        finally { IsBusy = false; }
    }

    private void HandleResult(Core.Models.AuthResult result)
    {
        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }
        LoginSucceeded?.Invoke();
    }
}
