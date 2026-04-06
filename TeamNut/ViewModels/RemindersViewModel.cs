using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using Windows.System;
using System;
namespace TeamNut.ViewModels
{
    public partial class RemindersViewModel : ObservableObject
    {
        private readonly ReminderService _reminderService;

        
        public ObservableCollection<Reminder> Reminders { get; } = new();

        [ObservableProperty]
        private bool _isBusy;

        public RemindersViewModel()
        {
            
            _reminderService = new ReminderService();
        }

        

     
       
        [RelayCommand]
        public async Task DeleteReminder(Reminder reminder)
        {
            if (reminder == null) return;

            await _reminderService.DeleteReminder(reminder.Id);
            Reminders.Remove(reminder);
        }

        

        [ObservableProperty]
        private Reminder? _selectedReminder; 

        [RelayCommand]
        public async Task SaveReminder(Reminder reminder)
        {
            if (reminder == null) return;

            
            string result = await _reminderService.SaveReminder(reminder);

            if (result == "Success")
            {
                
                await LoadReminders();
            }
        }

        [ObservableProperty]
        private Reminder? _nextReminder;


        [RelayCommand]
        public async Task LoadReminders()
        {
            // 1. Safety check to prevent double-loading
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                int currentId = UserSession.UserId ?? 0;

                if (currentId != 0)
                {
                    
                    var items = (await _reminderService.GetUserReminders(currentId)).ToList();

                    

                    var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

                    if (dispatcher != null)
                    {
                        dispatcher.TryEnqueue(() =>
                        {
                            Reminders.Clear();
                            foreach (var item in items)
                            {
                                Reminders.Add(item);
                            }
                        });
                    }
                    else
                    {
                       
                        Reminders.Clear();
                        foreach (var item in items) Reminders.Add(item);
                    }

                    
                    NextReminder = await _reminderService.GetNextReminder(currentId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reminders Load Error: {ex.Message}");
            }
            finally
            {
                
                IsBusy = false;
            }
        }

      


        [RelayCommand]
        public void PrepareNewReminder()
        {
            
            SelectedReminder = new Reminder { UserId = 1 }; 
        }
    }
}