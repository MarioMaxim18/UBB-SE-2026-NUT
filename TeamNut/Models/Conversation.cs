using System;

namespace TeamNut.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public bool HasUnansweredMessages { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}
