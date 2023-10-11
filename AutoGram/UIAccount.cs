using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoGram
{
    class UIAccount : IEquatable<UIAccount>
    {
        public Button SkipButton;
        public Button ControllerButton;
        public CheckBox LiteModeCheckBox;

        public bool Skip;

        private readonly Label _nameLabel;
        private readonly Label _accountsExecuted;
        private readonly Label _followersLabel;
        private readonly Label _directSource;
        private readonly Image _profileImage;
        private readonly RichTextBox _log;

        private static Uri _defaultImage = new Uri(@"/Instagram;component/Images/profilePhoto.jpg", UriKind.Relative);

        public void ControllerButtonState(bool state)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                ControllerButton.Content = state ? "Stop" : "Start";
            });
        }

        public UIAccount()
        {
            this._nameLabel = CreateLabel(new Thickness(10, 162, 0, 0));
            this._accountsExecuted = CreateLabel(new Thickness(73, 212, 0, 0));
            this._followersLabel = CreateLabel(new Thickness(73, 187, 0, 0));
            this._directSource = CreateLabel(new Thickness(10, 237, 0, 0));

            this._profileImage = CreateImage(_defaultImage,
                new Thickness(10, 10, 0, 0), 147, 146);
            this.ControllerButton = CreateButton(new Thickness(161, 242, 0, 0), "Start");

            this._log = CreateLog(new Thickness(161, 10, 0, 0), 400, 227);

            // Default values
            UpdateFollowersCount();
            UpdateAccountCount();
            SetName();
        }

        public void ClearInfo()
        {
            SetName();
            UpdateFollowersCount();
            UpdateProfileImage(_defaultImage);
        }

        public void EnableSkipButton(bool state)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
           {
               SkipButton.IsEnabled = state;
           });
        }

        public void UpdateDirectSource(string source)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                _directSource.Content = $"# {source}";
            });
        }

        public void WriteLog(string message)
        {
            try
            {
                Application.Current.Dispatcher.Invoke((Action)delegate ()
                {
                    if (_log.Document.Blocks.Count > Variables.LogMaxSize)
                    {
                        _log.Document.Blocks.Clear();
                    }

                    var para = new Paragraph { Margin = new Thickness(0) };
                    para.Inlines.Add("[" + DateTime.Now.ToLongTimeString() + "]: ");
                    para.Inlines.Add(message);

                    _log.Document.Blocks.Add(para);
                    _log.ScrollToEnd();
                });
            }
            catch (Exception e) { }
        }

        public void UpdateProfileImage(string imageUri)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                _profileImage.Source = new BitmapImage(new Uri(imageUri, UriKind.Absolute));
            });
        }

        public void UpdateProfileImage(Uri imageUri)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                _profileImage.Source = new BitmapImage(imageUri);
            });
        }

        public void UpdateAccountCount(int count = 0)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                var countStr = count.ToString();
                _accountsExecuted.Content = countStr;
            });
        }

        public void UpdateFollowersCount(string count = "0")
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                _followersLabel.Content = count;
            });
        }

        public void SetName(string name = "Undefined Name")
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                _nameLabel.Content = name;
            });
        }

        public void SetLink(string link)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                _profileImage.Tag = "https://instagram.com/" + link;
            });
        }

        public Grid GetContent()
        {
            Grid grid = new Grid { Background = Brushes.WhiteSmoke };

            grid.Children.Add(_nameLabel);
            grid.Children.Add(CreateLabel(new Thickness(10, 187, 0, 0), "Followers:"));
            grid.Children.Add(CreateLabel(new Thickness(10, 212, 0, 0), "Accounts:"));
            grid.Children.Add(_accountsExecuted);
            grid.Children.Add(_followersLabel);
            grid.Children.Add(_directSource);
            grid.Children.Add(_profileImage);
            grid.Children.Add(ControllerButton);
            grid.Children.Add(_log);

            return grid;
        }

        private static RichTextBox CreateLog(Thickness marginThickness, int width, int height)
        {
            RichTextBox richTextBox = new RichTextBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = marginThickness,
                Width = width,
                Height = height,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                IsReadOnly = true
            };

            richTextBox.Document.Blocks.Clear();
            richTextBox.IsDocumentEnabled = true;

            return richTextBox;
        }

        private static Label CreateLabel(Thickness marginThickness, string content = "")
        {
            Label label = new Label
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = marginThickness
            };

            return label;
        }

        private static CheckBox CreateCheckBox(Thickness marginThickness, string content, bool isChecked = false)
        {
            CheckBox checkBox = new CheckBox
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = marginThickness,
                IsChecked = isChecked
            };

            return checkBox;
        }

        private static Image CreateImage(Uri imageUri, Thickness marginThickness, int width, int height)
        {
            Image image = new Image()
            {
                Width = width,
                Height = height,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = marginThickness,
                Source = new BitmapImage(imageUri)
            };

            image.MouseDown += new MouseButtonEventHandler(OpenProfileInBrowser);

            return image;
        }

        private static Button CreateButton(Thickness marginThickness, string name = "")
        {
            Button button = new Button
            {
                Content = name,
                Width = 90,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = marginThickness
            };

            return button;
        }

        /* Event Handlers */

        private static void OpenProfileInBrowser(object sender, EventArgs e)
        {
            Image profileImage = sender as Image;
            if (profileImage?.Tag != null)
                Process.Start(profileImage.Tag.ToString());
        }

        public bool Equals(UIAccount other)
        {
            return other != null && this._nameLabel.Content == other._nameLabel.Content;
        }
    }
}
