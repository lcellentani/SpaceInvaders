using UnityEngine;

namespace dbga
{
    public class NotificationSubscriptionInfo
    {
        public Component Subscriber { get; set; }

        public string MessageName { get; set; }


        public NotificationSubscriptionInfo(Component subscriber, string messageName)
        {
            Subscriber = subscriber;
            MessageName = messageName;
        }
    }
}
