using UnityEngine;
using System.Collections;

namespace dbga
{
    public abstract class Enemy : MonoBehaviour
    {
        public static readonly string WillBeDestroyed = "EnemyWillBeDestroyed";

        [SerializeField]
        private Transform bulletSpawnPoint;
        [SerializeField]
        private Sprite[] movementFrames;

        private Transform xform;
        private Vector3 initialPosition;
        private SpriteRenderer spriteRenderer;

        private int currentFrame;
        private int numberOfFrames;

        private Spawner bulletSpawner;

        private bool dead;

        void Awake()
        {
            xform = transform;
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (bulletSpawnPoint == null)
            {
                bulletSpawnPoint = xform;
            }
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerBulletEntity bullet = other.gameObject.GetComponentInParent<PlayerBulletEntity>();
            if (bullet != null)
            {
                other.gameObject.SendMessageUpwards("DidCollideWithEnemy", this, SendMessageOptions.DontRequireReceiver);

                StopCoroutine("DestroySelf");
                StartCoroutine(DestroySelf());
            }
        }

        public void Setup()
        {
            spriteRenderer.enabled = false;
            dead = false;

            currentFrame = 0;
            numberOfFrames = movementFrames.Length;
            UpdateFrame();
        }

        public void Relayout(Vector3 initialPosition)
        {
            StopAllCoroutines();
            this.gameObject.SetActive(true);

            this.spriteRenderer.enabled = true;

            //@note: quando una variabile di classe ha lo stesso nome di un variabile locale o di un parametro di funzione
            //       possiamo usare la keyword this per esplicitare che ci stiamo riferendo alla variabile di classe come
            //       nell'esempio qui sotto.
            this.initialPosition = initialPosition;

            this.xform.position = this.initialPosition;
        }

        public bool Shoot(Spawner bulletSpawner)
        {
            if (!dead)
            {
                bulletSpawner.Spawn(bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                return true;
            }
            return false;
        }

        public void Despawn()
        {
            this.gameObject.SetActive(false);
        }

        public void UpdateFrame()
        {
            if (currentFrame >= 0 && currentFrame < numberOfFrames && movementFrames != null)
            {
                spriteRenderer.sprite = movementFrames[currentFrame];
                currentFrame++;
                if (currentFrame >= numberOfFrames)
                {
                    currentFrame = 0;
                }
            }
        }

        public bool IsAvailable()
        {
            return gameObject.activeSelf && !dead;
        }

        public abstract int GetScoreValue();

        private IEnumerator DestroySelf()
        {
            dead = true;
            NotificationCenter.DefaultCenter.PublishNotification(new Notification(this, WillBeDestroyed));

            yield return new WaitForEndOfFrame();

            this.gameObject.SetActive(false);
        }
    }
}