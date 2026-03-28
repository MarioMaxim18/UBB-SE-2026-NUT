using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace TeamNut.Views.UserView
{
    public sealed partial class UserView : UserControl
    {
        public UserView()
        {
            this.InitializeComponent();
            displayRegisterView();
        }

        public void displayRegisterView()
        {
            RootFrame.Navigate(typeof(RegisterPage));
        }

        public void displayLoginView()
        {
            RootFrame.Navigate(typeof(LoginPage));
        }

        public void displayUserDataView()
        {
            RootFrame.Navigate(typeof(UserDataPage));
        }
    }
}