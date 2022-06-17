using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.UI;
using PlayFab.Networking;
using GPUInstancer;

public class BuildingSystem : NetworkBehaviour
{
    #region Public;
    public static BuildingSystem Instance { get; private set; }
    public BuildingTypeListSO BuildingTypeListSO { get { return buildingTypeList; } }
    public BuildingTypeSO ActiveBuildingType { get { return activeBuildingType; } }
    public Dictionary<Vector2, List<GameObject>> towers = new Dictionary<Vector2, List<GameObject>>();
    #endregion

    #region private;
    private WorldManager worldManager;
    private BuildingTypeListSO buildingTypeList;
    private Vector2 selectedPosition;
    #endregion

    #region Serializables;
    [SerializeField]
    private BuildingTypeSO activeBuildingType;
    [SerializeField]
    private GPUInstancerPrefabManager gPUInstancerPrefabManager;

    [SerializeField]
    private Transform sellPanelUI;
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        worldManager = WorldManager.Instance;
        buildingTypeList = worldManager.BuildingTypeListSO;
        towers = new Dictionary<Vector2, List<GameObject>>();
    }

    #region Client
    [Client]
    public void checkForClick()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && (!EventSystem.current.IsPointerOverGameObject()) && activeBuildingType != null)
        {

            Vector3 pos = worldManager.GetPos();
            int myGold = worldManager.PlayersGold[worldManager.PlayerNumber];

            if (myGold < activeBuildingType.price[0])
            {
                Debug.Log("No money");
                activeBuildingType = null;
                return;
            }
            if (IsPositionInGrid(pos))
            {
                worldManager.Player.CmdSetTower(pos, buildingTypeList.towersList.IndexOf(activeBuildingType));
                activeBuildingType = null;
            }
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame && (!EventSystem.current.IsPointerOverGameObject()))
        {

            //Debug.Log("Here");
            Vector3 pos = worldManager.GetPos();

            Vector3 terainPos = worldManager.Terains[worldManager.PlayerNumber].transform.position;
            int width = worldManager.Width;

            if (!worldManager.Tiles[worldManager.PlayerNumber][(int)pos.x - (int)(terainPos.x - width / 2), (int)pos.z].HasTower)
            {
                selectedPosition = Vector2.zero;
                sellPanelUI.Find("SelectionTxt").GetComponent<Text>().text = " - ";
                sellPanelUI.Find("GoldText").GetComponent<Text>().text = " ";
                return;
            }
            selectedPosition = new Vector2(pos.x - (int)(terainPos.x - width / 2), pos.z);
            TowerBase selectedTowerBase = worldManager.Tiles[worldManager.PlayerNumber][(int)pos.x - (int)(terainPos.x - width / 2), (int)pos.z].TowerBase;

            string selectedTypetxt = selectedTowerBase.Data.towerType.ToString();
            sellPanelUI.Find("SelectionTxt").GetComponent<Text>().text = selectedTypetxt;
            int Gold = worldManager.BuildingTypeListSO.towersList[(int)selectedTowerBase.Data.towerType].price[0];
            sellPanelUI.Find("GoldText").GetComponent<Text>().text = Gold + "G";
        }
    }


    [Client]
    private bool IsPositionInGrid(Vector3 post)
    {
        //int playerNumber = UnityNetworkServer.Instance.Connections.FindIndex(c => c.ConnectionId == conn.connectionId);
        //Vector2 border = new Vector2(worldManager.Width, worldManager.Height);

        // return ((post.x <= 20 && post.x >= 0 && worldManager.Height <= 60 && post.z >= 0) && post.y != -10);

        Vector3 terainPos = worldManager.MyTerrain.transform.position;
        int width = worldManager.Width;
        int height = worldManager.Height;
        Tile[,] tiles = worldManager.Tiles[worldManager.PlayerNumber];

        for (int i = (int)post.x - 1; i <= (int)post.x + 1; i++)
        {
            for (int j = (int)post.z - 1; j <= (int)post.z + 1; j++)
            {
                if (!(i <= (int)(terainPos.x + width / 2 - 1) && (i >= (int)(terainPos.x - width / 2)) && (j <= (int)(terainPos.z + height / 2 - 1)) && (j >= (int)(terainPos.z - height / 2))))
                {
                    return false;
                }
                if (tiles[i - (int)(terainPos.x - width / 2), j].HasTower)
                {
                    return false;
                }
            }
        }

        return true;
    }

    [Client]
    public void SetActiveBuildingType(BuildingTypeSO towerType)
    {
        this.activeBuildingType = towerType;
    }

    [Client]
    public void OnSellClickButton()
    {
        if (selectedPosition == Vector2.zero)
        {
            return;
        }

        WorldManager.Instance.Player.CmdSellTower(selectedPosition);
        sellPanelUI.Find("SelectionTxt").GetComponent<Text>().text = " - ";
        sellPanelUI.Find("GoldText").GetComponent<Text>().text = " ";
        selectedPosition = Vector2.zero;
    }
    #endregion

    #region Server
    [Server]
    public void InitializeTower(NetworkConnection conn, Vector3 pos, int buildingType)
    {
        bool hasTower = false;
        int playerNumber = UnityNetworkServer.Instance.Connections.FindIndex(c => c.ConnectionId == conn.identity.connectionToClient.connectionId);

        int myGold = worldManager.PlayersGold[playerNumber];
        if (myGold < worldManager.BuildingTypeListSO.towersList[buildingType].price[0])
        {
            return;
        }

        List<Tile> tiles = new List<Tile>();

        Vector3 terainPos = worldManager.Terains[playerNumber].transform.position;
        int width = worldManager.Width;
        int height = worldManager.Height;

        for (int i = (int)pos.x - 1; i <= pos.x + 1; i++)
        {
            for (int j = (int)pos.z - 1; j <= pos.z + 1; j++)
            {
                if (!(i <= (int)(terainPos.x + width / 2 - 1) && (i >= (int)(terainPos.x - width / 2)) && (j <= (int)(terainPos.z + height / 2 - 1)) && (j >= (int)(terainPos.z - height / 2))))
                {
                    return;
                }
                if (worldManager.Tiles[playerNumber][i - (int)(terainPos.x - width / 2), j].HasTower)
                {
                    hasTower = true;
                    break;
                }
                tiles.Add(worldManager.Tiles[playerNumber][i - (int)(terainPos.x - width / 2), j]);
            }
        }

        if (hasTower)
        {
            return;
        }

        

        WorldObject tower = WorldObjectManager.Instance.InstantiateWorldObject(WorldObjectTypes.TowerBase, pos, Quaternion.identity);

        foreach (var tile in tiles)
        {
            tile.HasTower = true;
            tile.TowerBase = (TowerBase)tower;
        }

        TowerBaseObjectData data = (TowerBaseObjectData)tower.ReturnData();
        data.SetTowerBaseType((TowerBaseObjectData.TowerTypes)buildingType);
        data.SetTowerBaseLevel((TowerBaseObjectData.TowerLevel.Default));
        data.myTiles = tiles;
        data.playerOwner = playerNumber;
        tower.UpdateData(data);
        WorldObjectManager.Instance.InitializeWorldObject(tower, WorldObjectTypes.TowerBase);

        worldManager.ChangeTileOnClient(playerNumber, tiles);

        worldManager.Terains[playerNumber].GetComponent<NavMeshSurface>().BuildNavMesh();
        worldManager.RebuildNavMesh(playerNumber);
        worldManager.ChangeGold(-worldManager.BuildingTypeListSO.towersList[buildingType].price[0], playerNumber);
    }

    [Server]
    public void SellTower(NetworkConnection conn, Vector2 pos)
    {
        int playerNumber = UnityNetworkServer.Instance.Connections.FindIndex(c => c.ConnectionId == conn.identity.connectionToClient.connectionId);
        TowerBase tower = WorldManager.Instance.Tiles[playerNumber][(int)pos.x, (int)pos.y].TowerBase;
        
        int gold = worldManager.BuildingTypeListSO.towersList[(int)tower.Data.towerType].price[0];
        int towerType = (int)tower.Data.towerType;
        int towerLvl = (int)tower.Data.towerLevel;
        towers[new Vector2(towerType, towerLvl)].Remove(tower.TowerVisual);

        if (tower.isSkined)
        {
            Destroy(tower.RotationObject);
        }
        ClearTowerFromGPUList(pos, playerNumber);
        
        foreach(var tile in tower.Data.myTiles)
        {
            tile.HasTower = false;
        }

        worldManager.ChangeTileOnClient(playerNumber, tower.Data.myTiles);

        worldManager.ChangeGold(gold, playerNumber);

        StartCoroutine(DestroyTowerBase(tower));
    }

    [Server]
    private IEnumerator DestroyTowerBase(TowerBase tower)
    {
        yield return new WaitForSeconds(0.5f);

        WorldObjectManager.Instance.RemoveWorldObject(tower);
    }

    #endregion

    #region RPC.
    [ClientRpc]
    private void ClearTowerFromGPUList(Vector2 pos, int playerNumber)
    {

        TowerBase tower = WorldManager.Instance.Tiles[playerNumber][(int)pos.x, (int)pos.y].TowerBase;

        int towerType = (int)tower.Data.towerType;
        int towerLvl = (int)tower.Data.towerLevel;

        
        int index = towers[new Vector2(towerType, towerLvl)].IndexOf(tower.gameObject);

        GPUInstancerAPI.RemovePrefabInstance(gPUInstancerPrefabManager, towers[new Vector2(towerType, towerLvl)][index].GetComponent<GPUInstancerPrefab>());
        towers[new Vector2(towerType, towerLvl)].Remove(tower.gameObject);

        tower.gameObject.SetActive(false);
        if (tower.isSkined)
        {
            Destroy(tower.RotationObject);
        }

    }
    #endregion
}
