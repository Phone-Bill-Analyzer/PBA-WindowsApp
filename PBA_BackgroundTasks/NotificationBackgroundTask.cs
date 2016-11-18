using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;

namespace PBA_BackgroundTasks
{
    public sealed class NotificationBackgroundTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            RawNotification notification = (RawNotification)taskInstance.TriggerDetails;

            _deferral = taskInstance.GetDeferral();

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            XDocument xdoc = XDocument.Parse(notification.Content);
            string task = xdoc.Root.Element("Task").Value;

            if (task.Equals("ShowInfoMessage"))
            {
                string mid = xdoc.Root.Element("MessageID").Value;
                string title = xdoc.Root.Element("Title").Value;
                string content = xdoc.Root.Element("Content").Value;

                var last_mid = localSettings.Values["ToastMessageID"];

                if (last_mid != null && last_mid.Equals(mid))
                {
                    return;
                }

                localSettings.Values["ToastMessageID"] = mid;

                showInfoMessage(title, content);
            }
        }

        private void showToastNotification(string title, string content)
        {
            // Get Template
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;

            // Put text in the template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(title));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(content));

            // Set the Duration
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            // Create Toast and show
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private void showInfoMessage(string title, string content)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["ToastMessageTitle"] = title;
            localSettings.Values["ToastMessageContent"] = content;

            // Get Template
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;

            // Put text in the template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(title));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(content));

            // Set the Duration
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            // Show custom Text
            var toastNavigationUriString = "ShowInfoMessage";
            XmlElement toastElement = ((XmlElement)toastXml.SelectSingleNode("/toast"));
            toastElement.SetAttribute("launch", toastNavigationUriString);

            // Create Toast and show
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);

        }
    }
}