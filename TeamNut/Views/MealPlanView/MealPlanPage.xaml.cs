using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TeamNut.ModelViews;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.Views.MealPlanView
{
    public sealed partial class MealPlanPage : Page
    {
        public MealPlanViewModel ViewModel { get; } = new MealPlanViewModel();
        private UserService _userService;

        public MealPlanPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
            _userService = new UserService();

            // Subscribe to property changes to update UI
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.StatusMessage))
                {
                    StatusMessageText.Text = ViewModel.StatusMessage;
                }
                else if (e.PropertyName == nameof(ViewModel.GoalDescription))
                {
                    GoalDescriptionText.Text = ViewModel.GoalDescription;
                }
                else if (e.PropertyName == nameof(ViewModel.TotalNutritionSummary))
                {
                    TotalNutritionText.Text = ViewModel.TotalNutritionSummary;
                }
                else if (e.PropertyName == nameof(ViewModel.GeneratedMeals))
                {
                    UpdateMealsList();
                }
                else if (e.PropertyName == nameof(ViewModel.IsBusy))
                {
                    GenerateButton.IsEnabled = !ViewModel.IsBusy;
                }
                else if (e.PropertyName == nameof(ViewModel.ShowErrorDialog) && ViewModel.ShowErrorDialog)
                {
                    ShowErrorDialog();
                }
            };

            // Listen for changes to the collection
            ViewModel.GeneratedMeals.CollectionChanged += (s, e) =>
            {
                UpdateMealsList();
            };
        }

        private void GenerateMealPlan_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GenerateMealPlanCommand.CanExecute(null))
            {
                ViewModel.GenerateMealPlanCommand.Execute(null);
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            int? userId = UserSession.UserId;

            if (userId == null || userId <= 0)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Not Logged In",
                    Content = "You must be logged in to update your settings.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await errorDialog.ShowAsync();
                return;
            }

            // Get current user data
            var userData = await _userService.GetUserDataAsync(userId.Value);

            if (userData == null)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "No Data Found",
                    Content = "No user data found. Please complete your profile first.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await errorDialog.ShowAsync();
                return;
            }

            // Create the settings dialog
            await ShowSettingsDialog(userData);
        }

        private async System.Threading.Tasks.Task ShowSettingsDialog(UserData userData)
        {
            var dialog = new ContentDialog
            {
                Title = "⚙️ Update Your Preferences",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            // Create the form
            var stackPanel = new StackPanel { Spacing = 15 };

            // Weight
            var weightBox = new NumberBox
            {
                Header = "Weight (kg)",
                Value = userData.Weight,
                Minimum = 1,
                Maximum = 500,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };
            stackPanel.Children.Add(weightBox);

            // Height
            var heightBox = new NumberBox
            {
                Header = "Height (cm)",
                Value = userData.Height,
                Minimum = 1,
                Maximum = 300,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };
            stackPanel.Children.Add(heightBox);

            // Gender
            var genderCombo = new ComboBox
            {
                Header = "Gender",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedIndex = userData.Gender.Equals("male", StringComparison.OrdinalIgnoreCase) ? 0 : 1
            };
            genderCombo.Items.Add("Male");
            genderCombo.Items.Add("Female");
            stackPanel.Children.Add(genderCombo);

            // Goal
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
                "bulk" => 0,
                "cut" => 1,
                "maintenance" => 2,
                "well-being" => 3,
                _ => 2
            };
            stackPanel.Children.Add(goalCombo);

            // Info text
            var infoText = new TextBlock
            {
                Text = "💡 Changes will be reflected in your next meal plan generation.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.7,
                FontSize = 12,
                Margin = new Thickness(0, 10, 0, 0)
            };
            stackPanel.Children.Add(infoText);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Validate and save
                if (weightBox.Value < 1 || heightBox.Value < 1)
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Invalid Input",
                        Content = "Weight and height must be positive numbers.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await validationDialog.ShowAsync();
                    return;
                }

                // Update user data
                userData.Weight = (int)weightBox.Value;
                userData.Height = (int)heightBox.Value;
                userData.Gender = genderCombo.SelectedIndex == 0 ? "male" : "female";
                userData.Goal = goalCombo.SelectedItem.ToString().ToLower();

                // Recalculate nutritional needs
                userData.Bmi = userData.CalculateBmi();
                userData.CalorieNeeds = userData.CalculateCalorieNeeds();
                userData.ProteinNeeds = userData.CalculateProteinNeeds();
                userData.CarbNeeds = userData.CalculateCarbNeeds();
                userData.FatNeeds = userData.CalculateFatNeeds();

                try
                {
                    await _userService.UpdateUserDataAsync(userData);

                    var successDialog = new ContentDialog
                    {
                        Title = "✅ Settings Updated",
                        Content = "Your preferences have been saved successfully!\n\nGenerate a new meal plan to see the changes.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await successDialog.ShowAsync();

                    // Clear current meals to prompt regeneration
                    ViewModel.GeneratedMeals.Clear();
                    StatusMessageText.Text = "⚠️ Your settings have changed. Please generate a new meal plan.";
                    GoalDescriptionText.Text = "";
                    TotalNutritionText.Text = "";
                }
                catch (Exception ex)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to update settings: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await errorDialog.ShowAsync();
                }
            }
        }

        private async void ShowErrorDialog()
        {
            var dialog = new ContentDialog
            {
                Title = ViewModel.ErrorDialogTitle,
                Content = ViewModel.ErrorDialogMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            _ = await dialog.ShowAsync();
            ViewModel.ShowErrorDialog = false; // Reset the flag
        }

        private void UpdateMealsList()
        {
            MealsListView.ItemsSource = ViewModel.GeneratedMeals;

            if (ViewModel.GeneratedMeals.Count > 0)
            {
                MealsCountText.Text = $"📋 Your meal plan contains {ViewModel.GeneratedMeals.Count} meals:";
                MealsCountText.Visibility = Visibility.Visible;
            }
            else
            {
                MealsCountText.Visibility = Visibility.Collapsed;
            }
        }
    }
}