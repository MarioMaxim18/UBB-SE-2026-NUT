using System;

namespace TeamNut.Models
{
    /// <summary>Represents a chat conversation between a user and a nutritionist.</summary>
    public class Conversation
    {
        /// <summary>Gets or sets the conversation identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets a value indicating whether the conversation has unanswered messages.</summary>
        public bool HasUnanswered { get; set; }

        /// <summary>Gets or sets the user identifier.</summary>
        public int UserId { get; set; }

        /// <summary>Gets or sets the username of the participant.</summary>
        public string Username { get; set; } = string.Empty;
    }
}
