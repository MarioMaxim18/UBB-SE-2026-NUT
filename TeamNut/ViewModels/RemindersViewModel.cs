using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;
using Windows.System;
using System;
namespace TeamNut.ViewModels
{
    public partial class RemindersViewModel : ObservableObject
    {
        private readonly IReminderService _reminderService;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcher;

        
        public ObservableCollection<Reminder> Reminders { get; } = new();

        [ObservableProperty]
        private bool _isBusy;

        public RemindersViewModel(IReminderService reminderService)
        {
            _reminderService = reminderService;
           
            _dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            _reminderService.RemindersChanged += OnRemindersChanged;
        }

        private async void OnRemindersChanged(object? sender, int userId)
        {
            
            var current = UserSession.UserId ?? 0;
            if (current == userId)
            {
                await LoadReminders();
            }
        }

        

     
       
        [RelayCommand]
        public async Task DeleteReminder(Reminder reminder)
        {
            if (reminder == null) return;

            await _reminderService.DeleteReminder(reminder.Id);
            if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() => Reminders.Remove(reminder));
            }
            else
            {
                Reminders.Remove(reminder);
            }

            _reminderService.NotifyRemindersChangedForUser(UserSession.UserId ?? 0);
        }

        

        [ObservableProperty]
        private Reminder? _selectedReminder; 

        [RelayCommand]
        public async Task SaveReminder(Reminder reminder)
        {
            
            if (reminder == null) return;
            await SaveReminderAsync(reminder);
        }

        public async Task<string> SaveReminderAsync(Reminder reminder)
        {
            if (reminder == null) return "Error: invalid reminder";

            string result = await _reminderService.SaveReminder(reminder);

            if (result == "Success")
            {
                await LoadReminders();
            }

            return result;
        }

        [ObservableProperty]
        private Reminder? _nextReminder;


        [RelayCommand]
        public async Task LoadReminders()
        {
            
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                int currentId = UserSession.UserId ?? 0;

                if (currentId != 0)
                {
                    var items = (await _reminderService.GetUserReminders(currentId)).ToList();
                    var next = await _reminderService.GetNextReminder(currentId);

                    
                    if (_dispatcher != null)
                    {
                        _dispatcher.TryEnqueue(() =>
                        {
                            Reminders.Clear();
                            foreach (var item in items)
                            {
                                Reminders.Add(item);
                            }

                            NextReminder = next;
                        });
                    }
                    else
                    {
                        
                        Reminders.Clear();
                        foreach (var item in items) Reminders.Add(item);
                        NextReminder = next;
                    }
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
            var newReminder = new Reminder { UserId = UserSession.UserId ?? 0 };
            if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() => SelectedReminder = newReminder);
            }
            else
            {
                SelectedReminder = newReminder;
            }
        }

        [RelayCommand]
        public void EditReminder(Reminder reminder)
        {
            if (reminder == null) return;

            if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() => SelectedReminder = reminder);
            }
            else
            {
                SelectedReminder = reminder;
            }
        }
    }
}