using Microsoft.UI.Xaml.Controls;

namespace TeamNut.Views.UserView
{
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
