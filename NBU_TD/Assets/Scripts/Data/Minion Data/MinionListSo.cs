using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/MinionListSO")]
public class MinionListSo : ScriptableObject
{
    public List<MinionSO> minionList;
}
