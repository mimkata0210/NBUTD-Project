using UnityEngine;
using Mirror;
using PlayFab.Networking;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.AI;
using GPUInstancer;

public class WorldManager : NetworkBehaviour
{
    #region Instance.
    public static WorldManager Instance { get; private set; }
    #endregion

    #region Exposables.
    [Tooltip("Building grid hight")]
    [SerializeField]
    private int hight;
    [Tooltip("Building grid width")]
    [SerializeField]
    private int width;
    [SerializeField]
    private Material gridVisualMaterial;
    [Tooltip("Space for spawn/despawn of minions")]
    [SerializeField]
    private float spawnDespawnArea;
    [SerializeField]
    private GPUInstancerPrefabManager gPUIPrefabManager;
    [Tooltip("List of minions")]
    [SerializeField]
    private MinionListSo minionList;
    [Tooltip("All buildings list")]
    [SerializeField]
    private BuildingTypeListSO buildingTypeList;
    [Tooltip("All portals list")]
    [SerializeField]
    private PortalListSO portalListSO;
    [Tooltip("All castle list")]
    [SerializeField]
    private CastleListSO castleListSO;
    [SerializeField]
    private float spawnTimer;
    [SerializeField]
    private int startingGold;
    [SerializeField]
    private int startingIncome;
    [SerializeField]
    private int startingLives;
    [SerializeField]
    private PlayersStatsUI playersStats;
    #endregion

    #region Private.
    private Dictionary<int,WorldObject> terains = new Dictionary<int,WorldObject>();
    private Dictionary<int, Tile[,]> tiles = new Dictionary<int, Tile[,]>();
    private Ground myTerrain;
    private TowerInitialize player;
    private int playerNumber;
    private BuildingSystem buildingSystem;
    private MinionManager minionManager;
    private float currentTimeTillSpawn;
    private bool startSpawning;
    private List<int> playersLives = new List<int>();
    private List<int> playersIncome = new List<int>();
    private List<int> playersGold = new List<int>();
    #endregion

    #region Public.
    public  Dictionary<int,Tile[,]> Tiles { get { return tiles; } }
    public Ground MyTerrain { get { return myTerrain; } } 
    public int Width { get { return width; } }
    public int Height { get { return hight; } }
    public Dictionary<int, WorldObject> Terains { get { return terains; } }

    public TowerInitialize Player { get { return player; } }

    public int PlayerNumber { get { return playerNumber; } }
    public GPUInstancerPrefabManager GPUIPrefabManager { get { return gPUIPrefabManager; } }
    public MinionListSo MinionList { get { return minionList; } }
    public BuildingTypeListSO BuildingTypeListSO { get { return buildingTypeList; } }
    public PortalListSO PortalListSO { get { return portalListSO; } }
    public CastleListSO CastleListSO { get { return castleListSO; } }
    public float SpawnDespawnArea { get { return spawnDespawnArea; } }
    public List<int> PlayersGold { get { return playersGold; } }
    public List<int> PlayersIncome { get { return playersIncome; } }
    public List<int> PlayersLive { get { return playersLives; } }

