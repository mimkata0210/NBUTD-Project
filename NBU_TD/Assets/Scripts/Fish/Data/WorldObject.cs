using UnityEngine;

#pragma warning disable CS0618, CS0672, CS0649
    public abstract class WorldObject : MonoBehaviour
    {
        #region Serialized.
        /// <summary>
        /// ObjectType set to Data on awake.
        /// </summary>
        [SerializeField]
        private WorldObjectTypes _objectType;
        #endregion

        /// <summary>
        /// Always override this in children and call base.Awake()
        /// </summary>
        protected virtual void Awake()
        {
            ReturnData().SetObjectType(_objectType);
        }

        /// <summary>
        /// Updates this object's data.
        /// </summary>
        public virtual void UpdateData(WorldObjectData data) { }

        /// <summary>
        /// Returns WorldObjectData for this object.
        /// </summary>
        /// <returns></returns>
        public virtual WorldObjectData ReturnData()
        {
            return null;
        }

        /// <summary>
        /// Returns if the object is not in it's default state.
        /// </summary>
        /// <returns></returns>
        public virtual bool ObjectNotDefault()
        {
            return false;
        }
    }