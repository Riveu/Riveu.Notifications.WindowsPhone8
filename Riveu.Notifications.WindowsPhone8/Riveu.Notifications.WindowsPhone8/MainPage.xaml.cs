using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Notification;
using System.Threading.Tasks;

namespace Riveu.Notifications.WindowsPhone8
{
    public partial class MainPage : PhoneApplicationPage
    {
        private string username = "";
        private string password = "";
        private int refreshRate = 0;
        private bool isToastEnabled = false;
        private HttpNotificationChannel pushChannel;
        private string channelName = "RiveuNotificationPushChannel";

        NotificationService.NotificationServiceClient client = new NotificationService.NotificationServiceClient();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

			//Shows the rate reminder message, according to the settings of the RateReminder.
            (App.Current as App).rateReminder.Notify();
            Loaded += MainPage_Loaded;
            client.GetNotificationsCompleted += client_GetNotificationsCompleted;
            client.AuthenticateUserCompleted += client_AuthenticateUserCompleted;
            client.SendNotificationCompleted += client_SendNotificationCompleted;
        }

        void client_AuthenticateUserCompleted(object sender, NotificationService.AuthenticateUserCompletedEventArgs e)
        {
            try
            {
                if (e.Result)
                {
                    client.GetNotificationsAsync(username);
                }
                else
                {
                    MessageBox.Show("Invalid Credentials. Please configure the settings.");
                    radBusyIndicator.IsRunning = false;
                }
            }
            catch
            {
                MessageBox.Show("Unable to connect to the Riveu server. Please verify internet connection and try again.");
                radBusyIndicator.IsRunning = false;
            }
        }

        void client_GetNotificationsCompleted(object sender, NotificationService.GetNotificationsCompletedEventArgs e)
        {
            if (e.Result.Count < 1)
            {
                string[] data = new string[1];
                data[0] = "There are no notifications";
                notificationsListBox.ItemsSource = data;
            }
            else
            {
                notificationsListBox.ItemsSource = e.Result;
            }
            radBusyIndicator.IsRunning = false;
        }

