// <copyright file="UserView.xaml.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Views.UserView
{
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// UserView.
    /// </summary>
    public sealed partial class UserView : UserControl
    {
        public UserView()
        {
            this.InitializeComponent();
            DisplayRegisterView();
        }

        public void DisplayRegisterView()
        {
            RootFrame.Navigate(typeof(RegisterPage));
        }

        public void DisplayLoginView()
        {
            RootFrame.Navigate(typeof(LoginPage));
        }

        public void DisplayUserDataView()
        {
            RootFrame.Navigate(typeof(UserDataPage));
        }
    }
}
