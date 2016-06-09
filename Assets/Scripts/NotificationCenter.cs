using System.Collections.Generic;
using UnityEngine;

namespace dbga
{
    public class NotificationCenter : MonoBehaviour
    {
        private static NotificationCenter defaultCenter = null;
        public static NotificationCenter DefaultCenter
        {
            get
            {
                if (defaultCenter == null)
                {
                    defaultCenter = GameObject.FindObjectOfType<NotificationCenter>();
                }
                return defaultCenter;
            }
        }
        
        private Dictionary<string, HashSet<NotificationSubscriptionInfo>> subscriptionInfoByNotificationName;
        private Dictionary<string, HashSet<NotificationSubscriptionInfo>> SubscriptionInfoByNotificationName
        {
            get
            {
                if (subscriptionInfoByNotificationName == null)
                {
                    subscriptionInfoByNotificationName = new Dictionary<string, HashSet<NotificationSubscriptionInfo>>();
                }

                return subscriptionInfoByNotificationName;
            }
        }

        private HashSet<NotificationSubscriptionInfo> GetSubscriptionInfoHashSet(string notificationName)
        {
            HashSet<NotificationSubscriptionInfo> suscriptionInfoHashSet = null;
            if (!SubscriptionInfoByNotificationName.TryGetValue(notificationName, out suscriptionInfoHashSet))
            {
                suscriptionInfoHashSet = new HashSet<NotificationSubscriptionInfo>();

                SubscriptionInfoByNotificationName[notificationName] = suscriptionInfoHashSet;
            }

            return suscriptionInfoHashSet;
        }

        public void AddSubscriber(string notificationName, Component subscriber, string messageName)
        {
            if ((subscriber == null) || (messageName == null))
            {
                return;
            }

            //Debug.Log("+[AddSubscriber]: notificationName=" + notificationName + " - subscriber=" + subscriber + " - messageName=" + messageName);

            HashSet<NotificationSubscriptionInfo> subscriptionInfoHashSet = GetSubscriptionInfoHashSet(notificationName);
            subscriptionInfoHashSet.Add(new NotificationSubscriptionInfo(subscriber, messageName));
        }

        public void RemoveSubscriber(string notificationName, Component subscriber, string messageName)
        {
            if ((subscriber == null) || (messageName == null))
            {
                return;
            }

            //Debug.Log("-[RemoveSubscriber]: notificationName=" + notificationName + " - subscriber=" + subscriber + " - messageName=" + messageName);

            HashSet<NotificationSubscriptionInfo> subscriptionInfoHashSet = GetSubscriptionInfoHashSet(notificationName);
            HashSet<NotificationSubscriptionInfo> subscriptionInfoHashSetCopy = new HashSet<NotificationSubscriptionInfo>(subscriptionInfoHashSet);
            foreach (NotificationSubscriptionInfo subscriptionInfo in subscriptionInfoHashSetCopy)
            {
                if (subscriber.Equals(subscriptionInfo.Subscriber)
                    && messageName.Equals(subscriptionInfo.MessageName))
                {
                    subscriptionInfoHashSet.Remove(subscriptionInfo);
                }
            }
        }

        public void PublishNotification(Component publisher, string name)
        {
            PublishNotification(new Notification(publisher, name));
        }

        public void PublishNotification(Notification notification)
        {
            if (notification == null)
            {
                return;
            }

            HashSet<NotificationSubscriptionInfo> subscriptionInfoHashSet = GetSubscriptionInfoHashSet(notification.Name);

            HashSet<NotificationSubscriptionInfo> subscriptionInfoHashSetCopy = new HashSet<NotificationSubscriptionInfo>(subscriptionInfoHashSet);

            foreach (NotificationSubscriptionInfo subscriptionInfo in subscriptionInfoHashSetCopy)
            {
                Component subscriber = subscriptionInfo.Subscriber;

                if (subscriber == null)
                {
                    continue;
                }

                subscriber.SendMessage(subscriptionInfo.MessageName, notification, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}