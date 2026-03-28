using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.ModelViews
{
    public partial class UserViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial User CurrentUser { get; set; } = new();
        [ObservableProperty]
        public partial bool IsNutritionistChecked { get; set; }
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = string.Empty;
        public event EventHandler RegistrationValid;
        public UserViewModel()
        {
        }
        [RelayCommand]
        private void OnRegister()
        {
            StatusMessage = string.Empty;
            if (IsNutritionistChecked)
            {
                CurrentUser.Role = "Nutritionist";
            }
            else
                {
                CurrentUser.Role = "User";
            }
            List<String> errors = CurrentUser.ValidateAndReturnErrors();
            if (errors.Any())
            {
                StatusMessage = string.Join(Environment.NewLine, errors);
                return;
            }
            //TODO: Check user against database, show error if username already exists
            if(CurrentUser.Role == "User")
                RegistrationValid?.Invoke(this, EventArgs.Empty);
        }
    }
}
