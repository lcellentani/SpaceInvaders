using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace dbga
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefabObject;
        [SerializeField]
        private int preloadAmount;

        private int TotalCount
        {
            get
            {
                // Add all the items in the pool to get the total count
                int count = 0;
                count += this.spawned.Count;
                count += this.despawned.Count;
                return count;
            }
        }

        private List<Transform> spawned;
        private List<Transform> despawned;

        private Transform group;

        void Awake()
        {
            group = transform;

            spawned = new List<Transform>();
            despawned = new List<Transform>();
        }

        void Start()
        {
            PreloadInstances();
        }

        public Transform Spawn(Vector3 pos, Quaternion rot, Transform parent)
        {
            Transform inst;
            inst = SpawnInstance(pos, rot);
            if (inst == null) return null;

            if (parent != null)  // User explicitly provided a parent
            {
                inst.parent = parent;
            }
            //else if (!this.dontReparent && inst.parent != this.group)  // Auto organize?
            //{
            //    // If a new instance was created, it won't be grouped
            //    inst.parent = this.group;
            //}

            spawned.Add(inst);

            inst.gameObject.BroadcastMessage("OnSpawned", this, SendMessageOptions.DontRequireReceiver);

            return inst;
        }

        public Transform Spawn(Vector3 pos, Quaternion rot)
        {
            Transform inst = Spawn(pos, rot, null);

            // Can happen if limit was used
            if (inst == null) return null;

            return inst;
        }

        public Transform SpawnNew() { return this.SpawnNew(Vector3.zero, Quaternion.identity); }
        public Transform SpawnNew(Vector3 pos, Quaternion rot)
        {
            // Use the SpawnPool group as the default position and rotation
            if (pos == Vector3.zero) pos = group.position;
            if (rot == Quaternion.identity) rot = group.rotation;

            GameObject instGO = InstantiatePrefab(prefabObject, pos, rot);
            Transform inst = instGO.transform;

            NameInstance(inst);  // Adds the number to the end

            //if (!this.spawnPool.dontReparent)
            {
                // The SpawnPool group is the parent by default
                // This will handle RectTransforms as well
                var worldPositionStays = !(inst is RectTransform);
                inst.SetParent(group, worldPositionStays);
            }

            //if (this.spawnPool.matchPoolScale)
            //    inst.localScale = Vector3.one;

            //if (this.spawnPool.matchPoolLayer)
            //    this.SetRecursively(inst, this.spawnPool.gameObject.layer);

            // Start tracking the new instance
            this.spawned.Add(inst);

            return inst;
        }

        public void Despawn(Transform instance)
        {
            if (spawned.Contains(instance))
            {
                DespawnInstance(instance);
                              
                spawned.Remove(instance);
            }
        }
        
        public void Despawn(Transform instance, Transform parent)
        {
            instance.parent = parent;
            this.Despawn(instance);
        }

        public void DespawnAll()
        {
            var spawned = new List<Transform>(this.spawned);
            for (int i = 0; i < spawned.Count; i++)
            {
                this.Despawn(spawned[i]);
            }
        }

        private void PreloadInstances()
        {
            Transform inst;
            while (TotalCount < preloadAmount)
            {
                inst = SpawnNew();
                DespawnInstance(inst, false);
            }
        }

        private Transform SpawnInstance(Vector3 pos, Quaternion rot)
        {
            Transform inst;

            // If nothing is available, create a new instance
            if (despawned.Count == 0)
            {
                // This will also handle limiting the number of NEW instances
                inst = this.SpawnNew(pos, rot);
            }
            else
            {
                // Switch the instance we are using to the spawned list
                // Use the first item in the list for ease
                inst = despawned[0];
                despawned.RemoveAt(0);
                spawned.Add(inst);

                // Get an instance and set position, rotation and then 
                //   Reactivate the instance and all children
                inst.position = pos;
                inst.rotation = rot;
                inst.gameObject.SetActive(true);
            }

            return inst;
        }

        private bool DespawnInstance(Transform xform)
        {
            return DespawnInstance(xform, true);
        }

        private bool DespawnInstance(Transform xform, bool sendEventMessage)
        {
            // Switch to the despawned list
            spawned.Remove(xform);
            despawned.Add(xform);

            if (sendEventMessage)
                xform.gameObject.BroadcastMessage("OnDespawned", this, SendMessageOptions.DontRequireReceiver);

            // Deactivate the instance and all children
            xform.gameObject.SetActive(false);

            return true;
        }

        private GameObject InstantiatePrefab(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            return Object.Instantiate(prefab, pos, rot) as GameObject;
        }

        private void NameInstance(Transform instance)
        {
            // Rename by appending a number to make debugging easier
            //   ToString() used to pad the number to 3 digits. Hopefully
            //   no one has 1,000+ objects.
            instance.name += (TotalCount + 1).ToString("#000");
        }
    }
}
