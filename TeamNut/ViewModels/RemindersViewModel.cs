// <copyright file="RemindersViewModel.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;
namespace TeamNut.ViewModels
{
    /// <summary>
    /// RemindersViewModel.
    /// </summary>
    public partial class RemindersViewModel : ObservableObject, IDisposable
    {
        private bool disposed;

        private readonly IReminderService reminderService;

        private readonly DispatcherQueue? dispatcher;

        private const int InvalidUserId = 0;

        private const string SaveSuccessResult = "Success";

        private const string InvalidReminderResult = "Error: invalid reminder";

        private const string LoadErrorLogFormat = "this.Reminders Load Error: {0}";

        public ObservableCollection<Reminder> Reminders { get; } = new ObservableCollection<Reminder>();

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial Reminder? SelectedReminder { get; set; }

        [ObservableProperty]
        public partial Reminder? NextReminder { get; set; }

        public RemindersViewModel(IReminderService rreminderService)
        {
            this.reminderService = rreminderService;
            try
            {
                this.dispatcher = DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
                this.dispatcher = null;
            }
            this.reminderService.RemindersChanged += this.OnRemindersChanged;
        }

        private async void OnRemindersChanged(object? sender, int userId)
        {
            int currentUserId = UserSession.UserId ?? InvalidUserId;

            if (currentUserId == userId)
            {
                await this.LoadReminders();
            }
        }

        [RelayCommand]
        public async Task DeleteReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }

            await this.reminderService.DeleteReminder(reminder.Id);

            this.EnqueueUI(() => this.Reminders.Remove(reminder));

            this.reminderService.NotifyRemindersChangedForUser(
                UserSession.UserId ?? InvalidUserId);
        }

        [RelayCommand]
        public async Task SaveReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }

            await this.SaveReminderAsync(reminder);
        }

        public async Task<string> SaveReminderAsync(Reminder reminder)
        {
            if (reminder == null)
            {
                return InvalidReminderResult;
            }

            string result = await this.reminderService.SaveReminder(reminder);

            if (result == SaveSuccessResult)
            {
                await this.LoadReminders();
            }

            return result;
        }

        [RelayCommand]
        public async Task LoadReminders()
        {
            if (this.IsBusy)
            {
                return;
            }

            try
            {
                this.IsBusy = true;

                int userId = UserSession.UserId ?? InvalidUserId;
                if (userId == InvalidUserId)
                {
                    return;
                }

                var reminders = (await this.reminderService.GetUserReminders(userId)).ToList();
                var next = await this.reminderService.GetNextReminder(userId);

                this.EnqueueUI(() =>
                {
                    this.Reminders.Clear();
                    foreach (var reminder in reminders)
                    {
                        this.Reminders.Add(reminder);
                    }

                    this.NextReminder = next;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format(LoadErrorLogFormat, ex.Message));
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        [RelayCommand]
        public void PrepareNewReminder()
        {
            var reminder = new Reminder
            {
                UserId = UserSession.UserId ?? InvalidUserId
            };

            this.EnqueueUI(() => this.SelectedReminder = reminder);
        }

        [RelayCommand]
        public void EditReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }

            this.EnqueueUI(() => this.SelectedReminder = reminder);
        }

        private void EnqueueUI(Action action)
        {
            if (this.dispatcher != null)
            {
                this.dispatcher.TryEnqueue(() => action());
            }
            else
            {
                action();
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.reminderService.RemindersChanged -= this.OnRemindersChanged;
                this.disposed = true;
            }
        }
    }
}
