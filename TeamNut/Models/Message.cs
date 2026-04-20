using System;

namespace TeamNut.Models
{
    /// <summary>Represents a chat message within a conversation.</summary>
    public class Message
    {
        /// <summary>Gets or sets the message identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets when the message was sent.</summary>
        public DateTime SentAt { get; set; }

        /// <summary>Gets or sets the conversation identifier.</summary>
        public int ConversationId { get; set; }

        /// <summary>Gets or sets the sender's user identifier.</summary>
        public int SenderId { get; set; }

        /// <summary>Gets or sets the sender's username.</summary>
        public string SenderUsername { get; set; } = string.Empty;

        /// <summary>Gets or sets the sender's role (e.g. user, nutritionist).</summary>
        public string SenderRole { get; set; } = string.Empty;

        /// <summary>Gets or sets the message text content.</summary>
        public string TextContent { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether the message is from the current user.</summary>
        public bool IsFromCurrentUser { get; set; }

        /// <summary>Gets the formatted sent-at timestamp.</summary>
        public string SentAtFormatted => SentAt.ToString("g");
    }
}