        public static Task<bool> IsConnected()
        {
            var completed = new TaskCompletionSource<bool>();
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error == null && !e.Cancelled)
                {
                    completed.TrySetResult(true);
                }
                else
                {
                    completed.TrySetResult(false);
                }
            };
            client.DownloadStringAsync(new Uri("http://www.google.com/"));
            return completed.Task;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (await IsConnected())
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("Username"))
                {
                    username = IsolatedStorageSettings.ApplicationSettings["Username"].ToString();
                    settingsUsernameTextBox.Text = username;
                }
                if (IsolatedStorageSettings.ApplicationSettings.Contains("Password"))
                {
                    password = IsolatedStorageSettings.ApplicationSettings["Password"].ToString();
                    settingsPasswordTextBox.Password = password;
                }
                if (IsolatedStorageSettings.ApplicationSettings.Contains("RefreshRate"))
                {
                    refreshRate = Convert.ToInt32(IsolatedStorageSettings.ApplicationSettings["RefreshRate"].ToString());
                    settingsRefreshRate.Text = refreshRate.ToString();
                }
                if (IsolatedStorageSettings.ApplicationSettings.Contains("EnableToast"))
                {
                    isToastEnabled = Convert.ToBoolean(IsolatedStorageSettings.ApplicationSettings["EnableToast"].ToString());
                    enableToast.IsChecked = isToastEnabled;
                }
                else
                {
                    MessageBox.Show("This application supports push notifications. Please go to the settings screen and configure your credentials and push notifications.");
                }
                if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                {
                    HandlePushNotification();
                    radBusyIndicator.IsRunning = true;
                    try
                    {
                        client.AuthenticateUserAsync(username, password);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to connect to the Riveu server. Please verify internet connection and try again.");
                        radBusyIndicator.IsRunning = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("This application requires internet access. Please verify internet connection and relaunch the application. The application will now close");
                Application.Current.Terminate();
            }
        }

		/// <summary>
        /// Navigates to about page.
        /// </summary>
        private void GoToAbout(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
        }

        private void createAccountButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/CreateUser.xaml", UriKind.RelativeOrAbsolute));
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            radBusyIndicator.IsRunning = true;
            try
            {
                client.AuthenticateUserAsync(username, password);
            }
            catch
            {
                MessageBox.Show("Unable to connect to the Riveu server. Please verify internet connection and try again.");
                radBusyIndicator.IsRunning = false;
            }
        }

        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = messageTextBox.Text;
                if (!String.IsNullOrEmpty(message))
                {
                    radBusyIndicatorSendNotification.IsRunning = true;
                    client.SendNotificationAsync(username, password, message);
                }
                else
                {
                    MessageBox.Show("Message must be populated to send.");
                }
            }
            catch
            {
                MessageBox.Show("Unable to connect to the Riveu server. Please verify internet connection and try again.");
            }
        }

        void client_SendNotificationCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            radBusyIndicatorSendNotification.IsRunning = false;
            messageTextBox.Text = String.Empty;
            MessageBox.Show("Message sent succesfully");
            radBusyIndicator.IsRunning = true;
            try
            {
                client.AuthenticateUserAsync(username, password);
            }
            catch
            {
                MessageBox.Show("Unable to connect to the Riveu server. Please verify internet connection and try again.");
                radBusyIndicator.IsRunning = false;
            }
        }

        private async void saveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            string settingsUsername = settingsUsernameTextBox.Text;
            string settingsPassword = settingsPasswordTextBox.Password;
            string settingsRefresh = settingsRefreshRate.Text;
            bool enablePush = enableToast.IsChecked;
            int refreshInt = 0;

            if (!String.IsNullOrEmpty(settingsUsername) && !String.IsNullOrEmpty(settingsPassword))
            {
                if (Int32.TryParse(settingsRefresh, out refreshInt))
                {
                    if (refreshInt > 0 && refreshInt < 60)
                    {
                        IsolatedStorageSettings.ApplicationSettings["Username"] = settingsUsername;
                        IsolatedStorageSettings.ApplicationSettings["Password"] = settingsPassword;
                        IsolatedStorageSettings.ApplicationSettings["RefreshRate"] = refreshInt.ToString();
                        IsolatedStorageSettings.ApplicationSettings["EnableToast"] = enablePush.ToString();
                        IsolatedStorageSettings.ApplicationSettings.Save();
                        MessageBox.Show("Settings Saved Successfully");
                        username = settingsUsername;
                        password = settingsPassword;
                        refreshRate = refreshInt;
                        isToastEnabled = enablePush;
                        HandlePushNotification();
                        client.AuthenticateUserAsync(username, password);
                    }
                    else
                    {
                        MessageBox.Show("Refresh Rate must be between 1 and 59");
                    }
                    
                }
                else
                {
                    MessageBox.Show("Refresh Rate must be a number");
                }
            }
            else
            {
                MessageBox.Show("Username and Password are required");
            }
        }

        private void HandlePushNotification()
        {
            pushChannel = HttpNotificationChannel.Find(channelName);
            if (pushChannel == null)
            {
                pushChannel = new HttpNotificationChannel(channelName);
                pushChannel.ChannelUriUpdated += pushChannel_ChannelUriUpdated;
                pushChannel.ShellToastNotificationReceived += pushChannel_ShellToastNotificationReceived;
                pushChannel.Open();
                pushChannel.BindToShellToast();
            }
            else
            {
                pushChannel.ChannelUriUpdated += pushChannel_ChannelUriUpdated;
                pushChannel.ShellToastNotificationReceived += pushChannel_ShellToastNotificationReceived;
            }
            if (!isToastEnabled)
            {
                pushChannel.UnbindToShellToast();
                pushChannel.Close();
                ReportSubscriber(false);
            }
            else
            {
                ReportSubscriber(true);
            }
        }

        void pushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            radBusyIndicator.IsRunning = true;
            try
            {
                client.AuthenticateUserAsync(username, password);
            }
            catch
            {
                radBusyIndicator.IsRunning = false;
            }
        }

        void pushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            ReportSubscriber(true);
        }

        private void ReportSubscriber(bool register)
        {
            
            byte[] myDeviceID = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");
            string DeviceIDAsString = Convert.ToBase64String(myDeviceID);
            string deviceType = "WP8";
            if (register)
            {
                string uri = pushChannel.ChannelUri.ToString();
                client.RegisterSubscriberAsync(username, password, uri, deviceType, DeviceIDAsString);
            }
            else
            {
                client.UnregisterSubscriberAsync(username, password, deviceType, DeviceIDAsString);
            }
        }
    }
}
