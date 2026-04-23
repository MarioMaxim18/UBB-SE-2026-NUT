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
    /// <summary>
    /// NutritionistChatViewModel.
    /// </summary>
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
            this.Conversations = new ObservableCollection<Conversation>();
            this.Messages = new ObservableCollection<Message>();
            this.InputText = string.Empty;
            this.StatusMessage = string.Empty;

            this.chatService = cchatService;

            this.IsNutritionistUser = UserSession.Role == NutritionistRole;

            _ = this.LoadConversationsAsync();

            this.autoRefreshCts = new CancellationTokenSource();
            _ = this.AutoRefreshLoop(this.autoRefreshCts.Token);
        }

        partial void OnInputTextChanged(string value)
        {
            if (UserSession.Role == NutritionistRole && this.currentConversationId == null)
            {
                this.CanSend = false;
                this.StatusMessage = StatusSelectConversation;
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                this.CanSend = false;
                this.StatusMessage = string.Empty;
                return;
            }

            if (value.Length > MaxMessageLength)
            {
                this.CanSend = false;
                this.StatusMessage = StatusMessageTooLong;
                return;
            }

            if (!AllowedMessageRegex.IsMatch(value))
            {
                this.CanSend = false;
                this.StatusMessage = StatusInvalidCharacters;
                return;
            }

            this.CanSend = true;
            this.StatusMessage = string.Empty;
        }

        public async Task LoadConversationsAsync()
        {
            IEnumerable<Conversation> convs;

            if (UserSession.Role == NutritionistRole)
            {
                convs = this.IsNutritionistView
                    ? await this.chatService.GetConversationsWithUserMessagesAsync()
                    : await this.chatService.GetConversationsWhereNutritionistRespondedAsync(
                        UserSession.UserId ?? InvalidUserId);

                convs ??= Enumerable.Empty<Conversation>();
            }
            else
            {
                var conv = await this.chatService.GetOrCreateConversationForUserAsync(
                    UserSession.UserId ?? InvalidUserId);

                convs = conv != null
                    ? new[] { conv }
                    : Enumerable.Empty<Conversation>();
            }

            this.Conversations.Clear();
            foreach (var c in convs)
            {
                this.Conversations.Add(c);
            }

            if (UserSession.Role != NutritionistRole
                && this.currentConversationId == null
                && this.Conversations.Count > 0)
            {
                this.SelectedConversation = this.Conversations[0];
            }

            this.StatusMessage = this.Conversations.Any()
                ? string.Empty
                : StatusNoActiveConversations;
        }

        public async Task LoadMessagesForConversationAsync(int conversationId)
        {
            this.currentConversationId = conversationId;

            var msgs = (await this.chatService.GetMessagesForConversationAsync(conversationId)).ToList();

            if (this.Messages.Count == msgs.Count
                && this.Messages.Zip(msgs, (a, b) => a.Id == b.Id).All(eq => eq))
            {
                this.HasMessages = this.Messages.Count > 0;
                return;
            }

            this.Messages.Clear();

            foreach (var m in msgs)
            {
                this.Messages.Add(m);
            }

            this.HasMessages = this.Messages.Count > 0;
        }

        partial void OnSelectedConversationChanged(Conversation? value)
        {
            if (value != null)
            {
                _ = this.LoadMessagesForConversationAsync(value.Id);
            }
        }

        partial void OnIsNutritionistViewChanged(bool value)
        {
            _ = this.LoadConversationsAsync();
        }

        private async Task AutoRefreshLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(AutoRefreshSeconds), token);
                    await this.LoadConversationsAsync();

                    if (this.currentConversationId != null)
                    {
                        await this.LoadMessagesForConversationAsync(this.currentConversationId.Value);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        public void StopAutoRefresh()
        {
            this.autoRefreshCts?.Cancel();
        }

        [RelayCommand]
        public async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(this.InputText))
            {
                return;
            }

            if (this.InputText.Length > MaxMessageLength)
            {
                this.StatusMessage = StatusMessageTooLong;
                return;
            }

            if (!AllowedMessageRegex.IsMatch(this.InputText))
            {
                this.StatusMessage = StatusInvalidCharacters;
                return;
            }

            if (this.currentConversationId == null)
            {
                if (UserSession.Role == NutritionistRole)
                {
                    this.StatusMessage = StatusNutritionistCannotStartConversation;
                    return;
                }

                if (UserSession.UserId == null)
                {
                    return;
                }

                var conv = await this.chatService.GetOrCreateConversationForUserAsync(
                    UserSession.UserId.Value);

                this.currentConversationId = conv.Id;
            }

            if (UserSession.UserId == null)
            {
                return;
            }

            int senderId = UserSession.UserId.Value;
            bool isNutritionist = UserSession.Role == NutritionistRole;

            await this.chatService.AddMessageAsync(
                this.currentConversationId.Value,
                senderId,
                this.InputText.Trim(),
                isNutritionist);

            this.InputText = string.Empty;
            await this.LoadMessagesForConversationAsync(this.currentConversationId.Value);
        }
    }
}
