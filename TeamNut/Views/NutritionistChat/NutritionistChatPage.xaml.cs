using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TeamNut.Views.NutritionistChat
{
    /// <summary>
    /// NutritionistChatPage.
    /// </summary>
    public sealed partial class NutritionistChatPage : Page
    {
        public NutritionistChatViewModel ViewModel { get; }

        public NutritionistChatPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<NutritionistChatViewModel>();
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

        protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.StopAutoRefresh();
        }
    }
}
