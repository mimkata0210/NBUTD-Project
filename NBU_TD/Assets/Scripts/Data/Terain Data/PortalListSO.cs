using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PortalListSO")]
public class PortalListSO : ScriptableObject
{
    public List<PortalSO> portalList;
}
