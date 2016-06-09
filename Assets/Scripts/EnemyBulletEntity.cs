using UnityEngine;
using System.Collections;

namespace dbga
{
    public class EnemyBulletEntity : BulletEntity
    {
        private Vector3 movingDirection;

        void Start()
        {
            movingDirection = -transform.up;
        }

        public override Vector3 GetMovingDirection()
        {
            return movingDirection;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "BaseLine")
            {
                gameObject.SendMessageUpwards("DidCollideWithBaseLine", null, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
