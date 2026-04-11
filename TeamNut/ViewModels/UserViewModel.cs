using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private async Task OnRegister()
        {
            StatusMessage = string.Empty;

            if (IsNutritionistChecked)
                CurrentUser.Role = "Nutritionist";
            else
                CurrentUser.Role = "User";

            List<string> errors = CurrentUser.ValidateAndReturnErrors();
            if (errors.Any())
            {
                StatusMessage = string.Join(Environment.NewLine, errors);
                return;
            }

            if (await _userService.CheckIfUsernameExistsAsync(CurrentUser.Username))
            {
                StatusMessage = "Username already exists. Please choose another one.";
                return;
            }

            if (CurrentUser.Role == "User")
            {
                RegistrationValid?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var registeredUser = await _userService.RegisterUserAsync(CurrentUser);
                if (registeredUser != null)
                {
                    UserSession.Login(registeredUser.Id, registeredUser.Username, registeredUser.Role);
                    LoginSuccess?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [RelayCommand]
        private async Task OnSaveData()
        {
            StatusMessage = string.Empty;

            try
            {
                List<string> errors = CurrentUserData.GetValidationErrors();
                if (errors.Any())
                {
                    StatusMessage = string.Join(Environment.NewLine, errors);
                    return;
                }

                CurrentUserData.Age = CurrentUserData.CalculateAge(SelectedDate);
                if (CurrentUserData.Age <= 0)
                {
                    StatusMessage = "Please select a valid birthdate.";
                    return;
                }
                CurrentUserData.Bmi = CurrentUserData.CalculateBmi();
                CurrentUserData.CalorieNeeds = CurrentUserData.CalculateCalorieNeeds();
                CurrentUserData.ProteinNeeds = CurrentUserData.CalculateProteinNeeds();
                CurrentUserData.FatNeeds = CurrentUserData.CalculateFatNeeds();
                CurrentUserData.CarbNeeds = CurrentUserData.CalculateCarbNeeds();

                var registeredUser = await _userService.RegisterUserAsync(CurrentUser);
                if (registeredUser == null)
                {
                    StatusMessage = "Registration failed. Username might already exist.";
                    return;
                }

                CurrentUserData.UserId = registeredUser.Id;
                await _userService.AddUserDataAsync(CurrentUserData);

                UserSession.Login(registeredUser.Id, registeredUser.Username, registeredUser.Role);
                SaveDataSuccess?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                StatusMessage = "An error occurred while saving: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task OnLogin()
        {
            StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CurrentUser.Username) ||
                string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                StatusMessage = "Username and Password are required.";
                return;
            }

            try
            {
                var user = await _userService.LoginAsync(CurrentUser.Username, CurrentUser.Password);

                if (user != null)
                {
                    UserSession.Login(user.Id, user.Username, user.Role);
                    LoginSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    StatusMessage = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Could not reach the local database (NutData.db). Check the file is next to the project and you have read/write access. " + ex.Message;
            }
        }
    }
}
