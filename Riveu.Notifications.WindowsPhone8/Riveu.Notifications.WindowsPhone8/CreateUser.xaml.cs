using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;

namespace Riveu.Notifications.WindowsPhone8
{
    public partial class CreateUser : PhoneApplicationPage
    {
        NotificationService.NotificationServiceClient client = new NotificationService.NotificationServiceClient();
        public CreateUser()
        {
            InitializeComponent();
            client.VerifyUserAccountExistsCompleted += client_VerifyUserAccountExistsCompleted;
            client.RegisterUserCompleted += client_RegisterUserCompleted;
        }

        void client_RegisterUserCompleted(object sender, NotificationService.RegisterUserCompletedEventArgs e)
        {
            bool result = e.Result;
            if (result)
            {
                usernameTextBox.Text = String.Empty;
                passwordTextBox.Password = String.Empty;
                MessageBox.Show("Account created successfully. Please go to settings to use the new account");
            }
            else
            {
                MessageBox.Show("Unable to create account. Please try again.");
            }
        }

        void client_VerifyUserAccountExistsCompleted(object sender, NotificationService.VerifyUserAccountExistsCompletedEventArgs e)
        {
            bool result = e.Result;
            if (result)
            {
                MessageBox.Show("Username already in use. Please try another user account");
            }
            else
            {
                try
                {
                    client.RegisterUserAsync(usernameTextBox.Text, passwordTextBox.Password);
                }
                catch
                {
                    MessageBox.Show("Unable to connect to Riveu Server. Please verify internet connection and try again.");
                }
            }
        }

        private async void submitButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Password;

            if(!String.IsNullOrEmpty(username.Trim()) && !String.IsNullOrEmpty(password.Trim()))
            {
                try
                {
                    client.VerifyUserAccountExistsAsync(username);
                }
                catch
                {
                    MessageBox.Show("Unable to connect to Riveu Server. Please verify internet connection and try again.");
                }
            }
            else
            {
                MessageBox.Show("Username and Password are required to create an account");
            }
        }
    }
}