using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TextEditor.App.ViewModels;

namespace TextEditor.App.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        vm.LoginSucceeded += () =>
        {
            DialogResult = true;
            Close();
        };
    }

    private void LoginPasswordBox_Changed(object sender, RoutedEventArgs e)
        => _vm.Password = LoginPasswordBox.Password;

    private void RegisterPasswordBox_Changed(object sender, RoutedEventArgs e)
        => _vm.Password = RegisterPasswordBox.Password;

    private void ConfirmPasswordBox_Changed(object sender, RoutedEventArgs e)
        => _vm.ConfirmPassword = ConfirmPasswordBox.Password;
}
