using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public partial class NutritionistChatViewModel : ObservableObject
    {
        private readonly ChatService chatService;
        private CancellationTokenSource? autoRefreshCts;
        private int? currentConversationId;

        [ObservableProperty]
        public partial ObservableCollection<Conversation> Conversations { get; set; } = new ObservableCollection<Conversation>();

        [ObservableProperty]
        public partial ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

        [ObservableProperty]
        public partial string InputText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool CanSend { get; set; }

        [ObservableProperty]
        public partial string StatusMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsNutritionistView { get; set; }

        [ObservableProperty]
        public partial Conversation? SelectedConversation { get; set; }

        [ObservableProperty]
        public partial bool HasMessages { get; set; }

        [ObservableProperty]
        public partial bool IsNutritionistUser { get; set; }

        public NutritionistChatViewModel()
        {
            chatService = new ChatService();
            _ = LoadConversationsAsync();

            autoRefreshCts = new CancellationTokenSource();
            _ = AutoRefreshLoop(autoRefreshCts.Token);

            Messages.CollectionChanged += (s, e) => HasMessages = Messages.Count > 0;

            IsNutritionistUser = UserSession.Role == "Nutritionist";
        }

        partial void OnInputTextChanged(string value)
        {
            if (UserSession.Role == "Nutritionist" && currentConversationId == null)
            {
                CanSend = false;
                StatusMessage = "Please select a conversation to respond.";
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                CanSend = false;
                StatusMessage = string.Empty;
                return;
            }

            if (value.Length > 1000)
            {
                CanSend = false;
                StatusMessage = "message too long";
                return;
            }

            if (!Regex.IsMatch(value, "^[a-zA-Z0-9 .,!?'\\-()]+$"))
            {
                CanSend = false;
                StatusMessage = "Only alphanumeric characters and basic punctuation are allowed.";
                return;
            }

            CanSend = true;
            StatusMessage = string.Empty;
        }

        public async Task LoadConversationsAsync()
        {
            IEnumerable<Conversation> convs;
            if (UserSession.Role == "Nutritionist")
            {
                if (IsNutritionistView)
                {
                    convs = await chatService.GetConversationsWithUserMessagesAsync();
                }
                else
                {
                    convs = await chatService.GetConversationsWhereNutritionistRespondedAsync(UserSession.UserId ?? 0);
                }
            }
            else
            {
                var conv = await chatService.GetOrCreateConversationForUserAsync(UserSession.UserId ?? 0);
                convs = new[] { conv };
            }
            Conversations.Clear();
            foreach (var c in convs)
            {
                Conversations.Add(c);
            }

            if (!Conversations.Any())
            {
                StatusMessage = "no active user inquiries at this time";
            }
            else
            {
                StatusMessage = string.Empty;
            }
        }

        public async Task LoadMessagesForConversationAsync(int conversationId)
        {
            currentConversationId = conversationId;
            var msgs = await chatService.GetMessagesForConversationAsync(conversationId);
            Messages.Clear();
            foreach (var m in msgs)
            {
                Messages.Add(m);
            }
        }

        partial void OnSelectedConversationChanged(Conversation? value)
        {
            if (value != null)
            {
                _ = LoadMessagesForConversationAsync(value.Id);
            }
        }

        partial void OnIsNutritionistViewChanged(bool value)
        {
            _ = LoadConversationsAsync();
        }

        private async Task AutoRefreshLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                    await LoadConversationsAsync();
                    if (currentConversationId != null)
                    {
                        await LoadMessagesForConversationAsync(currentConversationId.Value);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        public void StopAutoRefresh()
        {
            try
            {
                autoRefreshCts?.Cancel();
            }
            catch
            {
            }
        }

        [RelayCommand]
        public async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                return;
            }
            if (InputText.Length > 1000)
            {
                StatusMessage = "message too long";
                return;
            }

            if (!Regex.IsMatch(InputText, "^[a-zA-Z0-9 .,!?'\\-()]+$"))
            {
                StatusMessage = "Only alphanumeric characters and basic punctuation are allowed.";
                return;
            }

            if (currentConversationId == null)
            {
                if (UserSession.Role == "Nutritionist")
                {
                    StatusMessage = "Nutritionists can only respond to existing conversations.";
                    return;
                }
                if (UserSession.UserId == null)
                {
                    return;
                }
                var conv = await chatService.GetOrCreateConversationForUserAsync(UserSession.UserId.Value);
                currentConversationId = conv.Id;
            }

            var senderId = UserSession.UserId ?? 0;
            var isNutritionist = UserSession.Role == "Nutritionist";
            await chatService.AddMessageAsync(currentConversationId.Value, senderId, InputText.Trim(), isNutritionist);
            InputText = string.Empty;
            await LoadMessagesForConversationAsync(currentConversationId.Value);
        }
    }
}
