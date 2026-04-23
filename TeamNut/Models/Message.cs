using System;

namespace TeamNut.Models
{
    public class Message
    {
        public int Id { get; set; }
        public DateTime SentAt { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public bool IsFromCurrentUser { get; set; }

        public string SentAtFormatted => SentAt.ToString("g");
    }
}