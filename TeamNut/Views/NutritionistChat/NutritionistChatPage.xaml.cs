using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.ViewModels;

namespace TeamNut.Views.NutritionistChat
{
    /// <summary>Page for the nutritionist chat feature.</summary>
    public sealed partial class NutritionistChatPage : Page
    {
        /// <summary>Gets the view model.</summary>
        public NutritionistChatViewModel ViewModel { get; } = new NutritionistChatViewModel();

        /// <summary>Initializes a new instance of the <see cref="NutritionistChatPage"/> class.</summary>
        public NutritionistChatPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }

        private async void ConversationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView lv && lv.SelectedItem is Conversation conv)
            {
                ViewModel.SelectedConversation = conv;
                await ViewModel.LoadMessagesForConversationAsync(conv.Id);
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SendMessageAsync();
            try
            {
                MessagesItems.UpdateLayout();
                if (MessagesItems.Items?.Count > 0)
                {
                    MessagesScrollViewer.ChangeView(null, MessagesScrollViewer.ScrollableHeight, null, true);
                }
            }
            catch
            {
            }
        }

        /// <summary>Stops auto-refresh when navigating away.</summary>
        /// <param name="e">Navigation event arguments.</param>
        protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.StopAutoRefresh();
        }
    }
}
