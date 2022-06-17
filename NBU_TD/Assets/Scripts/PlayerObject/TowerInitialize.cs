using Mirror;
using UnityEngine;
using PlayFab.Networking;

public class TowerInitialize : NetworkBehaviour
{
    #region Private
    private BuildingSystem buildingSystem;
    private MinionManager minionManager;
    #endregion

    private void Start()
    {
        buildingSystem = BuildingSystem.Instance;
        minionManager = MinionManager.Instance;
    }
    #region Command
    [Command]
    public void CmdSetTower(Vector3 pos, int buildingType)
    {
        buildingSystem.InitializeTower(connectionToClient, new Vector3((int)pos.x + 0.5f, (int)pos.y, (int)pos.z + 0.2f), buildingType);
    }
    [Command]
    public void CmdSpawnEnemy(int minionType)
    {
        minionManager.InitializeMinion(connectionToClient, minionType);
    }
    [Command]
    public void CmdSellTower(Vector2 pos)
    {
        buildingSystem.SellTower(connectionToClient, pos);
    }
    #endregion
}
