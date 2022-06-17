using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/BuildingType")]
public class BuildingTypeSO : ScriptableObject
{
    public Sprite sprite;
    public List<GameObject> prefab;
    public bool isSkindMesh;
    public bool hasRotatePart;
    public List<GameObject> skinnedMesh;
    public List<int> price;
}
