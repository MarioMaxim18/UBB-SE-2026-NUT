// <copyright file="Conversation.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Models
{
    using System;

    /// <summary>Represents a chat conversation between a user and a nutritionist.</summary>
    public class Conversation
    {
        public int Id { get; set; }

        public bool HasUnanswered { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;
    }
}
