using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AutoGram
{
    public partial class PhoneVerificationWindow
    {
        public string VerificationCode { get; private set; }
        public string PhoneNumber { get; private set; }
        public bool IsCancel { get; private set; }

        private string _profileUrl;

        public PhoneVerificationWindow(string phoneNumber = null)
        {
            InitializeComponent();

            this.SetCurrentValue(BorderThicknessProperty, new Thickness(0));
            this.SetCurrentValue(BorderBrushProperty, null);
            this.SetCurrentValue(GlowBrushProperty, Brushes.Black);
            this.TitlebarHeight = 35;
            this.TitleCharacterCasing = CharacterCasing.Normal;

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                PhoneNumberTextBox.Text = phoneNumber;

                PhoneNumberTextBox.IsEnabled = false;
                AddPhoneNumberButton.IsEnabled = false;

                VerificationCodeTextBox.IsEnabled = true;
                VerifyButton.IsEnabled = true;
            }
        }

        private void AddPhoneNumberButton_Click(object sender, RoutedEventArgs e)
        {
            this.PhoneNumber = PhoneNumberTextBox.Text;
            this.Close();
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            this.VerificationCode = VerificationCodeTextBox.Text;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsCancel = true;
            this.Close();
        }

        private void BrowseProfileButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(_profileUrl);
        }
    }
}
