using UnityEngine;
using System.Collections;

namespace dbga
{
    public class UnitsRowCoordinator : MonoBehaviour
    {
        private Transform xform;
        private GameObject[] units;
        private Bounds unitBounds;

        private Enemy[] enemies;

        private float height = 0.0f;

        private float leftPositionThreshold;
        private float rightPositionThrehold;
        private float movementDirection = -1.0f;

        void Awake()
        {
            xform = transform;
        }

        public float GetHeight()
        {
            return height;
        }

        public void Setup(GameObject unitPrefab, int unitsCount, float leftThreshold, float rightThreshold)
        {
            if (unitsCount > 0)
            {
                units = new GameObject[unitsCount];
                enemies = new Enemy[unitsCount];
                for (int i = 0; i < unitsCount; i++)
                {
                    GameObject go = Object.Instantiate(unitPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    if (go != null)
                    {
                        go.transform.parent = xform;
                        Enemy enemy = go.GetComponent<Enemy>();
                        {
                            enemy.Setup();
                            enemies[i] = enemy;
                        }
                        units[i] = go;
                    }
                }

                GameObject unitGO = units[0];
                BoxCollider2D unitCollider = unitGO.GetComponent<BoxCollider2D>();
                if (unitCollider != null)
                {
                    unitBounds = unitCollider.bounds;
                    height = unitBounds.size.y;
                }
            }

            leftPositionThreshold = leftThreshold;
            rightPositionThrehold = rightThreshold;
        }

        public void Relayout(float baseY, float spacing)
        {
            xform.position += new Vector3(0.0f, baseY, 0.0f);

            int unitsCount = units.Length;
            GameObject unitGO = units[0];

            float unitWidth = unitBounds.size.x;
            float unitsOverallWidth = ((unitWidth + spacing) * unitsCount);
            float initialX = -(unitsOverallWidth / 2.0f) + ((unitWidth + spacing) / 2.0f);
            Vector3 position = transform.position + new Vector3(initialX, 0.0f, 0.0f);

            for (int i = 0; i < unitsCount; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy != null)
                {
                    enemy.Relayout(position);
                }
                position.x += unitWidth + spacing;
            }

            int first = 0;
            int last = unitsCount - 1;
            float minX = enemies[first].transform.position.x - (unitWidth * 0.5f);
            float maxX = enemies[last].transform.position.x + (unitWidth * 0.5f);
        }

        public void Shoot(Spawner bulletSpawner)
        {
            int unitsCount = units.Length;
            int index = Random.Range(0, unitsCount - 1);
            if (index >= 0 && index < unitsCount)
            {
                enemies[index].Shoot(bulletSpawner);
            }
        }

        public void DespawnAll()
        {
            int unitsCount = units.Length;
            for (int i = 0; i < unitsCount; i++)
            {
                enemies[i].Despawn();
            }
        }

        public float minX;
        public float maxX;

        public bool UpdatePosition()
        {
            bool result = false;

            int unitsCount = units.Length;
            int first = GetFirstAvailableUnit();
            int last = GetLastAvailableUnit();
            if (first >= 0 || last >= 0)
            {
                Vector3 position = xform.position;

                position.x += movementDirection * 10.0f;

                float unitWidth = unitBounds.size.x;
                minX = enemies[first].transform.position.x - (unitWidth * 0.5f);
                maxX = enemies[last].transform.position.x + (unitWidth * 0.5f);

                if ((movementDirection < 0 && minX <= leftPositionThreshold) || (movementDirection > 0 && maxX >= rightPositionThrehold))
                {
                    position.x = xform.position.x;
                    
                    result = true;
                }

                xform.position = position;

                foreach (Enemy e in enemies)
                {
                    e.UpdateFrame();
                }
            }

            return result;
        }

        public void UpdateMovementDirection(float offsetY)
        {
            Vector3 position = xform.position;
            position.y -= offsetY;
            xform.position = position;
            movementDirection = -movementDirection;
        }

        private int GetFirstAvailableUnit()
        {
            int index = -1;
            int unitsCount = units.Length;
            for (int i = 0; i < unitsCount; i++)
            {
                if (enemies[i].IsAvailable())
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private int GetLastAvailableUnit()
        {
            int index = -1;
            int unitsCount = units.Length;
            for (int i = unitsCount - 1; i >= 0 ; i--)
            {
                if (enemies[i].IsAvailable())
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}
