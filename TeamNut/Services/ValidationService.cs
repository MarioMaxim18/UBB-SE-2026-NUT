// <copyright file="ValidationService.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;

    public class ValidationService : IValidationService
    {
        private static readonly Regex AllowedTextInputRegex = new Regex("^[a-zA-Z0-9 .,!?'\\-()]+$", RegexOptions.Compiled);

        public List<string> ValidateUser(User user)
        {
            if (user == null)
            {
                return new List<string> { "User cannot be null" };
            }

            return user.ValidateAndReturnErrors();
        }

        public List<string> ValidateUserData(UserData userData)
        {
            if (userData == null)
            {
                return new List<string> { "User data cannot be null" };
            }

            return userData.GetValidationErrors();
        }

        public bool IsValidTextInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return AllowedTextInputRegex.IsMatch(input);
        }

        public bool IsNumericOnly(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return true;
            }

            return input.All(char.IsDigit);
        }

        public bool IsValidReminderName(string name, int maxLength = 50)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return name.Length <= maxLength;
        }
    }
}
