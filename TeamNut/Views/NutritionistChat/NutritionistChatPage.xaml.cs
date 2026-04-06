using Microsoft.UI.Xaml.Controls;
using TeamNut.ViewModels;
using TeamNut.Models;
using Microsoft.UI.Xaml;
using System;

namespace TeamNut.Views.NutritionistChat
{
    public sealed partial class NutritionistChatPage : Page
    {
        public NutritionistChatViewModel ViewModel { get; } = new NutritionistChatViewModel();

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
            // after sending, attempt to scroll to bottom
            try
            {
                MessagesItems.UpdateLayout();
                if (MessagesItems.Items?.Count > 0)
                {
                    // get last container and bring into view by scrolling
                    MessagesScrollViewer.ChangeView(null, MessagesScrollViewer.ScrollableHeight, null, true);
                }
            }
            catch { }
        }

        protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.StopAutoRefresh();
        }
    }
}
