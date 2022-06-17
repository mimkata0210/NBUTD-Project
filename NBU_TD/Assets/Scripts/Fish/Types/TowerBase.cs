using Mirror;
using System;
using UnityEngine;
using Unity.Mathematics;
using GPUInstancer;
using System.Collections.Generic;

public class TowerBase : WorldObject
{
    #region Public.
    [SerializeField, HideInInspector]
    public TowerBaseObjectData Data = new TowerBaseObjectData();
    public GameObject TowerVisual { get { return towerVisual; } }
    public GameObject RotationObject { get { return rotationPart; } }
    public bool isSkined { get; private set; }
    #endregion

    #region Private.
    private GameObject towerVisual;
    private GameObject rotationPart;
    private GPUInstancerPrefabManager prefabManager;
    private BuildingSystem buildingSystem;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    public override void UpdateData(WorldObjectData data)
    {
        Data = (TowerBaseObjectData)data;
    }
    public override bool ObjectNotDefault()
    {
        /* You would override this for each object to determine
        * if it has changed. Generally if an object is spawned
        * and exist in any fashion it is not considered default,
        * as spawned objects must be sent to late joining clients. */
        bool changed = (Data.towerLevel != TowerBaseObjectData.TowerLevel.Default ||
                           Data.Instantiated);
        return changed;
    }
    public override WorldObjectData ReturnData()
    {
        return Data;
    }
    

    private void Start()
    {
        prefabManager = WorldManager.Instance.GPUIPrefabManager;
        buildingSystem = BuildingSystem.Instance;

        BuildingTypeSO buildingTypeSO = buildingSystem.BuildingTypeListSO.towersList[(int)Data.towerType];

        if (!buildingTypeSO.isSkindMesh)
        {
            InstantiateNonSkinnedMesh(buildingTypeSO);
            return;
        }

        if (buildingTypeSO.hasRotatePart)
        {
            InstantiateSkinnedMesh(buildingTypeSO);
            isSkined = true;
            return;
        }
    }

    private void InstantiateNonSkinnedMesh(BuildingTypeSO buildingTypeSO)
    {
        GameObject prefab = buildingTypeSO.prefab[(int)Data.towerLevel];
        GPUInstancerPrefabPrototype prototype = GPUInstancerAPI.DefineGameObjectAsPrefabPrototypeAtRuntime(prefabManager, prefab);
        
        prototype.enableRuntimeModifications = true;
        prototype.autoUpdateTransformData = true;

        towerVisual = Instantiate(prefab, transform.position, prefab.transform.rotation);

        towerVisual.transform.parent = transform;

        if (!buildingSystem.towers.ContainsKey(new Vector2((int)Data.towerType, (int)Data.towerLevel)))
        {
            buildingSystem.towers.Add(new Vector2((int)Data.towerType, (int)Data.towerLevel), new List<GameObject>());
        }

        buildingSystem.towers[new Vector2((int)Data.towerType, (int)Data.towerLevel)].Add(transform.gameObject);

        GPUInstancerAPI.AddInstancesToPrefabPrototypeAtRuntime(prefabManager, prototype, buildingSystem.towers[new Vector2((int)Data.towerType, (int)Data.towerLevel)]);
    }

    private void InstantiateSkinnedMesh(BuildingTypeSO buildingTypeSO)
    {
        GameObject rotationGO = buildingTypeSO.skinnedMesh[(int)Data.towerLevel];
        GameObject baseGO = buildingTypeSO.prefab[(int)Data.towerLevel];

        GPUInstancerPrefabPrototype prototype = GPUInstancerAPI.DefineGameObjectAsPrefabPrototypeAtRuntime(prefabManager, baseGO);

        prototype.enableRuntimeModifications = true;
        prototype.autoUpdateTransformData = true;

        towerVisual = Instantiate(baseGO, transform.position, baseGO.transform.rotation);
        towerVisual.transform.parent = transform;
        rotationPart = Instantiate(rotationGO, transform.position, rotationGO.transform.rotation);

        if (!buildingSystem.towers.ContainsKey(new Vector2((int)Data.towerType, (int)Data.towerLevel)))
        {
            buildingSystem.towers.Add(new Vector2((int)Data.towerType, (int)Data.towerLevel), new List<GameObject>());
        }
        
        buildingSystem.towers[new Vector2((int)Data.towerType, (int)Data.towerLevel)].Add(transform.gameObject);

        GPUInstancerAPI.AddInstancesToPrefabPrototypeAtRuntime(prefabManager, prototype, buildingSystem.towers[new Vector2((int)Data.towerType, (int)Data.towerLevel)]);
    }
}
