using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;

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
        public async Task LoadReminders()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Reminders.Clear();

                var items = await _reminderService.GetUserReminders();

                foreach (var item in items)
                {
                    Reminders.Add(item);
                }
            }
            finally
            {
                IsBusy = false;
            }
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

        [RelayCommand]
        public void PrepareNewReminder()
        {
            
            SelectedReminder = new Reminder { UserId = 1 }; 
        }
    }
}