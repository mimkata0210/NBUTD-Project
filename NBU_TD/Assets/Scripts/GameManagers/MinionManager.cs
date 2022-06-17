using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using PlayFab.Networking;
using GPUInstancer;

public class MinionManager : NetworkBehaviour
{
    #region Public.
    public static MinionManager Instance { get; private set; }
    #endregion

    #region Private.
    private Dictionary<int,Queue<Vector2>> minionsType;
    private WorldManager worldManager;
    private Dictionary<int,List<MinionBase>> liveMonsters = new Dictionary<int, List<MinionBase>>();
    #endregion

    #region Serializable.
    [SerializeField]
    private float spawnDelay;
    [SerializeField]
    private int maxMinions;
    #endregion
    private void Awake()
    {
        Instance = this;
        minionsType = new Dictionary<int, Queue<Vector2>>();
    }

    private void Start()
    {
        worldManager = WorldManager.Instance;
    }

    #region Client.
    [Client]
    public void AddMinionForSpawn(int minionType)
    {
        worldManager.Player.CmdSpawnEnemy(minionType);
    }
    #endregion

    #region Server.
    [Server]
    public void InitializeMinion(NetworkConnection conn, int minionType)
    {
        int playerNumber = UnityNetworkServer.Instance.Connections.FindIndex(c => c.ConnectionId == conn.identity.connectionToClient.connectionId);
        MinionSO minionSO = worldManager.MinionList.minionList[minionType];
        if (minionsType[playerNumber].Count + liveMonsters.Count >= maxMinions || minionSO.price > worldManager.PlayersGold[playerNumber])
        {
            return;
        }
        minionsType[playerNumber].Enqueue(new Vector2((float)playerNumber,(float)minionType));

        
        worldManager.ChangeIncome(minionSO.income, playerNumber);
        worldManager.ChangeGold(-minionSO.price, playerNumber);
    }

    [Server]
    public IEnumerator SpawnMinions(int queKey)
    {
        Queue<Vector2> minionQue = minionsType[queKey];
        while(minionQue.Count > 0)
        {
            var minionType = minionQue.Peek();
            int playerNumber = (int)minionType.x;
            int buildingType = (int)minionType.y;
            int enemyPlayer;
            if (playerNumber + 1 == worldManager.Terains.Count)
            {

                enemyPlayer = 0;
            }
            else
            {
                enemyPlayer = playerNumber + 1;
            }

            Vector3 terainPos = worldManager.Terains[enemyPlayer].transform.position;
            terainPos.z = terainPos.z * 2 + worldManager.SpawnDespawnArea;


            WorldObject minion = WorldObjectManager.Instance.InstantiateWorldObject(WorldObjectTypes.MinionBase, terainPos, Quaternion.identity);

            MinionBaseObjectData data = (MinionBaseObjectData)minion.ReturnData();
            data.SetMinionBaseType((MinionBaseObjectData.MinionType)buildingType);
            data.SetMinionBaseState(0f);
            data.playerOwner = playerNumber;
            data.enemyNumber = enemyPlayer;
            minion.UpdateData(data);
            WorldObjectManager.Instance.InitializeWorldObject(minion, WorldObjectTypes.MinionBase);
            minionQue.Dequeue();

            if (liveMonsters.ContainsKey(playerNumber))
            {
                liveMonsters[playerNumber].Add(minion.GetComponent<MinionBase>());
            }
            else
            {
                List<MinionBase> minionList = new List<MinionBase>();
                minionList.Add(minion.GetComponent<MinionBase>());
                liveMonsters.Add(playerNumber, minionList);
            }
            yield return new WaitForSeconds(spawnDelay);
        } 
    }

    [Server]
    public void AddNewPlayerSpawnQue(NetworkConnection conn)
    {

        int playerNumber = UnityNetworkServer.Instance.Connections.FindIndex(c => c.ConnectionId == conn.identity.connectionToClient.connectionId);
        if (!minionsType.ContainsKey(playerNumber))
        {
            minionsType.Add(playerNumber, new Queue<Vector2>());
        }
    }

    [Server]
    public void MinionReachedDestination(MinionBase minion, int playerNumber)
    {
        minion.Data.SetMinionBaseState(MinionBaseObjectData.MinionState.Removed);

        if(playerNumber - 1 < 0)
        {
            playerNumber = worldManager.Terains.Count - 1;
        }
        else
        {
            playerNumber -= 1;
        }
        liveMonsters[playerNumber].Remove(minion);
        WorldObjectManager.Instance.RemoveWorldObject(minion);
    }
    #endregion
}
