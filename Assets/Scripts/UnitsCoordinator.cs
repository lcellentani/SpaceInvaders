using UnityEngine;
using System.Collections;

namespace dbga
{
    [System.Serializable]
    public class RowConfig
    {
        public GameObject enemyPrefab;
        public int enemiesCount;
        public float spacing;
    }

    public class UnitsCoordinator : MonoBehaviour
    {
        [SerializeField]
        private RowConfig[] rows;

        [SerializeField]
        private int interline = 4;

        [SerializeField]
        [Range(0, 100)]
        private int screenThreshold = 15;

        [SerializeField]
        private Spawner enemyBulletSpawner;
        [SerializeField]
        private float shootDelay = 1.0f;
        [SerializeField]
        private float movementInitialDelay = 1.0f;
        [SerializeField]
        private float movementMinimumDelay = 0.05f;
        [SerializeField]
        private float movementDecrementDelay = 0.1f;
        [SerializeField]
        private float movementOffsetY = 20.0f;

        private float shootTimer = 0.0f;
        private float movementDelay;
        private float movementTimer = 0.0f;

        private Transform xform;
        private UnitsRowCoordinator[] unitsRows;

        private int numberOfEnemies;
        public int NumberOfEnemies
        {
            get { return numberOfEnemies; }
        }

#if UNITY_EDITOR
        [SerializeField]
        private Transform leftBorder;
        [SerializeField]
        private Transform rightBorder;
#endif

        void Awake()
        {
            xform = transform;
        }

        public void Setup()
        {
            AllocateUnits();
        }

        public void Reset()
        {
            LayoutUnits();

            movementDelay = movementInitialDelay;
            movementTimer = 0.0f;
            shootTimer = 0.0f;
        }

        public void UpdateMovementDelay()
        {
            movementDelay -= movementDecrementDelay;
            if (movementDelay < movementMinimumDelay)
            {
                movementDelay = movementMinimumDelay;
            }
        }

        public void UpadeLogic()
        {
            movementTimer -= Time.deltaTime;
            if (movementTimer <= 0)
            {
                movementTimer += movementDelay;
                UpdateUnitsPositions();
            }

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                shootTimer += shootDelay;
                ExecuteEnemyShooting();
            }
        }

        public void DespawnAll()
        {
            for (int i = 0; i < unitsRows.Length; i++)
            {
                unitsRows[i].DespawnAll();
            }
        }

        private void AllocateUnits()
        {
            if (rows != null)
            {
                float positionThreshold = screenThreshold * 0.01f;
                float screenWidth = WorldBoundsProxy.SharedInstance.ScreeWidth;
                float halfScreenWidth = screenWidth * 0.5f;
                float offset = screenWidth * positionThreshold;

                float leftPositionThreshold = -halfScreenWidth + (offset * 0.5f);
                float rightPositionThrehold = halfScreenWidth - (offset * 0.5f);

#if UNITY_EDITOR
                if (leftBorder != null)
                {
                    Vector3 position = leftBorder.position;
                    position.x = leftPositionThreshold;
                    leftBorder.position = position;
                }
                if (rightBorder != null)
                {
                    Vector3 position = rightBorder.position;
                    position.x = rightPositionThrehold;
                    rightBorder.position = position;
                }
#endif

                unitsRows = new UnitsRowCoordinator[rows.Length];
                for (int i = 0; i < rows.Length; i++)
                {
                    GameObject go = new GameObject();
                    if (go != null)
                    {
                        go.name = string.Format("row{0}", (i + 1));
                        go.transform.parent = xform;

                        unitsRows[i] = go.AddComponent<UnitsRowCoordinator>();
                        unitsRows[i].Setup(rows[i].enemyPrefab, rows[i].enemiesCount, leftPositionThreshold, rightPositionThrehold);
                    }
                }
            }
        }

        private void LayoutUnits()
        {
            if (unitsRows != null)
            {
                numberOfEnemies = 0;

                float baseY = transform.position.y;
                for(int i = 0; i < unitsRows.Length; i++)
                {
                    unitsRows[i].transform.position = Vector3.zero;
                    unitsRows[i].Relayout(baseY, rows[i].spacing);
                    baseY += unitsRows[i].GetHeight() + interline;

                    numberOfEnemies += rows[i].enemiesCount;
                }
            }
        }

        private void ExecuteEnemyShooting()
        {
            for (int i = 0; i < unitsRows.Length; i++)
            {
                unitsRows[i].Shoot(enemyBulletSpawner);
            }
        }

        private void UpdateUnitsPositions()
        {
            bool sideCollision = false;
            for (int i = 0; i < unitsRows.Length; i++)
            {
                sideCollision |= unitsRows[i].UpdatePosition();
            }

            if (sideCollision)
            {
                for (int i = 0; i < unitsRows.Length; i++)
                {
                    unitsRows[i].UpdateMovementDirection(movementOffsetY);
                }
            }
        }
    }
}
