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
        private const string RoleNutritionist = "Nutritionist";
        private const string RoleUser = "User";
        private const string ErrorUsernameExists = "Username already exists. Please choose another one.";
        private const string ErrorInvalidBirthdate = "Please select a valid birthdate.";
        private const string ErrorRegistrationFailed = "Registration failed. Username might already exist.";
        private const string ErrorUsernamePasswordRequired = "Username and Password are required.";
        private const string ErrorInvalidCredentials = "Invalid username or password.";
        private const string ErrorDatabaseConnectionFormat = "Database Connection Failed! Start SSMS and check your server. Error: {0}";
        private const string ErrorSavingDataFormat = "An error occurred while saving: {0}";

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

            CurrentUser.Role = IsNutritionistChecked
                ? RoleNutritionist
                : RoleUser;

            List<string> errors = CurrentUser.ValidateAndReturnErrors();
            if (errors.Any())
            {
                StatusMessage = string.Join(Environment.NewLine, errors);
                return;
            }

            if (await _userService.CheckIfUsernameExistsAsync(CurrentUser.Username))
            {
                StatusMessage = ErrorUsernameExists;
                return;
            }

            if (CurrentUser.Role == RoleUser)
            {
                RegistrationValid?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var registeredUser = await _userService.RegisterUserAsync(CurrentUser);
                if (registeredUser != null)
                {
                    UserSession.Login(
                        registeredUser.Id,
                        registeredUser.Username,
                        registeredUser.Role);

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
                    StatusMessage = ErrorInvalidBirthdate;
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
                    StatusMessage = ErrorRegistrationFailed;
                    return;
                }

                CurrentUserData.UserId = registeredUser.Id;
                await _userService.AddUserDataAsync(CurrentUserData);

                UserSession.Login(
                    registeredUser.Id,
                    registeredUser.Username,
                    registeredUser.Role);

                SaveDataSuccess?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(ErrorSavingDataFormat, ex.Message);
            }
        }

        [RelayCommand]
        private async Task OnLogin()
        {
            StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CurrentUser.Username) ||
                string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                StatusMessage = ErrorUsernamePasswordRequired;
                return;
            }

            try
            {
                var user = await _userService.LoginAsync(
                    CurrentUser.Username,
                    CurrentUser.Password);

                if (user != null)
                {
                    UserSession.Login(
                        user.Id,
                        user.Username,
                        user.Role);

                    LoginSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    StatusMessage = ErrorInvalidCredentials;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(ErrorDatabaseConnectionFormat, ex.Message);
            }
        }
    }
}