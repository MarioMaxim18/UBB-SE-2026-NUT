namespace TeamNut.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using TeamNut.Models;
    using TeamNut.Services;
    using TeamNut.Services.Interfaces;

    /// <summary>
    /// UserViewModel.
    /// </summary>
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
            this.userService = uuserService;
            this.validationService = vvalidationService;
            this.nutritionCalculationService = nnutritionCalculationService;
        }

        [RelayCommand]
        private async Task OnRegister()
        {
            this.StatusMessage = string.Empty;

            this.CurrentUser.Role = this.IsNutritionistChecked
                ? RoleNutritionist
                : RoleUser;

            List<string> errors = this.validationService.ValidateUser(this.CurrentUser);
            if (errors.Count > 0)
            {
                this.StatusMessage = string.Join(Environment.NewLine, errors);
                return;
            }

            if (await this.userService.CheckIfUsernameExistsAsync(this.CurrentUser.Username))
            {
                this.StatusMessage = ErrorUsernameExists;
                return;
            }

            if (this.CurrentUser.Role == RoleUser)
            {
                this.RegistrationValid?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var registeredUser = await this.userService.RegisterUserAsync(this.CurrentUser);
                if (registeredUser != null)
                {
                    this.LoginSuccess?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [RelayCommand]
        private async Task OnSaveData()
        {
            this.StatusMessage = string.Empty;

            try
            {
                List<string> errors = this.validationService.ValidateUserData(this.CurrentUserData);
                if (errors.Count > 0)
                {
                    this.StatusMessage = string.Join(Environment.NewLine, errors);
                    return;
                }

                int age = this.nutritionCalculationService.CalculateAge(this.SelectedDate);
                if (age <= 0)
                {
                    this.StatusMessage = ErrorInvalidBirthdate;
                    return;
                }

                var registeredUser = await this.userService.RegisterUserAsync(this.CurrentUser);
                if (registeredUser == null)
                {
                    this.StatusMessage = ErrorRegistrationFailed;
                    return;
                }

                this.CurrentUserData.UserId = registeredUser.Id;
                await this.userService.AddUserDataAsync(this.CurrentUserData, this.SelectedDate);

                this.SaveDataSuccess?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                this.StatusMessage = string.Format(ErrorSavingDataFormat, ex.Message);
            }
        }

        [RelayCommand]
        private async Task OnLogin()
        {
            this.StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(this.CurrentUser.Username) ||
                string.IsNullOrWhiteSpace(this.CurrentUser.Password))
            {
                this.StatusMessage = ErrorUsernamePasswordRequired;
                return;
            }

            try
            {
                var user = await this.userService.LoginAsync(
                    this.CurrentUser.Username,
                    this.CurrentUser.Password);

                if (user != null)
                {
                    this.CurrentUser = user;
                    this.LoginSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    this.StatusMessage = ErrorInvalidCredentials;
                }
            }
            catch (Exception ex)
            {
                this.StatusMessage = string.Format(ErrorDatabaseConnectionFormat, ex.Message);
            }
        }
    }
}
