using Microsoft.UI.Xaml.Controls;

namespace TeamNut.Views.UserView
{
    /// <summary>Host control that navigates between login, register, and user-data pages.</summary>
    public sealed partial class UserView : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="UserView"/> class.</summary>
        public UserView()
        {
            this.InitializeComponent();
            DisplayRegisterView();
        }

        /// <summary>Navigates to the registration page.</summary>
        public void DisplayRegisterView()
        {
            RootFrame.Navigate(typeof(RegisterPage));
        }

        /// <summary>Navigates to the login page.</summary>
        public void DisplayLoginView()
        {
            RootFrame.Navigate(typeof(LoginPage));
        }

        /// <summary>Navigates to the user data page.</summary>
        public void DisplayUserDataView()
        {
            RootFrame.Navigate(typeof(UserDataPage));
        }
    }
}
