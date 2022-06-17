using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleReached : MonoBehaviour
{
    #region Public.
    public int PlayerNumber { get; set;}
    #endregion
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.GetComponent<MinionBase>())
        {

            MinionManager.Instance.MinionReachedDestination(other.GetComponent<MinionBase>(), PlayerNumber);
            WorldManager.Instance.ChangeLives(-1, PlayerNumber);
        }
    }
}
