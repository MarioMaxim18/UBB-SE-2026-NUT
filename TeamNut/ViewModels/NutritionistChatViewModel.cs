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
        private readonly IChatService chatService;
        private CancellationTokenSource? autoRefreshCts;
        private int? currentConversationId;
        private const int MaxMessageLength = 1000;
        private const int AutoRefreshSeconds = 5;
        private const int InvalidUserId = 0;
        private const string NutritionistRole = "Nutritionist";
        private const string StatusSelectConversation = "Please select a conversation to respond.";
        private const string StatusMessageTooLong = "Message too long.";
        private const string StatusInvalidCharacters = "Only alphanumeric characters and basic punctuation are allowed.";
        private const string StatusNoActiveConversations = "No active user inquiries at this time.";
        private const string StatusNutritionistCannotStartConversation = "Nutritionists can only respond to existing conversations.";
        private static readonly Regex AllowedMessageRegex = new Regex("^[a-zA-Z0-9 .,!?'\\-()]+$", RegexOptions.Compiled);

        [ObservableProperty]
        public partial ObservableCollection<Conversation> Conversations { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<Message> Messages { get; set; }

        [ObservableProperty]
        public partial string InputText { get; set; }

        [ObservableProperty]
        public partial bool CanSend { get; set; }

        [ObservableProperty]
        public partial string StatusMessage { get; set; }

        [ObservableProperty]
        public partial bool IsNutritionistView { get; set; }

        [ObservableProperty]
        public partial Conversation? SelectedConversation { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmptyPlaceholderVisible))]
        public partial bool HasMessages { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmptyPlaceholderVisible))]
        public partial bool IsNutritionistUser { get; set; }

        public bool IsEmptyPlaceholderVisible => !IsNutritionistUser && !HasMessages;

        public NutritionistChatViewModel(IChatService cchatService)
        {
            Conversations = new ObservableCollection<Conversation>();
            Messages = new ObservableCollection<Message>();
            InputText = string.Empty;
            StatusMessage = string.Empty;

            chatService = cchatService;

            IsNutritionistUser = UserSession.Role == NutritionistRole;

            _ = LoadConversationsAsync();

            autoRefreshCts = new CancellationTokenSource();
            _ = AutoRefreshLoop(autoRefreshCts.Token);

            Messages.CollectionChanged += (_, _) =>
                HasMessages = Messages.Count > 0;
        }

        partial void OnInputTextChanged(string value)
        {
            if (UserSession.Role == NutritionistRole && currentConversationId == null)
            {
                CanSend = false;
                StatusMessage = StatusSelectConversation;
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                CanSend = false;
                StatusMessage = string.Empty;
                return;
            }

            if (value.Length > MaxMessageLength)
            {
                CanSend = false;
                StatusMessage = StatusMessageTooLong;
                return;
            }

            if (!AllowedMessageRegex.IsMatch(value))
            {
                CanSend = false;
                StatusMessage = StatusInvalidCharacters;
                return;
            }

            CanSend = true;
            StatusMessage = string.Empty;
        }

        public async Task LoadConversationsAsync()
        {
            IEnumerable<Conversation> convs;

            if (UserSession.Role == NutritionistRole)
            {
                convs = IsNutritionistView
                    ? await chatService.GetConversationsWithUserMessagesAsync()
                    : await chatService.GetConversationsWhereNutritionistRespondedAsync(
                        UserSession.UserId ?? InvalidUserId);
            }
            else
            {
                var conv = await chatService.GetOrCreateConversationForUserAsync(
                    UserSession.UserId ?? InvalidUserId);

                convs = new[] { conv };
            }

            Conversations.Clear();
            foreach (var c in convs)
            {
                Conversations.Add(c);
            }

            StatusMessage = Conversations.Any()
                ? string.Empty
                : StatusNoActiveConversations;
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
                    await Task.Delay(TimeSpan.FromSeconds(AutoRefreshSeconds), token);
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
            autoRefreshCts?.Cancel();
        }

        [RelayCommand]
        public async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                return;
            }

            if (InputText.Length > MaxMessageLength)
            {
                StatusMessage = StatusMessageTooLong;
                return;
            }

            if (!AllowedMessageRegex.IsMatch(InputText))
            {
                StatusMessage = StatusInvalidCharacters;
                return;
            }

            if (currentConversationId == null)
            {
                if (UserSession.Role == NutritionistRole)
                {
                    StatusMessage = StatusNutritionistCannotStartConversation;
                    return;
                }

                if (UserSession.UserId == null)
                {
                    return;
                }

                var conv = await chatService.GetOrCreateConversationForUserAsync(
                    UserSession.UserId.Value);

                currentConversationId = conv.Id;
            }

            int senderId = UserSession.UserId ?? InvalidUserId;
            bool isNutritionist = UserSession.Role == NutritionistRole;

            await chatService.AddMessageAsync(
                currentConversationId.Value,
                senderId,
                InputText.Trim(),
                isNutritionist);

            InputText = string.Empty;
            await LoadMessagesForConversationAsync(currentConversationId.Value);
        }
    }
}
