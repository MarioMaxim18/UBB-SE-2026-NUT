using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;

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
        public partial User CurrentUser { get; set; } = new User();

        [ObservableProperty]
        public partial UserData CurrentUserData { get; set; } = new UserData();

        [ObservableProperty]
        public partial bool IsNutritionistChecked { get; set; }

        [ObservableProperty]
        public partial string StatusMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial DateTimeOffset SelectedDate { get; set; } = DateTimeOffset.Now;

        public event EventHandler? RegistrationValid;
        public event EventHandler? LoginSuccess;
        public event EventHandler? SaveDataSuccess;

        private readonly IUserService userService;
        private readonly IValidationService validationService;
        private readonly INutritionCalculationService nutritionCalculationService;

        public UserViewModel(
            IUserService uuserService,
            IValidationService vvalidationService,
            INutritionCalculationService nnutritionCalculationService)
        {
            userService = uuserService;
            validationService = vvalidationService;
            nutritionCalculationService = nnutritionCalculationService;
        }

        [RelayCommand]
        private async Task OnRegister()
        {
            StatusMessage = string.Empty;

            CurrentUser.Role = IsNutritionistChecked
                ? RoleNutritionist
                : RoleUser;

            List<string> errors = validationService.ValidateUser(CurrentUser);
            if (errors.Count > 0)
            {
                StatusMessage = string.Join(Environment.NewLine, errors);
                return;
            }

            if (await userService.CheckIfUsernameExistsAsync(CurrentUser.Username))
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
                var registeredUser = await userService.RegisterUserAsync(CurrentUser);
                if (registeredUser != null)
                {
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
                List<string> errors = validationService.ValidateUserData(CurrentUserData);
                if (errors.Count > 0)
                {
                    StatusMessage = string.Join(Environment.NewLine, errors);
                    return;
                }

                int age = nutritionCalculationService.CalculateAge(SelectedDate);
                if (age <= 0)
                {
                    StatusMessage = ErrorInvalidBirthdate;
                    return;
                }

                var registeredUser = await userService.RegisterUserAsync(CurrentUser);
                if (registeredUser == null)
                {
                    StatusMessage = ErrorRegistrationFailed;
                    return;
                }

                CurrentUserData.UserId = registeredUser.Id;
                await userService.AddUserDataAsync(CurrentUserData, SelectedDate);

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
                var user = await userService.LoginAsync(
                    CurrentUser.Username,
                    CurrentUser.Password);

                if (user != null)
                {
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