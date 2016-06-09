using UnityEngine;
using System.Collections;

namespace dbga
{
    public abstract class BulletEntity : MonoBehaviour
    {
        public abstract Vector3 GetMovingDirection();
    }
}