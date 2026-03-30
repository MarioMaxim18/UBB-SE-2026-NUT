using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
// using TeamNut.ViewModels;

namespace TeamNut.Views.MealPlanView
{
    /// <summary>
    /// Main page for viewing and generating the meal plan.
    /// </summary>
    public sealed partial class MealPlanPage : Page
    {
        // This line will be activated in task NUT-77.
        // It links the UI with the business logic.
        // public MealPlanViewModel ViewModel => App.MainViewModel; 

        public MealPlanPage()
        {
            this.InitializeComponent();

            // Sets the data context to enable Data Binding (x:Bind).
            // This will be activated in task NUT-77.
            // this.DataContext = ViewModel;
        }
    }
}