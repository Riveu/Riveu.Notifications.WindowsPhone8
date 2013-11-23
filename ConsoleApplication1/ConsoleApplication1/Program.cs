using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string uri = "http://sn1.notify.live.net/throttledthirdparty/01.00/AQHcU-6NEoHhTotYLugdIer-AgAAAAADAQAAAAQUZm52OkQ2Njc0RDFBNkYyRUMyNEEFBlVTU0MwMQ";
            string message = "toast test";
            HttpWebRequest sendNotificationRequest = (HttpWebRequest)WebRequest.Create(uri);

            // Create an HTTPWebRequest that posts the toast notification to the Microsoft Push Notification Service.
            // HTTP POST is the only method allowed to send the notification.
            sendNotificationRequest.Method = "POST";

            // The optional custom header X-MessageID uniquely identifies a notification message. 
            // If it is present, the same value is returned in the notification response. It must be a string that contains a UUID.
            // sendNotificationRequest.Headers.Add("X-MessageID", "<UUID>");

            // Create the toast message.
            string toastMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<wp:Notification xmlns:wp=\"WPNotification\">" +
               "<wp:Toast>" +
                    "<wp:Text1>Riveu</wp:Text1>" +
                    "<wp:Text2>" + message + "</wp:Text2>" +
                    "<wp:Param>/MainPage.xaml</wp:Param>" +
               "</wp:Toast> " +
            "</wp:Notification>";

            // Set the notification payload to send.
            byte[] notificationMessage = Encoding.Default.GetBytes(toastMessage);

            // Set the web request content length.
            sendNotificationRequest.ContentLength = notificationMessage.Length;
            sendNotificationRequest.ContentType = "text/xml";
            sendNotificationRequest.Headers.Add("X-WindowsPhone-Target", "toast");
            sendNotificationRequest.Headers.Add("X-NotificationClass", "2");


            using (Stream requestStream = sendNotificationRequest.GetRequestStream())
            {
                requestStream.Write(notificationMessage, 0, notificationMessage.Length);
            }

        }
    }
}
