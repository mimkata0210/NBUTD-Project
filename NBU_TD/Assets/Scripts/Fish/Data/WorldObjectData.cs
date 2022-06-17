using Mirror;
using UnityEngine;

#pragma warning disable CS0618, CS0672
/* I made the data which must be in sync it's own class so that I
 * could write a serializer for it and pass that over the network. */
[System.Serializable]
    public class WorldObjectData
    {
        /// <summary>
        /// Serialized value because accessors cannot be serialized.
        /// </summary>
        [SerializeField, HideInInspector]
        private uint _key;
        /// <summary>
        /// Key for this data.
        /// </summary>
        public uint Key
        {
            get { return _key; }
            set { _key = value; }
        }
        /// <summary>
        /// Sets the Key value.
        /// </summary>
        /// <param name="value"></param>
        public void SetKey(uint value)
        {
            Key = value;
        }
        /// <summary>
        /// WorldObjectType for this instance.
        /// </summary>
        public WorldObjectTypes ObjectType { get; private set; } = WorldObjectTypes.Unset;
        /// <summary>
        /// Sets Type value.
        /// </summary>
        /// <param name="value"></param>
        public void SetObjectType(WorldObjectTypes value)
        {
            ObjectType = value;
        }
        /// <summary>
        /// True if instantiated rather than placed in the scene.
        /// </summary>
        public bool Instantiated { get; private set; } = false;
        /// <summary>
        /// Sets Instantiated value.
        /// </summary>
        /// <param name="value"></param>
        public void SetInstantiated(bool value) { Instantiated = value; }

    }

public static class WorldObjectDataSerializer
{
    public static void WriteWorldObjectData(this NetworkWriter writer, WorldObjectData data)
    {
        writer.WriteInt((int)data.ObjectType);
        writer.WriteUInt(data.Key);
        writer.WriteBool(data.Instantiated);

        //Tree type.
        if (data is GroundObjectData od)
        {
            writer.WriteByte((byte)od.GroundState);
            writer.WriteInt(od.PlayerNumber);
            writer.WriteInt(od.TerrainWidth);
            writer.WriteInt(od.TerrainHight);
        }
        else if(data is TowerBaseObjectData tb)
        {
            writer.WriteByte((byte)tb.towerType);
            writer.WriteByte((byte)tb.towerLevel);
            writer.WriteInt(tb.playerOwner);
        }
        else if (data is MinionBaseObjectData md)
        {
            writer.WriteByte((byte)md.minionType);
            writer.WriteByte((byte)md.minionState);
            writer.WriteInt(md.playerOwner);
            writer.WriteInt(md.enemyNumber);
        }
    }

    public static WorldObjectData ReadWorldObjectData(this NetworkReader reader)
    {
        WorldObjectTypes objectType = (WorldObjectTypes)reader.ReadInt();
        uint key = reader.ReadUInt();
        bool spawned = reader.ReadBool();

        //Tree type.

        if (objectType == WorldObjectTypes.Ground)
        {
            GroundObjectData data = new GroundObjectData();
            data.SetObjectType(objectType);
            data.SetKey(key);
            data.SetInstantiated(spawned);
            data.SetGroundState((GroundObjectData.GroundStates)reader.ReadByte());
            data.PlayerNumber = reader.ReadInt();
            data.TerrainWidth = reader.ReadInt();
            data.TerrainHight = reader.ReadInt();

            return data;
        }

        // Tower Type
        else if(objectType == WorldObjectTypes.TowerBase)
        {
            TowerBaseObjectData data = new TowerBaseObjectData();
            data.SetObjectType(objectType);
            data.SetKey(key);
            data.SetInstantiated(spawned);
            data.SetTowerBaseType((TowerBaseObjectData.TowerTypes)reader.ReadByte());
            data.SetTowerBaseLevel((TowerBaseObjectData.TowerLevel)reader.ReadByte());
            data.playerOwner = reader.ReadInt();

            return data;
        }
        else if(objectType == WorldObjectTypes.MinionBase)
        {
            MinionBaseObjectData data = new MinionBaseObjectData();
            data.SetObjectType(objectType);
            data.SetKey(key);
            data.SetInstantiated(spawned);
            data.SetMinionBaseType((MinionBaseObjectData.MinionType)reader.ReadByte());
            data.SetMinionBaseState((MinionBaseObjectData.MinionState)reader.ReadByte());
            data.playerOwner = reader.ReadInt();
            data.enemyNumber = reader.ReadInt();
            return data;
        }
        //Not supported by serializer.
        else
        {
            Debug.LogError("Serializer not written for type " + objectType.ToString());
            return null;
        }

    }

}