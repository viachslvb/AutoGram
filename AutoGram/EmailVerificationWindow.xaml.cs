using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoGram
{
    public partial class EmailVerificationWindow : Window
    {
        public string VerificationCode { get; set; }

        public EmailVerificationWindow(Email email)
        {
            InitializeComponent();

            emailTextBox.Text = $"{email.Username}:{email.Password}";
        }

        private void VerifyEmailButton_Click(object sender, RoutedEventArgs e)
        {
            VerificationCode = CodeTextBox.Text;
            this.Close();
        }
    }
}
