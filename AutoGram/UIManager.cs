using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoGram.Storage.Model;


namespace AutoGram
{
    static class UIManager
    {
        private static TabControl _uiAccountManager;
        private static Label _accountsState;
        private static Label _proxiesState;

        public static void Init(TabControl accountManager, Grid workPlace)
        {
            if (_uiAccountManager == null)
                _uiAccountManager = accountManager;

            _accountsState = CreateLabel(new Thickness(589, 293, 0, 0), "Accounts Ended");
            _proxiesState = CreateLabel(new Thickness(589, 312, 0, 0), "Proxies Ended");

            workPlace.Children.Add(_accountsState);
            workPlace.Children.Add(_proxiesState);
        }

        private static Label CreateLabel(Thickness marginThickness, string content = "")
        {
            Label label = new Label
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = marginThickness,
                Visibility = Visibility.Hidden,
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };

            return label;
        }

        public static void AccountsEnded(bool state)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
           {
               Visibility visibility = state ? Visibility.Visible : Visibility.Hidden;
               _accountsState.Visibility = visibility;
           });
        }

        public static void ProxiesEnded(bool state)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                Visibility visibility = state ? Visibility.Visible : Visibility.Hidden;
                _proxiesState.Visibility = visibility;
            });
        }

        public static void Add(WorkerSettings workerSettings = null)
        {
            var allCount = Worker.All.Count;

            var num = allCount > 0 ? Worker.All.Last().Id + 1 : 1;

            UIAccount uiAccount = new UIAccount();

            Worker worker = new Worker(uiAccount, num, workerSettings);
            TabItem tabItem = new TabItem();

            tabItem.Header = workerSettings != null ? workerSettings.Name : $"{num}";

            var paddingLeft = allCount < 1 ? 5 : 15;
            tabItem.Padding = new Thickness(paddingLeft, 0, 15, 5);
            tabItem.Content = uiAccount.GetContent();

            _uiAccountManager.Items.Add(tabItem);
            _uiAccountManager.SelectedIndex = _uiAccountManager.Items.Count - 1;

            Worker.All.Add(worker);
        }

        public static void SelectFirst()
        {
            _uiAccountManager.SelectedIndex = 0;
        }

        public static void Add(int count)
        {
            for (var i = 0; i < count; i++)
                Add();

            _uiAccountManager.SelectedIndex = 0;
        }

        public static void Delete()
        {
            // Minimum one worker
            if (Worker.All.Count < 2 || Worker.All.All(w => w.IsWork)) return;

            // Find worker who dont work
            Worker lazyWorker = Worker.All.FindLast(w => w.IsWork == false);
            var indexWorker = Worker.All.IndexOf(lazyWorker);

            // Destroy worker and delete him from list
            lazyWorker.Destroy();
            Worker.All.RemoveAt(indexWorker);

            // And from UIManager
            _uiAccountManager.Items.RemoveAt(indexWorker);
        }
    }
}