    public Tile checkTile;
    #endregion
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        buildingSystem = BuildingSystem.Instance;
        minionManager = MinionManager.Instance;
        currentTimeTillSpawn = spawnTimer;
    }

    private void Update()
    {
        if (!startSpawning && terains.Count == UnityNetworkServer.Instance.MaxConnections && isServer)
        {
            startSpawning = true;
            for (int i = 0; i < UnityNetworkServer.Instance.numPlayers; i++)
            {
                playersGold.Add(startingGold);
                playersIncome.Add(startingIncome);
                playersLives.Add(startingLives);
            }
            InitializeUI(terains.Count, spawnTimer);

            SyncTileLists(UnityNetworkServer.Instance.numPlayers, width, hight);
            SyncLives(playersLives);
            SyncGold(playersGold);
            SyncIncome(PlayersIncome);
        }

        if (startSpawning && isServer)
        {
            if(currentTimeTillSpawn <= 0)
            {
                for(int i = 0; i < UnityNetworkServer.Instance.numPlayers; i++)
                {
                    StartCoroutine(minionManager.SpawnMinions(i));
                    ChangeGold(playersIncome[i], i);
                }
                currentTimeTillSpawn = spawnTimer;

            }
            currentTimeTillSpawn -= Time.deltaTime;
        }

        if (isClient)
        {
            buildingSystem.checkForClick();
        }
    }

    public Vector3 GetPos()
    {
        Plane plane = new Plane(Vector3.up, 0);

        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }

        Vector3 zero = new Vector3(0, -10, 0);
        return zero;
    }
    #region Server.
    [Server]
    public void CreateRoad(NetworkConnection conn, GameObject playerObj)
    {
        int playerNumber = UnityNetworkServer.Instance.Connections.FindIndex(c => c.ConnectionId == conn.connectionId);

        Vector3 pos = new Vector3(width / 2 + 100 * playerNumber, 0, hight / 2);

        WorldObject wo = WorldObjectManager.Instance.InstantiateWorldObject(WorldObjectTypes.Ground, pos, Quaternion.identity);
        terains.Add(playerNumber,wo);
        wo.transform.localScale = new Vector3(width / 10, 1, hight / 10 + spawnDespawnArea);


        GroundObjectData data = (GroundObjectData)wo.ReturnData();
        data.SetGroundState(GroundObjectData.GroundStates.Default);
        data.PlayerNumber = playerNumber;
        data.TerrainHight = hight;
        data.TerrainWidth = width;
        wo.UpdateData(data);
        WorldObjectManager.Instance.InitializeWorldObject(wo, WorldObjectTypes.Ground);

        Tile[,] tempTiles = new Tile[width, hight];
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < hight; j++)
            { 
                int[] colorIndexs = new int[4];
                for(int k = 0; k < 4; k++)
                {
                    colorIndexs[k] = 4 * (i * hight + j) + k;
                }
                tempTiles[i,j] = new Tile(i, j, colorIndexs);
            }
        }


        tiles.Add(playerNumber, tempTiles);
        InitializeGridOnTarget(conn, hight, width, data);
        SetPlayerObj(conn, playerObj);
        SetPlayerNumber(conn, playerNumber);
        wo.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    [Server]
    public void ChangeLives(int live, int playerListNumber)
    {
        playersLives[playerListNumber] += live;
        if(playersLives[playerListNumber] <= 0)
        {
            // defeat
            Debug.Log("Def" + playerListNumber);
        }
        SyncLives(playersLives);
    }

    [Server]
    public void ChangeIncome(int income, int playerListNumber)
    {
        playersIncome[playerListNumber] += income;
        SyncIncome(playersIncome);
    }

    [Server]
    public void ChangeGold(int gold, int playerListNumber)
    {
        playersGold[playerListNumber] += gold;
        SyncGold(playersGold);
    }
    #endregion

    #region RPC.
    [ClientRpc]
    private void SyncTileLists(int numberOfPlayers, int width, int hight)
    {
        if (isServer)
        {
            return;
        }

        Dictionary<int, Tile[,]> syncTiles = new Dictionary<int, Tile[,]>();
        for (int n = 0; n < numberOfPlayers; n++)
        {
            syncTiles.Add(n, new Tile[width, hight]);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < hight; j++)
                {
                    int[] colorIndexs = new int[4];
                    for (int k = 0; k < 4; k++)
                    {
                        colorIndexs[k] = 4 * (i * hight + j) + k;
                    }
                    syncTiles[n][i, j] = new Tile(i, j, colorIndexs);
                }
            }
        }
        tiles = syncTiles;
    }
    [TargetRpc]
    private void InitializeGridOnTarget(NetworkConnection conn, int hight, int width, WorldObjectData terrainData)
    {
        Tile[,] tempTiles = new Tile[width, hight];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < hight; j++)
            {

                int[] colorIndexs = new int[4];
                for (int k = 0; k < 4; k++)
                {
                    colorIndexs[k] = 4 * (i * hight + j) + k;
                }
                tempTiles[i, j] = new Tile(i, j, colorIndexs);
            }
        }

        this.tiles.Clear();
        this.tiles.Add(0, tempTiles);

        myTerrain = (Ground)WorldObjectManager.Instance.ReturnWorldObjectPublic(terrainData);

        GameObject go = new GameObject();
        go.AddComponent(typeof(MeshFilter));
        go.AddComponent(typeof(MeshRenderer));
        go.AddComponent(typeof(GridVisual));

        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Vector3 pos = myTerrain.transform.position;
        pos.x -= width / 2;
        pos.y = 0.01f;
        pos.z -= hight / 2;

        go.transform.position = pos;

        go.GetComponent<MeshRenderer>().material = gridVisualMaterial;

        myTerrain.Grid = go.GetComponent<GridVisual>();
        myTerrain.Grid.CreateGridVisual(hight, width);

        go.transform.parent = myTerrain.transform;
    }

    [TargetRpc]
    private void SetPlayerObj(NetworkConnection conn, GameObject obj)
    {
        this.player = obj.GetComponent<TowerInitialize>();
    }


    [TargetRpc]
    private void SetPlayerNumber(NetworkConnection conn, int playerNumber)
    {
        this.playerNumber = playerNumber;
    }

    [ClientRpc]
    public void ChangeTileOnClient(int playerNumber, List<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            this.tiles[playerNumber][tile.X, tile.Z] = tile;

            if (tile.HasTower)
            {
                int[] indexes = this.tiles[playerNumber][tile.X, tile.Z].ColorIndexs;

                List<int[]> list = new List<int[]>();
                list.Add(indexes);
                
                if(PlayerNumber == playerNumber)
                    myTerrain.Grid.ChangeColorsForCell(list, Color.red);
            }
            else
            {
                int[] indexes = this.tiles[playerNumber][tile.X, tile.Z].ColorIndexs;

                List<int[]> list = new List<int[]>();
                list.Add(indexes);

                if (PlayerNumber == playerNumber)
                    myTerrain.Grid.ChangeColorsForCell(list, Color.white);
            }
        }
    }

    [ClientRpc]
    public void RebuildNavMesh(int playerNumber)
    {
        if (isServer)
            return;

        terains[playerNumber].GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    [ClientRpc]
    public void SyncGold(List<int> gold)
    {
        playersGold = gold;
        playersStats.SetGold();
    }

    [ClientRpc]
    public void SyncIncome(List<int> income)
    {
        playersIncome = income;
        playersStats.SetIncome();
    }

    [ClientRpc]
    public void SyncLives(List<int> lives)
    {
        playersLives = lives;
        playersStats.SetLives();
    }

    [ClientRpc]
    private void InitializeUI(int playersNumber, float incomeTimer)
    {
        playersStats.InitializePlayerStatsUI(playersNumber, incomeTimer);
    }
    #endregion

    #region Client.
    [Client]
    public void AddTerrain(GameObject terrain, int playerNumber)
    {
        if (isServer)
            return;

        terains.Add(playerNumber, terrain.GetComponent<WorldObject>());
    }
    #endregion
}
