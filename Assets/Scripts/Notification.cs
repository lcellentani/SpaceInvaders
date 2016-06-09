using UnityEngine;

namespace dbga
{
    public class Notification
    {
        private Component publisher;
        public Component Publisher
        {
            get { return publisher; }
        }

        private string name;
        public string Name
        {
            get { return name; }
        }

        public Notification(Component publisher, string name)
        {
            this.publisher = publisher;
            this.name = name;
        }
    }
}