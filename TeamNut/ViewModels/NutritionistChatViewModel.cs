using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using System.Threading;

namespace TeamNut.ViewModels
{
    public partial class NutritionistChatViewModel : ObservableObject
    {
        private readonly ChatService _chatService;
        private CancellationTokenSource? _autoRefreshCts;

        [ObservableProperty]
        private ObservableCollection<Conversation> conversations = new();

        [ObservableProperty]
        private ObservableCollection<Message> messages = new();

        [ObservableProperty]
        private string inputText = string.Empty;

        [ObservableProperty]
        private bool canSend = false;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool isNutritionistView = false;

        [ObservableProperty]
        private bool hasMessages = false;

        [ObservableProperty]
        private bool isNutritionistUser = false;

        private int? _currentConversationId;

        [ObservableProperty]
        private Conversation? selectedConversation;

        public NutritionistChatViewModel()
        {
            _chatService = new ChatService();
            _ = LoadConversationsAsync();

            // Start periodic refresh
            _autoRefreshCts = new CancellationTokenSource();
            _ = AutoRefreshLoop(_autoRefreshCts.Token);

            // Monitor messages collection to update HasMessages
            Messages.CollectionChanged += (s, e) => HasMessages = Messages.Count > 0;

            // Set nutritionist flag for UI
            IsNutritionistUser = UserSession.Role == "Nutritionist";
        }

        partial void OnInputTextChanged(string value)
        {
            if (UserSession.Role == "Nutritionist" && _currentConversationId == null)
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
                StatusMessage = "Message too long.";
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
                    convs = await _chatService.GetConversationsWithUserMessagesAsync();
                }
                else
                {
                    convs = await _chatService.GetConversationsWhereNutritionistRespondedAsync(UserSession.UserId ?? 0);
                }
            }
            else
            {
                var conv = await _chatService.GetOrCreateConversationForUserAsync(UserSession.UserId ?? 0);
                convs = new[] { conv };
            }

            Conversations.Clear();
            foreach (var c in convs) Conversations.Add(c);

            StatusMessage = !Conversations.Any() ? "No active user inquiries at this time." : string.Empty;
        }

        public async Task LoadMessagesForConversationAsync(int conversationId)
        {
            _currentConversationId = conversationId;
            var msgs = await _chatService.GetMessagesForConversationAsync(conversationId);

            Messages.Clear();
            foreach (var m in msgs) Messages.Add(m);
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

                    if (_currentConversationId != null)
                    {
                        await LoadMessagesForConversationAsync(_currentConversationId.Value);
                    }
                }
            }
            catch (TaskCanceledException) { }
        }

        public void StopAutoRefresh()
        {
            try
            {
                _autoRefreshCts?.Cancel();
            }
            catch { }
        }

        [RelayCommand]
        public async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText)) return;

            if (InputText.Length > 1000)
            {
                StatusMessage = "Message too long.";
                return;
            }

            if (!Regex.IsMatch(InputText, "^[a-zA-Z0-9 .,!?'\\-()]+$"))
            {
                StatusMessage = "Only alphanumeric characters and basic punctuation are allowed.";
                return;
            }

            if (_currentConversationId == null)
            {
                if (UserSession.Role == "Nutritionist")
                {
                    StatusMessage = "Nutritionists can only respond to existing conversations.";
                    return;
                }

                if (UserSession.UserId == null) return;

                var conv = await _chatService.GetOrCreateConversationForUserAsync(UserSession.UserId.Value);
                _currentConversationId = conv.Id;
            }

            var senderId = UserSession.UserId ?? 0;
            var isNutritionist = UserSession.Role == "Nutritionist";

            await _chatService.AddMessageAsync(_currentConversationId.Value, senderId, InputText.Trim(), isNutritionist);

            InputText = string.Empty;
            await LoadMessagesForConversationAsync(_currentConversationId.Value);
        }
    }
}