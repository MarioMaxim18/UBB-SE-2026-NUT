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
    /// <summary>View model for user registration, login, and profile data.</summary>
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

        /// <summary>Gets or sets the current user being registered or logged in.</summary>
        [ObservableProperty]
        public partial User CurrentUser { get; set; } = new User();

        /// <summary>Gets or sets the health profile data for the current user.</summary>
        [ObservableProperty]
        public partial UserData CurrentUserData { get; set; } = new UserData();

        /// <summary>Gets or sets a value indicating whether the nutritionist role is selected.</summary>
        [ObservableProperty]
        public partial bool IsNutritionistChecked { get; set; }

        /// <summary>Gets or sets the status message shown to the user.</summary>
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = string.Empty;

        /// <summary>Gets or sets the selected birth date.</summary>
        [ObservableProperty]
        public partial DateTimeOffset SelectedDate { get; set; } = DateTimeOffset.Now;

        /// <summary>Raised when registration data is valid and the user should proceed to enter health data.</summary>
        public event EventHandler? RegistrationValid;

        /// <summary>Raised when login succeeds.</summary>
        public event EventHandler? LoginSuccess;
        private readonly IUserService userService;

        /// <summary>Raised when health data is saved successfully.</summary>
        public event EventHandler? SaveDataSuccess;

        /// <summary>Initializes a new instance of the <see cref="UserViewModel"/> class.</summary>
        public UserViewModel(IUserService uuserService)
        {
            userService = uuserService;
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

                var registeredUser = await userService.RegisterUserAsync(CurrentUser);
                if (registeredUser == null)
                {
                    StatusMessage = ErrorRegistrationFailed;
                    return;
                }

                CurrentUserData.UserId = registeredUser.Id;
                await userService.AddUserDataAsync(CurrentUserData);

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
                var user = await userService.LoginAsync(
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