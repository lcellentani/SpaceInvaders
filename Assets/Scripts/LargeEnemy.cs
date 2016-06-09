using UnityEngine;
using System.Collections;

namespace dbga
{
    public class LargeEnemy : Enemy
    {
        [SerializeField]
        private int scoreValue = 0;

        public override int GetScoreValue()
        {
            return scoreValue;
        }
    }
}
