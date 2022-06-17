using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MinionBaseObjectData : WorldObjectData
{
    public enum MinionState
    {
        Default = 0,
        Removed = 1
    }
    public enum MinionType {
        Lizard = 0
    }


    public MinionType minionType { get; private set; }
    public MinionState minionState { get; private set; }
    public int playerOwner;
    public int enemyNumber;

    public void SetMinionBaseState(MinionState state) { minionState = state; }
    public void SetMinionBaseType(MinionType type) { minionType = type; }

}
