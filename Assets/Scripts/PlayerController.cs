using UnityEngine;
using System.Collections;

namespace dbga
{
    public class PlayerController : MonoBehaviour
    {
        public static readonly string DidCollideWithEnemyBullet = "PlayerDidCollideWithEnemyBullet";
        public static readonly string DidCollideWithEnemy = "PlayerDidCollideWithEnemy";

        public float speed = 5.0f;

        [SerializeField]
        private Transform bulletSpawnPoint;
        [SerializeField]
        private Spawner bulletSpawner;
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        [SerializeField]
        private AudioClip fireSound;
        [SerializeField]
        private AudioClip explosionSound;

        private Transform xform;
        private BoxCollider2D shipCollider;
        private Bounds shipBounds;
       
        private AudioSource fireAudioSource;
        private AudioSource explosionAudioSource;

        private Vector3 initialPosition;

        private bool dying = false;

        void Awake()
        {
            xform = transform;
            shipCollider = GetComponentInChildren<BoxCollider2D>();

            SubscribeToNotifications();
        }

        void Start()
        {
            if (shipCollider != null)
            {
                shipBounds = shipCollider.bounds;
            }
            if (bulletSpawnPoint == null)
            {
                bulletSpawnPoint = xform;
            }

            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.playOnAwake = false;

            explosionAudioSource = gameObject.AddComponent<AudioSource>();
            explosionAudioSource.playOnAwake = false;

            initialPosition = xform.position;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!dying)
            {
                EnemyBulletEntity bullet = other.gameObject.GetComponentInParent<EnemyBulletEntity>();
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                if (bullet != null)
                {
                    StopCoroutine("DestroySelf");
                    StartCoroutine(DestroySelf());
                }
                else if (enemy != null)
                {
                    NotificationCenter.DefaultCenter.PublishNotification(new Notification(this, DidCollideWithEnemy));
                }
            }
        }

        public void Reset()
        {
            dying = false;
            xform.position = initialPosition;

            spriteRenderer.enabled = true;
        }

        public void UpdateLogic()
        {
            if (CanInteract())
            {
                UpdatePosition();

                CheckEnvironmentBounds();

#if UNITY_EDITOR
                if (Input.GetKeyDown(KeyCode.Space))
#else
                if (Input.GetButtonDown("Fire1"))
#endif
                {
                    FireBullet();
                }
            }
        }

        private bool CanInteract()
        {
            return !dying;
        }

        private void SubscribeToNotifications()
        {
            NotificationCenter.DefaultCenter.AddSubscriber(BulletController.DidWentOutOfScreen, this, "BulletDidWentOutOfScreenEvent");
        }

        private void UpdatePosition()
        {
            float direction = Input.GetAxis("Horizontal");
            float dx = speed * Time.deltaTime * direction;

            xform.position += xform.right * dx;
        }

        private void CheckEnvironmentBounds()
        {
            float screenHalfWidth = WorldBoundsProxy.SharedInstance.ScreenHalfWidth;

            Vector3 position = xform.position;
            float posX = position.x;
            float halfWidth = shipBounds.extents.x;

            if (posX - halfWidth < -screenHalfWidth)
            {
                posX = -screenHalfWidth + halfWidth;
            }
            else if (posX + halfWidth > screenHalfWidth)
            {
                posX = screenHalfWidth - halfWidth;
            }

            position.x = posX;

            xform.position = position;
        }

        private void FireBullet()
        {
            Transform inst = bulletSpawner.Spawn(bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            if (inst != null)
            {
                fireAudioSource.PlayOneShot(fireSound);
            }
        }

        private void BulletDidWentOutOfScreenEvent(Notification notification)
        {
            GameObject bulletGO = notification.Publisher.gameObject;
            if (bulletGO != null && bulletSpawner != null)
            {
                bulletSpawner.Despawn(bulletGO.transform);
            }
        }

        private IEnumerator DestroySelf()
        {
            dying = true;

            if (explosionAudioSource != null)
            {
                explosionAudioSource.PlayOneShot(explosionSound);
            }

            spriteRenderer.enabled = false;

            int count = 5;
            do
            {
                count--;
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.2f);
            } while (count > 0);

            spriteRenderer.enabled = false;

            NotificationCenter.DefaultCenter.PublishNotification(new Notification(this, DidCollideWithEnemyBullet));

            Reset();
        }
    }
}