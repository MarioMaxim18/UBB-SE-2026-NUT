using System.Collections.Generic;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
    public interface IValidationService
    {
        List<string> ValidateUser(User user);
        List<string> ValidateUserData(UserData userData);
        bool IsValidTextInput(string input);
        bool IsNumericOnly(string input);
        bool IsValidReminderName(string name, int maxLength = 50);
    }
}
