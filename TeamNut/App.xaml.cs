using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TeamNut.ModelViews;
using TeamNut.Repositories;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services;
using TeamNut.Services.Interfaces;
using TeamNut.ViewModels;
using TeamNut.Views;
using TeamNut.Views.MealPlanView;
using TeamNut.Views.UserView;

namespace TeamNut
{
    public partial class App : Application
    {
        internal Window? _window;

        public static IServiceProvider Services { get; private set; }


        public App()
        {
            this.UnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled: {e.Exception}");
            };

            InitializeComponent();
            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();


            services.AddSingleton<IDbConfig, DbConfig>();


            services.AddTransient<IChatRepository, ChatRepository>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<NutritionistChatViewModel>();


            services.AddTransient<IDailyLogRepository, DailyLogRepository>();
            services.AddTransient<IDailyLogService, DailyLogService>();
            services.AddTransient<DailyLogViewModel>();

            services.AddTransient<IShoppingListRepository, ShoppingListRepository>();
            services.AddTransient<IShoppingListService, ShoppingListService>();
            services.AddTransient<ShoppingListViewModel>();

            services.AddTransient<IIngredientRepository, IngredientRepository>();

            services.AddTransient<IInventoryRepository, InventoryRepository>();
            services.AddTransient<IInventoryService, InventoryService>();
            services.AddTransient<InventoryViewModel>();

            services.AddTransient<IMealPlanRepository, MealPlanRepository>();
            services.AddTransient<IMealPlanService, MealPlanService>();
            services.AddTransient<MealPlanViewModel>();

            services.AddTransient<IMealRepository, MealRepository>();
            services.AddTransient<IMealService, MealService>();
            services.AddTransient<MealSearchViewModel>();

            services.AddTransient<IReminderRepository, ReminderRepository>();
            services.AddSingleton<IReminderService, ReminderService>();
            services.AddTransient<RemindersViewModel>();

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserService, UserService>();
            services.AddSingleton<UserViewModel>();



            return services.BuildServiceProvider();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();

            _window.Content = new UserView();

            _window.Activate();
        }
    }
}
