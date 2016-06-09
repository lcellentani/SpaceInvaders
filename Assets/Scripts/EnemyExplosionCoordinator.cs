using UnityEngine;
using System.Collections;

namespace dbga
{
    public class EnemyExplosionCoordinator : MonoBehaviour
    {
        private Spawner explosionSpawner;

        void Awake()
        {
            explosionSpawner = GetComponent<Spawner>();

            SubscribeToNotifications();
        }

        void Start()
        {
#if UNITY_EDITOR
            if (explosionSpawner == null)
            {
                Debug.LogWarning("Missing Spawner component. Disabling myself");
                this.enabled = false;
                return;
            }
#endif
            
        }

        private void SubscribeToNotifications()
        {
            NotificationCenter.DefaultCenter.AddSubscriber(Enemy.WillBeDestroyed, this, "WillBeDestroyedEvent");
        }

        private void WillBeDestroyedEvent(Notification notification)
        {
            Enemy enemy = (Enemy)notification.Publisher;
            if (enemy != null)
            {
                explosionSpawner.Spawn(enemy.transform.position, Quaternion.identity);
            }
        }
    }
}