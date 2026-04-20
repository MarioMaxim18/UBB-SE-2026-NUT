using Microsoft.UI.Xaml.Controls;

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
