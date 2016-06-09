using UnityEngine;
using System.Collections;
using System;

namespace dbga
{
    public class PlayerBulletEntity : BulletEntity
    {
        private Vector3 movingDirection;

        void Start()
        {
            movingDirection = transform.up;
        }

        public override Vector3 GetMovingDirection()
        {
            return movingDirection;
        }
    }
}