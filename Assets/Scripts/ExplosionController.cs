using UnityEngine;
using System.Collections;

namespace dbga
{
    public class ExplosionController : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] frames = null;

        [SerializeField]
        private float duration = 1.0f; // in seconds

        private float frameDelay;
        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
            }
        }

        private void PerformExplosion(Spawner spawner)
        {
            if (frames != null)
            {
                StopCoroutine("ExecuteExplosion");
                StartCoroutine(ExecuteExplosion(spawner));
            }
        }

        private IEnumerator ExecuteExplosion(Spawner spawner)
        {
            frameDelay = duration / frames.Length;
            bool completed = false;
            int currentFrame = 0;
            int numberOfFrames = frames.Length;
            do
            {
                if (currentFrame < numberOfFrames)
                {
                    spriteRenderer.sprite = frames[currentFrame];
                }
                currentFrame++;
                completed = currentFrame > numberOfFrames;
                spriteRenderer.enabled = !completed;
                //Debug.Log("currentFrame = " + currentFrame + " - completed = " + completed + " - frameDelay = " + frameDelay);
                yield return new WaitForSeconds(frameDelay);
            } while (!completed);

            if (spawner != null)
            {
                spawner.Despawn(this.transform);
            }
        }

        private void OnSpawned(Spawner spawner)
        {
            PerformExplosion(spawner);
        }
    }
}
