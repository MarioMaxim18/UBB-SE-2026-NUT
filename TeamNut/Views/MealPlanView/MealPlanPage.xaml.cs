// <copyright file="MealPlanPage.xaml.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Views.MealPlanView
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using TeamNut.ModelViews;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;
    using TeamNut.ViewModels;

    /// <summary>
    /// MealPlanPage.
    /// </summary>
    public sealed partial class MealPlanPage : Page
    {
        public MealPlanViewModel ViewModel { get; }

        private IUserService userService;

        private const string ButtonOk = "OK";

        private const string ButtonSave = "Save";

        private const string ButtonCancel = "Cancel";

        private const string GenderMale = "male";

        private const string GenderFemale = "female";

        private const string TitleNotLoggedIn = "Not Logged In";

        private const string TitleNoDataFound = "No Data Found";

        private const string TitleInvalidInput = "Invalid Input";

        private const string TitleSettingsUpdated = "Settings Updated";

        private const string TitleError = "Error";

        private const string TitleNoMealPlan = "No Meal Plan";

        private const string TitleNoMeals = "No Meals";

        private const string TitleSuccess = "Success";

        private const string TitleSaveFailed = "Save Failed";

        private const string TitleRegenerationFailed = "Regeneration Failed";

        private const string TitleUpdatePreferences = "Update Your Preferences";

        private const string MsgLoginRequired = "You must be logged in to update your settings.";

        private const string MsgNoUserData = "No user data found. Please complete your profile first.";

        private const string MsgInvalidWeightHeight = "Weight and height must be positive numbers.";

        private const string MsgSettingsSaved = "Your preferences have been saved successfully!\n\n" + "Your new preferences will be applied to tomorrow's meal plan, which will be automatically generated when you log in.\n\n" + "Today's meal plan will remain unchanged.";

        private const string MsgSettingsStatus = "Settings saved! New preferences will apply to tomorrow's meal plan.";

        private const string MsgNoMealPlanLoaded = "No meal plan is currently loaded. Please generate a meal plan first.";

        private const string MsgNoMealsGenerated = "No meals to save. Please generate a meal plan first.";

        private const string MsgPreferenceInfo = "Changes will be reflected in your next meal plan generation.";

        private const string MealBullet = "\u2022";

        private const string KcalUnit = "kcal";

        private const int WeightMin = 1;

        private const int WeightMax = 500;

        private const int HeightMin = 1;

        private const int HeightMax = 300;

        private const int StackPanelSpacing = 15;

        private const double InfoTextOpacity = 0.7;

        private const int InfoFontSize = 12;

        private const int InfoTopMargin = 10;

        private const int IndexMale = 0;

        private const int IndexFemale = 1;

        private const int GoalBulk = 0;

        private const int GoalCut = 1;

        private const int GoalMaintenance = 2;

        private const int GoalWellBeing = 3;

        public MealPlanPage()
        {
            this.InitializeComponent();
            this.ViewModel = App.Services.GetRequiredService<MealPlanViewModel>();
            this.DataContext = this.ViewModel;
            this.userService = App.Services.GetRequiredService<IUserService>();

            this.ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.ViewModel.StatusMessage))
                {
                    StatusMessageText.Text = this.ViewModel.StatusMessage;
                }
                else if (e.PropertyName == nameof(this.ViewModel.GoalDescription))
                {
                    GoalDescriptionText.Text = this.ViewModel.GoalDescription;
                }
                else if (e.PropertyName == nameof(this.ViewModel.TotalNutritionSummary))
                {
                    TotalNutritionText.Text = this.ViewModel.TotalNutritionSummary;
                }
                else if (e.PropertyName == nameof(this.ViewModel.GeneratedMeals))
                {
                    this.UpdateMealsList();
                }
                else if (e.PropertyName == nameof(this.ViewModel.ShowErrorDialog) && this.ViewModel.ShowErrorDialog)
                {
                    this.ShowErrorDialog();
                }
            };

            this.ViewModel.GeneratedMeals.CollectionChanged += (s, e) => this.UpdateMealsList();

            _ = this.ViewModel.LoadOrGenerateTodaysMealPlanAsync();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            int? userId = UserSession.UserId;

            if (userId == null || userId <= 0)
            {
                await this.ShowSimpleDialog(TitleNotLoggedIn, MsgLoginRequired);
                return;
            }

            var userData = await this.userService.GetUserDataAsync(userId.Value);

            if (userData == null)
            {
                await this.ShowSimpleDialog(TitleNoDataFound, MsgNoUserData);
                return;
            }

            await this.ShowSettingsDialog(userData);
        }

        private async System.Threading.Tasks.Task ShowSettingsDialog(UserData userData)
        {
            var dialog = new ContentDialog
            {
                Title = TitleUpdatePreferences,
                PrimaryButtonText = ButtonSave,
                CloseButtonText = ButtonCancel,
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            var panel = new StackPanel { Spacing = StackPanelSpacing };

            var weightBox = new NumberBox
            {
                Header = "Weight (kg)",
                Value = userData.Weight,
                Minimum = WeightMin,
                Maximum = WeightMax,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var heightBox = new NumberBox
            {
                Header = "Height (cm)",
                Value = userData.Height,
                Minimum = HeightMin,
                Maximum = HeightMax,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var genderCombo = new ComboBox
            {
                Header = "Gender",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedIndex = userData.Gender.Equals(GenderMale, StringComparison.OrdinalIgnoreCase)
                    ? IndexMale
                    : IndexFemale
            };
            genderCombo.Items.Add("Male");
            genderCombo.Items.Add("Female");

            var goalCombo = new ComboBox
            {
                Header = "Goal",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            goalCombo.Items.Add("Bulk");
            goalCombo.Items.Add("Cut");
            goalCombo.Items.Add("Maintenance");
            goalCombo.Items.Add("Well-being");

            goalCombo.SelectedIndex = userData.Goal.ToLower() switch
            {
                "bulk" => GoalBulk,
                "cut" => GoalCut,
                "maintenance" => GoalMaintenance,
                "well-being" => GoalWellBeing,
                _ => GoalMaintenance
            };

            var infoText = new TextBlock
            {
                Text = MsgPreferenceInfo,
                TextWrapping = TextWrapping.Wrap,
                Opacity = InfoTextOpacity,
                FontSize = InfoFontSize,
                Margin = new Thickness(0, InfoTopMargin, 0, 0)
            };

            panel.Children.Add(weightBox);
            panel.Children.Add(heightBox);
            panel.Children.Add(genderCombo);
            panel.Children.Add(goalCombo);
            panel.Children.Add(infoText);

            dialog.Content = panel;

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            if (weightBox.Value < WeightMin || heightBox.Value < HeightMin)
            {
                await this.ShowSimpleDialog(TitleInvalidInput, MsgInvalidWeightHeight);
                return;
            }

            userData.Weight = (int)weightBox.Value;
            userData.Height = (int)heightBox.Value;
            userData.Gender = genderCombo.SelectedIndex == IndexMale ? GenderMale : GenderFemale;
            userData.Goal = goalCombo.SelectedItem?.ToString()?.ToLower() ?? "maintenance";

            try
            {
                await this.userService.UpdateUserDataAsync(userData);

                await this.ShowSimpleDialog(TitleSettingsUpdated, MsgSettingsSaved);
                StatusMessageText.Text = MsgSettingsStatus;
            }
            catch (Exception ex)
            {
                await this.ShowSimpleDialog(TitleError, $"Failed to update settings: {ex.Message}");
            }
        }

        private async void SaveToDailyLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ViewModel.CurrentMealPlanId <= 0)
                {
                    await this.ShowSimpleDialog(TitleNoMealPlan, MsgNoMealPlanLoaded);
                    return;
                }

                if (this.ViewModel.GeneratedMeals.Count == 0)
                {
                    await this.ShowSimpleDialog(TitleNoMeals, MsgNoMealsGenerated);
                    return;
                }

                await this.ViewModel.SaveToDailyLogAsync();

                var message = $"Successfully saved {this.ViewModel.GeneratedMeals.Count} meals:\n\n";
                foreach (var meal in this.ViewModel.GeneratedMeals)
                {
                    message += $"{MealBullet} {meal.Name}: {meal.Calories} {KcalUnit}\n";
                }

                await this.ShowSimpleDialog(TitleSuccess, message);

                StatusMessageText.Text =
                    $"All {this.ViewModel.GeneratedMeals.Count} meals saved to daily log!";
            }
            catch (Exception ex)
            {
                await this.ShowSimpleDialog(
                    TitleSaveFailed,
                    $"Failed to save to daily log:\n\n{ex.Message}");
            }
        }

        private async void AddMealToLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: MealViewModel meal })
            {
                return;
            }

            try
            {
                var mealPlanService = App.Services.GetRequiredService<IMealPlanService>();
                await mealPlanService.SaveMealToDailyLogAsync(meal.Id, meal.Calories);
                StatusMessageText.Text = $"{meal.Name} saved to daily log.";
            }
            catch (Exception ex)
            {
                await this.ShowSimpleDialog(TitleSaveFailed, $"Failed to save meal:\n\n{ex.Message}");
            }
        }

        private async void RegenerateTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusMessageText.Text = "Regenerating meal plan...";
                await this.ViewModel.ForceRegenerateMealPlanAsync();
            }
            catch (Exception ex)
            {
                await this.ShowSimpleDialog(TitleRegenerationFailed, ex.Message);
            }
        }

        private async void ShowErrorDialog()
        {
            await this.ShowSimpleDialog(
                this.ViewModel.ErrorDialogTitle,
                this.ViewModel.ErrorDialogMessage);

            this.ViewModel.ShowErrorDialog = false;
        }

        private async System.Threading.Tasks.Task ShowSimpleDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = ButtonOk,
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void UpdateMealsList()
        {
            MealsListView.ItemsSource = this.ViewModel.GeneratedMeals;
        }
    }
}
