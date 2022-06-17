using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/MinionSO")]
public class MinionSO : ScriptableObject
{
    public GameObject prefab;
    public Sprite sprite;
    public int income;
    public int price;
    public int goldOnDeath;
}
