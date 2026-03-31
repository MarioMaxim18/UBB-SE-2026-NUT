using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public partial class UserViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial User CurrentUser { get; set; } = new();
        [ObservableProperty]
        public partial UserData CurrentUserData { get; set; } = new();
        [ObservableProperty]
        public partial bool IsNutritionistChecked { get; set; }
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial DateTimeOffset SelectedDate { get; set; } = DateTimeOffset.Now;
        public event EventHandler RegistrationValid;
        public event EventHandler LoginSuccess;
        public event EventHandler SaveDataSuccess;
        private readonly UserService _userService;

        public UserViewModel()
        {
            _userService = new UserService();
        }
        [RelayCommand]
        private async void OnRegister()
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
            if (await _userService.CheckIfUsernameExistsAsync(CurrentUser.Username))
            {
                this.StatusMessage = "Username already exists. Please choose another one.";
                return;
            }
            if(CurrentUser.Role == "User")
                RegistrationValid?.Invoke(this, EventArgs.Empty);
            else
            {
                var registeredUser = await _userService.RegisterUserAsync(CurrentUser);
                if (registeredUser != null)
                {

                    LoginSuccess?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        [RelayCommand]
        private async void OnSaveData()
        {
            StatusMessage = string.Empty;
            List<string> errors = CurrentUserData.GetValidationErrors();
            if (errors.Any())
            {
                StatusMessage = string.Join(Environment.NewLine, errors);
                return;
            }
            CurrentUserData.CalculateAge(SelectedDate);
            var registeredUser = await _userService.RegisterUserAsync(CurrentUser);
            if (registeredUser == null)
            {
                StatusMessage = "Registration failed. Username might already exist.";
                return;
            }
            CurrentUserData.UserId = registeredUser.Id;
            await _userService.AddUserDataAsync(CurrentUserData);

            
            SaveDataSuccess?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private async void OnLogin()
        {
            StatusMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(CurrentUser.Username) || string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                StatusMessage = "Username and Password are required.";
                return;
            }

            var user = await _userService.LoginAsync(CurrentUser.Username, CurrentUser.Password);
            if (user != null)
            {
                
                LoginSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StatusMessage = "Invalid username or password.";
            }
        }
    }
}
