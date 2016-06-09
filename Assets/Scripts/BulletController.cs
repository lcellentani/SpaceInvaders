using UnityEngine;
using System.Collections;

namespace dbga
{
    public class BulletController : MonoBehaviour
    {
        public static readonly string DidWentOutOfScreen = "BulletDidWentOutOfScreen";

        [SerializeField]
        private float speed = 300.0f; // pix/sec

        private Transform xform;
        private BoxCollider2D bulletCollider;
        private Bounds bulletBounds;
        private BulletEntity bulletEntity;

        private Spawner parentSpawner;

        void Awake()
        {
            xform = transform;
            bulletCollider = GetComponentInChildren<BoxCollider2D>();
            if (bulletCollider != null)
            {
                bulletBounds = bulletCollider.bounds;
            }
            bulletEntity = GetComponent<BulletEntity>();
        }

#if UNITY_EDITOR
        void Start()
        {
            if (bulletEntity == null)
            {
                Debug.LogWarning("Cannot find any BulletEntiy, disabling myself");
                this.enabled = false;
            }
        }
#endif

        void Update()
        {
            UpdatePosition();

            CheckOutOfScreen();
        }

        private void UpdatePosition()
        {
            float positionIncrement = speed * Time.deltaTime;
            xform.position += bulletEntity.GetMovingDirection() * positionIncrement;
        }

        private void CheckOutOfScreen()
        {
#if UNITY_EDITOR
            if (bulletBounds == null)
            {
                Debug.LogWarning("Cannot find collider, disabling myself");
                this.enabled = false;
                return;
            }
#endif
            float screenHalfHeight = WorldBoundsProxy.SharedInstance.ScreenHalfHeight;

            Vector3 position = xform.position;
            float posY = position.y;
            float halfHeight = bulletBounds.extents.y;
            //@note: safe value to be sure we are really "out of screen"
            float safeHeight = 10.0f;

            if (posY - halfHeight - safeHeight > screenHalfHeight)
            {
                StopCoroutine("DespawnSelf");
                StartCoroutine(DespawnSelf());
            }
        }

        private IEnumerator DespawnSelf()
        {
            yield return new WaitForEndOfFrame();

            if (parentSpawner != null)
            {
                parentSpawner.Despawn(xform);
            }
        }

        private void DidCollideWithBaseLine()
        {
            StopCoroutine("DespawnSelf");
            StartCoroutine(DespawnSelf());
        }

        private void DidCollideWithEnemy(Enemy enemy)
        {
            StopCoroutine("DespawnSelf");
            StartCoroutine(DespawnSelf());
        }

        private void OnSpawned(Spawner spawner)
        {
            parentSpawner = spawner;
        }
    }
}