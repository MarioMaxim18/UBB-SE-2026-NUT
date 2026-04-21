using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.ModelViews;
using TeamNut.Models;
using TeamNut.ModelViews;
using TeamNut.Services.Interfaces;
using TeamNut.ViewModels;

namespace TeamNut.Views.MealPlanView
{
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
