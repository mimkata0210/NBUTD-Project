using Mirror;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable CS0618, CS0672, CS0649
public class Ground : WorldObject
{
    #region Public.
    /// <summary>
    /// Data for this WorldObject. Needs to be serialized so that the key be written in the scene.
    /// </summary>
    [SerializeField, HideInInspector]
    public GroundObjectData Data = new GroundObjectData();

    public GridVisual Grid { get { return grid; } set { grid = value; } }
    #endregion

    #region Serialized.
    /// <summary>
    /// Object to show when WorldObject is available.
    /// </summary>
    [Tooltip("Object to show when WorldObject is available.")]
    [SerializeField]
    private GameObject _visualObject;
    [Tooltip("Object to show the grid on")]
    [SerializeField]
    private GridVisual grid;
    #endregion


    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        WorldManager worldManager = WorldManager.Instance;
        worldManager.AddTerrain(transform.gameObject, Data.PlayerNumber);
        transform.GetComponent<NavMeshSurface>().BuildNavMesh();

        Vector3 position = transform.position;
        position.z -= worldManager.Height / 2 + worldManager.SpawnDespawnArea * 5;
        GameObject castle = Instantiate(worldManager.CastleListSO.castleListSO[0].prefab, position, Quaternion.identity);
        castle.GetComponent<CastleReached>().PlayerNumber = Data.PlayerNumber;
    }

    public override void UpdateData(WorldObjectData data)
    {
        Data = (GroundObjectData)data;
    }

    public override bool ObjectNotDefault()
    {
        /* You would override this for each object to determine
        * if it has changed. Generally if an object is spawned
        * and exist in any fashion it is not considered default,
        * as spawned objects must be sent to late joining clients. */
        bool changed = (Data.GroundState != GroundObjectData.GroundStates.Default ||
                           Data.Instantiated);
        return changed;
    }

    public override WorldObjectData ReturnData()
    {
        return Data;
    }


   
}
