using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoGram.Services;
using AutoGram.Task;
using MahApps.Metro.Controls.Dialogs;

namespace AutoGram
{
    public partial class MainWindow
    {
        private bool _closeMe;
        private string _title;

        private static int _programStartUnixTime;

        public MainWindow()
        {
            InitializeComponent();

            UIManager.Init(AccountsManagerTabs, WorkPlace);

            if (Settings.IsAdvanced && Settings.Advanced.WorkersSettings.UseAdvancedSettings)
            {
                foreach (var workerSettings in Settings.Advanced.WorkersSettings.Workers)
                {
                    UIManager.Add(workerSettings);
                }

                UIManager.SelectFirst();
            }
            else
            {
                // Add Default Values
                UIManager.Add(Settings.Basic.UI.Workers);
                UpdateWorkers();
            }

            this.SetCurrentValue(BorderThicknessProperty, new Thickness(0));
            this.SetCurrentValue(BorderBrushProperty, null);
            this.SetCurrentValue(GlowBrushProperty, Brushes.Black);
            this.TitlebarHeight = 35;
            this.TitleCharacterCasing = CharacterCasing.Normal;

            if (Settings.Advanced.WorkersSettings.UseAdvancedSettings)
            {
                _title = "Instagram. MultiTask";
            }
            else
            {
                _title = Settings.Basic.Register.RegisterAccounts
                    ? "Instagram. Registration"
                    : Settings.Advanced.Live.Enable
                        ? "Instagram. Live"
                        : "Instagram. Posting";
            }

            if (Settings.Advanced.StoryViewer.Enable)
            {
                _title = "Instagram. Stories";
            }

            if (Settings.Advanced.DirectSender.Enable)
            {
                _title = "Instagram. Direct Sender";
            }

            if (Settings.Advanced.FollowSender.Enable)
            {
                _title = "Instagram. Follow & Sender";
            }

            if (Settings.Advanced.CreatorChildAccounts.Enable)
            {
                _title = "Instagram. Accounts creator";
            }

            this.Title = _title;

            Worker.UpdateStats += Worker_UpdateStats;
            Worker.UpdateAccountsStateEvent += Worker_UpdateAccountsStateEvent;

            _programStartUnixTime = Utils.DateTimeNowTotalSeconds;
        }

        private void Worker_UpdateStats(object sender, EventArgs e)
        {
            string stats = (string)sender;

            var nowTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var activeTime = nowTime - _programStartUnixTime;

            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                this.Title = $"{_title} | {Utils.SecondsToHourMinutes(activeTime)} | {stats}";
            });
        }

        private void Worker_UpdateAccountsStateEvent(object sender, EventArgs e)
        {
            var result = (string)sender;
            var authSuccessfully = result.Split(':')[0];
            var blocked = result.Split(':')[1];

            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                AccountsStateAuthLabel.Content = authSuccessfully;
                AccountsStateBlockedLabel.Content = blocked;
            });
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            UIManager.Add();
            UpdateWorkers();
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            UIManager.Delete();
            UpdateWorkers();
        }

        private void UpdateWorkers()
        {
            Settings.Basic.UI.Workers = Worker.All.Count;
            Settings.Save();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e.Cancel) return;

            // we want manage the closing itself!
            e.Cancel = !this._closeMe;

            // yes we want now really close the window
            if (this._closeMe) return;

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Quit",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = false
            };
            var result = await this.ShowMessageAsync(
                "Quit application?",
                "Sure you want to quit application?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            this._closeMe = result == MessageDialogResult.Affirmative;

            if (this._closeMe)
            {
                if (FollowSender.TemporaryBlackList != null && FollowSender.TemporaryBlackList.Any())
                    UsersDirectBlacklistRepository.AddRange(FollowSender.TemporaryBlackList.ToList());

                if (DirectSender.DirectUsersUpdateList != null && DirectSender.DirectUsersUpdateList.Any())
                    UsersDirectBlacklistRepository.AddRange(DirectSender.DirectUsersUpdateList.ToList());

                StopWork();
                this.Close();
            }
        }

        private void StopWork()
        {
            if (Worker.All.Any(w => w.IsWork && w.Thread != null && w.Thread.IsAlive))
            {
                foreach (var b in Worker.All.Where(w => w.Thread != null && w.Thread.IsAlive))
                {
                    b.Thread.Abort();
                    b.IsWork = false;
                }
            }
        }
    }
}
