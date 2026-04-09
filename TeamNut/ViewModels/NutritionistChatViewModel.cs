using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private int? _currentConversationId;

        [ObservableProperty]
        private Conversation? selectedConversation;

        public NutritionistChatViewModel()
        {
            _chatService = new ChatService();
            _ = LoadConversationsAsync();

            // start periodic refresh
            _autoRefreshCts = new CancellationTokenSource();
            _ = AutoRefreshLoop(_autoRefreshCts.Token);

            // monitor messages collection to update HasMessages
            Messages.CollectionChanged += (s, e) => HasMessages = Messages.Count > 0;

            // set nutritionist flag for UI
            IsNutritionistUser = TeamNut.Models.UserSession.Role == "Nutritionist";
        }

        partial void OnInputTextChanged(string value)
        {
            var validation = _chatService.ValidateMessageInput(value, _currentConversationId != null, TeamNut.Models.UserSession.Role == "Nutritionist");
            CanSend = validation.CanSend;
            StatusMessage = validation.StatusMessage;
        }

        [ObservableProperty]
        private bool hasMessages = false;

        [ObservableProperty]
        private bool isNutritionistUser = false;

        public async Task LoadConversationsAsync()
        {
            IEnumerable<Conversation> convs;
            if (TeamNut.Models.UserSession.Role == "Nutritionist")
            {
                // nutritionist: toggle between showing all user chats or only those they've responded to
                if (IsNutritionistView)
                {
                    // Show all chats from users who have sent at least one message
                    convs = await _chatService.GetConversationsWithUserMessagesAsync();
                }
                else
                {
                    // Show only chats where this nutritionist has responded
                    convs = await _chatService.GetConversationsWhereNutritionistRespondedAsync(TeamNut.Models.UserSession.UserId ?? 0);
                }
            }
            else
            {
                // regular user: only their own conversation
                var conv = await _chatService.GetOrCreateConversationForUserAsync(TeamNut.Models.UserSession.UserId ?? 0);
                convs = new[] { conv };
            }
            Conversations.Clear();
            foreach (var c in convs) Conversations.Add(c);

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
            var validation = _chatService.ValidateMessageInput(InputText, _currentConversationId != null, TeamNut.Models.UserSession.Role == "Nutritionist");
            if (!validation.CanSend)
            {
                StatusMessage = validation.StatusMessage;
                return;
            }

            if (_currentConversationId == null)
            {
                if (TeamNut.Models.UserSession.Role == "Nutritionist")
                {
                    StatusMessage = "Please select a conversation to respond.";
                    return;
                }
                if (TeamNut.Models.UserSession.UserId == null) return;
                var conv = await _chatService.GetOrCreateConversationForUserAsync(TeamNut.Models.UserSession.UserId.Value);
                _currentConversationId = conv.Id;
            }

            var senderId = TeamNut.Models.UserSession.UserId ?? 0;
            var isNutritionist = TeamNut.Models.UserSession.Role == "Nutritionist";
            await _chatService.AddMessageAsync(_currentConversationId.Value, senderId, InputText.Trim(), isNutritionist);
            InputText = string.Empty;
            await LoadMessagesForConversationAsync(_currentConversationId.Value);
        }
    }
}


