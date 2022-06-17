using Mirror;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

#pragma warning disable CS0618, CS0672, CS0649
    public class WorldObjectManager : NetworkBehaviour
    {
        #region Types.
        /// <summary>
        /// Prefabs for WorldObjectTypes.
        /// </summary>
        [System.Serializable]
        public class WorldObjectPrefab
        {
            /// <summary>
            /// 
            /// </summary>
            [Tooltip("ObjectType this prefab is for.")]
            [SerializeField]
            private WorldObjectTypes _objectType = WorldObjectTypes.Unset;
            /// <summary>
            /// ObjectType this prefab is for.
            /// </summary>
            public WorldObjectTypes ObjectType { get { return _objectType; } }
            /// <summary>
            /// 
            /// </summary>
            [Tooltip("Prefab for the ObjectType.")]
            [SerializeField]
            private WorldObject _prefab;
            /// <summary>
            /// Prefab for the ObjectType.
            /// </summary>
            public WorldObject Prefab { get { return _prefab; } }

        }

        /// <summary>
        /// Reasons a WorldObject needs to be updated as dirty or not.
        /// </summary>
        private enum DirtyUpdateReasons
        {
            Updated,
            Instantiated,
            Removed
        }
        /// <summary>
        /// Datas needed to spawn a WorldObject on clients.
        /// </summary>
        public class AddedWorldObjectData
        {
            public AddedWorldObjectData() { }
            public AddedWorldObjectData(Vector3 position, Quaternion rotation, Vector3 scale, WorldObjectData data)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale;  
                Data = data;
            }

            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public readonly WorldObjectData Data;
            public readonly Vector3 Scale;
        }
        #endregion

        #region Public.
        /// <summary>
        /// Singleton reference to this object.
        /// </summary>
        public static WorldObjectManager Instance { get; private set; }
        #endregion

        #region Serialized.
        /// <summary>
        /// Enable to rebuild scene WorldObjects database.
        /// </summary>
        [Tooltip("Enable to rebuild scene WorldObjects database.")]
        [SerializeField]
        private bool _rebuildSceneWorldObjects = false;
        /// <summary>
        /// Prefabs for each WorldObject.
        /// </summary>
        [Tooltip("Prefabs for each WorldObject.")]
        [SerializeField]
        private WorldObjectPrefab[] _prefabs = new WorldObjectPrefab[0];
        #endregion

        #region Private.
        /// <summary>
        /// Next key to apply to WorldObjects.
        /// </summary>
        private uint _nextKey = 0;
        /// <summary>
        /// WorldObjects which currently exist.
        /// </summary>
        private Dictionary<uint, WorldObject> _worldObjects = new Dictionary<uint, WorldObject>();
        /// <summary>
        /// WorldObjects marked as dirty. These items must be sent to new joiners.
        /// </summary>
        private HashSet<WorldObject> _dirty = new HashSet<WorldObject>();
        /// <summary>
        /// Keys which can be reused from spawned WorldObject.
        /// </summary>
        private Stack<uint> _cachedKeys = new Stack<uint>();
        #endregion

        private void Awake()
        {
            FirstInitialize();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            UnityNetworkServer.RelayOnServerAddPlayer += BulkObjectsNetworkManager_RelayOnServerAddPlayer;
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void FirstInitialize()
        {
            Instance = this;
            //Add found WorldObjects into WorldObject collection immediately.
            List<WorldObject> wos = FindSceneWorldObjects();
            for (int i = 0; i < wos.Count; i++)
                _worldObjects[wos[i].ReturnData().Key] = wos[i];

            //Set next key based on how many WorldObjects were added.
            _nextKey = (uint)wos.Count;
        }

        /// <summary>
        /// Finds WorldObjects in the scene.
        /// </summary>
        /// <returns></returns>
        private List<WorldObject> FindSceneWorldObjects()
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            List<WorldObject> results = new List<WorldObject>();
            for (int i = 0; i < roots.Length; i++)
            {
                WorldObject[] WorldObjects = roots[i].GetComponentsInChildren<WorldObject>(true);
                results.AddRange(WorldObjects);
            }

            return results;
        }

        /// <summary>
        /// Received when a client joins the server.
        /// </summary>
        /// <param name="obj"></param>
        private void BulkObjectsNetworkManager_RelayOnServerAddPlayer(NetworkConnection obj)
        {
            if (_dirty.Count == 0)
                return;

            List<AddedWorldObjectData> addedWorldObjects = new List<AddedWorldObjectData>();
            List<WorldObjectData> updatedWorldObjects = new List<WorldObjectData>();

            foreach (WorldObject wo in _dirty)
            {
                //If WorldObject was spawned then send as added WorldObject.
                if (wo.ReturnData().Instantiated)
                    addedWorldObjects.Add(new AddedWorldObjectData(wo.transform.position, wo.transform.rotation, wo.transform.localScale, wo.ReturnData()));
                //Not spawned, then its a scene WorldObject. Send as updated WorldObject.
                else
                    updatedWorldObjects.Add(wo.ReturnData());

            }

            if (addedWorldObjects.Count > 0)
                TargetAddWorldObject(obj, addedWorldObjects.ToArray());
            if (updatedWorldObjects.Count > 0)
                TargetUpdateWorldObject(obj, updatedWorldObjects.ToArray());
        }

        /// <summary>
        /// Instantiates a WorldObject and returns it. Complete synchronization by calling InitializeWorldObject.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        [Server]
        public WorldObject InstantiateWorldObject(WorldObjectTypes objectType, Vector3 position, Quaternion rotation)
        {
            WorldObject prefab = ReturnPrefab(objectType);
            /* To keep the code simple WorldObjects will be added
             * only using position. */
            //Instantiate a new WorldObject using the position passed in from data.
            WorldObject wo = Instantiate(prefab, position, rotation);
            return wo;
        }

        /// <summary>
        /// Initializes an instantiated WorldObject and tells clients of addition.
        /// </summary>
        /// <param name="position">Position to spawn new WorldObject.</param>
        [Server]
        public void InitializeWorldObject(WorldObject wo, WorldObjectTypes objectType)
        {
            if (wo == null || wo.gameObject == null)
            {
                Debug.LogError("You must first InstantiateWorldObject, and use the result within InitializeWorldObject.");
                return;
            }

            WorldObjectData data = wo.ReturnData();
            data.SetKey(ReturnNextKey());
            data.SetObjectType(objectType);
            data.SetInstantiated(true);
            //Update WorldObjects data with newly generated data.
            wo.UpdateData(data);
            //Add to WorldObjects collection.
            _worldObjects[data.Key] = wo;
            //WorldObject was modified so do a dirty update.
            UpdateDirty(wo, DirtyUpdateReasons.Instantiated);
            //Generate new added WorldObject and send to clients.
            AddedWorldObjectData addedWorldObject = new AddedWorldObjectData(wo.transform.position, wo.transform.rotation, wo.transform.localScale, data);
            RpcAddWorldObject(new AddedWorldObjectData[] { addedWorldObject });
        }


        /// <summary>
        /// Adds new WorldObjects.
        /// </summary>
        [ClientRpc]
        private void RpcAddWorldObject(AddedWorldObjectData[] addedDatas)
        {
            //Already ran on server.
            if (base.isServer)
                return;

            for (int i = 0; i < addedDatas.Length; i++)
                ClientAddWorldObject(addedDatas[i]);
        }
        /// <summary>
        /// Adds new WorldObject.
        /// </summary>
        /// <param name="addedDatas"></param>
        [TargetRpc]
        private void TargetAddWorldObject(NetworkConnection conn, AddedWorldObjectData[] addedDatas)
        {
            for (int i = 0; i < addedDatas.Length; i++)
                ClientAddWorldObject(addedDatas[i]);

            
    }
        /// <summary>
        /// Adds a WorldObject for clients.
        /// </summary>
        /// <param name="addedData"></param>
        private void ClientAddWorldObject(AddedWorldObjectData addedData)
        {
            WorldObject prefab = ReturnPrefab(addedData.Data.ObjectType);
            //Instantiate a new WorldObject using the position passed in from data.
            WorldObject wo = Instantiate(prefab, addedData.Position, Quaternion.identity);
            wo.transform.localScale = addedData.Scale;
            //Update newly spawned WorldObject with current date.
            wo.UpdateData(addedData.Data);
            //Add to collection.
            _worldObjects[addedData.Data.Key] = wo;
            
    }

        /// <summary>
        /// Removes a WorldObject from the scene and tells clients it was removed.
        /// </summary>
        /// <param name="wo"></param>
        [Server]
        public void RemoveWorldObject(WorldObject wo)
        {
            WorldObjectData data = wo.ReturnData();

            //WorldObject was modified so do a dirty update.
            UpdateDirty(wo, DirtyUpdateReasons.Removed);

            //If spawned.
            if (data.Instantiated)
            {
                //Only remove from WorldObjects if spawned.
                _worldObjects.Remove(data.Key);
                /* If spawned WorldObject then add key to cache.
                 * Cannot cache WorldObjects placed in editor(not spawned) as
                 * they aren't actually destroyed, and remain
                 * in scene. */
                _cachedKeys.Push(data.Key);
                /* This is a spawned WorldObject so it's safe to destroy;
                * new clients do not need to know about spawned WorldObjects
                * which were destroyed before they joined. */
                Destroy(wo.gameObject);
            }

            RpcRemoveWorldObject(new WorldObjectData[] { data });
        }

        /// <summary>
        /// Removes WorldObjects.
        /// </summary>
        /// <param name="WorldObjects"></param>
        [ClientRpc]
        private void RpcRemoveWorldObject(WorldObjectData[] datas)
        {
            //Already ran on server.
            if (base.isServer)
                return;

            for (int i = 0; i < datas.Length; i++)
                ClientRemoveWorldObject(datas[i]);
        }

        /// <summary>
        /// Removes a WorldObject on clients.
        /// </summary>
        /// <param name="data"></param>
        private void ClientRemoveWorldObject(WorldObjectData data)
        {
            WorldObject wo = ReturnWorldObject(data);
            //If WorldObject was found.
            if (wo != null)
            {
                /* Update WorldObject with new data. This is because
                 * even though the WorldObject is removed, it may have
                 * already existed in the scene thus cannot
                 * be destroyed, but rather hidden. Updating
                 * the WorldObject will hide the WorldObject and if it does
                 * need to be destroyed that will occur after
                 * the update. */
                wo.UpdateData(data);

                //If was spawned then destroy WorldObject.
                if (data.Instantiated)
                    Destroy(wo.gameObject);
            }
            //Not found.
            else
            {
                Debug.LogError("Could not find WorldObject data in WorldObjects collection. This can occur when added or removed WorldObjects are not properly handled on clients.");
            }
        }
        /// <summary>
        /// Updates existing WorldObjects.
        /// </summary>
        /// <param name="wo"></param>
        [Server]
        public void UpdateWorldObject(WorldObject wo)
        {
            //WorldObject was modified so do a dirty update.
            UpdateDirty(wo, DirtyUpdateReasons.Updated);

            /* Presumable the changes were already made on the server.
             * In my example they are, I update the data in my WorldObject
             * then tell the WorldObjectManager that the WorldObject
             * has changed. There is no reason for me to update the
             * WorldObject again on the server. */
            RpcUpdateWorldObject(new WorldObjectData[] { wo.ReturnData() });
        }

        /// <summary>
        /// Updates existing WorldObjects.
        /// </summary>
        /// <param name="WorldObjects"></param>
        [ClientRpc]
        private void RpcUpdateWorldObject(WorldObjectData[] datas)
        {
            //Already ran on server.
            if (base.isServer)
                return;

            for (int i = 0; i < datas.Length; i++)
                ClientUpdateWorldObject(datas[i]);
        }
        /// <summary>
        /// Updates already known WorldObjects.
        /// </summary>
        /// <param name="WorldObjects"></param>
        [TargetRpc]
        private void TargetUpdateWorldObject(NetworkConnection conn, WorldObjectData[] datas)
        {
            for (int i = 0; i < datas.Length; i++)
                ClientUpdateWorldObject(datas[i]);
        }
        /// <summary>
        /// Updates a WorldObject on clients.
        /// </summary>
        /// <param name="data"></param>
        private void ClientUpdateWorldObject(WorldObjectData data)
        {
            WorldObject wo = ReturnWorldObject(data);
            //If WorldObject was found.
            if (wo != null)
                wo.UpdateData(data);
            //Not found.
            else
                Debug.LogError("Could not find WorldObject data in WorldObjects collection. This can occur when added or removed WorldObjects are not properly handled on clients.");
        }

        /// <summary>
        /// Returns WorldObject associated with data.
        /// </summary>
        /// <param name="data"></param>
        private WorldObject ReturnWorldObject(WorldObjectData data)
        {
            WorldObject result;
            _worldObjects.TryGetValue(data.Key, out result);

            return result;
        }


        /// <summary>
        /// Returns the next key to use.
        /// </summary>
        /// <returns></returns>
        private uint ReturnNextKey()
        {
            uint result;
            //Use cache first when possible.
            if (_cachedKeys.Count > 0)
            {
                //Set result to last entry, and remove from end of list for performance.
                result = _cachedKeys.Pop();
            }
            //No cache avialable.
            else
            {
                result = _nextKey;
                _nextKey++;
            }

            return result;
        }

        /// <summary>
        /// Updates dirty collection.
        /// </summary>
        /// <param name="wo"></param>
        /// <param name="reason"></param>
        [Server]
        private void UpdateDirty(WorldObject wo, DirtyUpdateReasons reason)
        {
            //Added
            if (reason == DirtyUpdateReasons.Instantiated)
            {
                //Added WorldObjects are always marked as dirty since they are spawned at runtime.
                _dirty.Add(wo);
            }
            //Removed.
            else if (reason == DirtyUpdateReasons.Removed)
            {
                /* If WorldObject was instantiated then remove from dirty.
                 * Spawned WorldObjects which are removed are destroyed
                 * so there is no reason to track them for late
                 * joiners. */
                if (wo.ReturnData().Instantiated)
                {
                    _dirty.Remove(wo);
                }
                //If wasn't spawned at runtime.
                else
                {
                    //If not default add to dirty.
                    if (wo.ObjectNotDefault())
                        _dirty.Add(wo);
                    //Default.
                    else
                        _dirty.Remove(wo);
                }
            }
            //Updated.
            else if (reason == DirtyUpdateReasons.Updated)
            {
                //If not default add to dirty.
                if (wo.ObjectNotDefault())
                    _dirty.Add(wo);
                //Default, not a dirty object.
                else
                    _dirty.Remove(wo);
            }
        }

        /// <summary>
        /// Returns a prefab for a WorldObjectTypes.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private WorldObject ReturnPrefab(WorldObjectTypes type)
        {
            for (int i = 0; i < _prefabs.Length; i++)
            {
                if (_prefabs[i].ObjectType == type)
                    return _prefabs[i].Prefab;
            }

            //Fall through, nothing found.
            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            /* If to rebuild scene WorldObjects then all WorldObjects in the scene of this object
             * will be added to the WorldObjects list. */
            if (_rebuildSceneWorldObjects)
            {
                //Only check if not playing.
                if (Application.isPlaying)
                {
                    Debug.LogError("Cannot rebuild WorldObject while in play mode.");
                }
                else
                //Not playing.
                {
                    int serializedCount = 0;
                    //Reset key.
                    _nextKey = 0;

                    _rebuildSceneWorldObjects = false;
                    List<WorldObject> wo = FindSceneWorldObjects();
                    for (int i = 0; i < wo.Count; i++)
                    {
                        wo[i].ReturnData().SetKey(ReturnNextKey());
                        EditorUtility.SetDirty(wo[i]);
                        serializedCount++;
                    }

                    Debug.Log("Scene WorldObjects rebuilt! " + serializedCount + " objects serialized.");
                }
            }
        }
#endif
        
        public WorldObject ReturnWorldObjectPublic(WorldObjectData data)
        {
            return ReturnWorldObject(data);
        }
}

