using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionBase : WorldObject
{
    #region Public.
    [SerializeField, HideInInspector]
    public MinionBaseObjectData Data = new MinionBaseObjectData();
    public List<Tile> myTiles = new List<Tile>();
    #endregion

    #region Private.
    private MinionListSo minionTypeList;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    public override void UpdateData(WorldObjectData data)
    {
        Data = (MinionBaseObjectData)data;
    }

    public override bool ObjectNotDefault()
    {
        /* You would override this for each object to determine
        * if it has changed. Generally if an object is spawned
        * and exist in any fashion it is not considered default,
        * as spawned objects must be sent to late joining clients. */
        bool changed = (Data.Instantiated || Data.minionState != MinionBaseObjectData.MinionState.Default);
        return changed;
    }
    public override WorldObjectData ReturnData()
    {
        return Data;
    }

    private void Start()
    {
        minionTypeList = WorldManager.Instance.MinionList;
        MinionSO minionType = minionTypeList.minionList[(int)Data.minionType];
        GameObject minion = Instantiate(minionType.prefab, transform);
        transform.rotation = Quaternion.Euler(new Vector3(0f, 180, 0f));

        Vector3 goalPosition = WorldManager.Instance.Terains[Data.enemyNumber].transform.position;
        goalPosition.z = 0 - WorldManager.Instance.SpawnDespawnArea * 5;

        transform.GetComponent<NavMeshAgent>().SetDestination(goalPosition);
    }
}
