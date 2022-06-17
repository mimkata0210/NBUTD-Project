using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayersStatsUI : MonoBehaviour
{
    #region Serialize.
    [SerializeField]
    private Transform playerStatsTemplate;
    [SerializeField]
    private float offsetAmount;
    [SerializeField]
    private Transform nextIncomeText;
    #endregion

    #region Private.
    private List<Transform> statsTransforms = new List<Transform>();
    private float totalIncomeTimer;
    private float currentIncomeTimeLeft;
    #endregion
    public void InitializePlayerStatsUI(int playersNumber, float incomeTimer)
    {
        playerStatsTemplate.gameObject.SetActive(false);
        for(int i = 0; i < playersNumber; i++)
        {
            Transform statsTransfrom = Instantiate(playerStatsTemplate, transform);
            statsTransfrom.gameObject.SetActive(true);

            statsTransfrom.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, offsetAmount * i);
            if(i == WorldManager.Instance.PlayerNumber)
            {
                statsTransfrom.Find("PlayerName").GetComponent<TextMeshProUGUI>().SetText("Me");
            }
            else
            {
                statsTransfrom.Find("PlayerName").GetComponent<TextMeshProUGUI>().SetText("P" + (i + 1));
            }
            statsTransforms.Add(statsTransfrom);
        }
        totalIncomeTimer = incomeTimer;
        currentIncomeTimeLeft = totalIncomeTimer;
        StartCoroutine(WaveCounter());
    }

    public void SetGold()
    {
        for (int i = 0; i < WorldManager.Instance.Terains.Count; i++)
        {
            statsTransforms[i].Find("TotalGold").GetComponent<TextMeshProUGUI>().SetText(WorldManager.Instance.PlayersGold[i].ToString());
        }   
    }

    public void SetIncome()
    {
        for (int i = 0; i < WorldManager.Instance.Terains.Count; i++)
        {
            statsTransforms[i].Find("Income").GetComponent<TextMeshProUGUI>().SetText(WorldManager.Instance.PlayersIncome[i].ToString());
        }
    }

    public void SetLives()
    {
        for (int i = 0; i < WorldManager.Instance.Terains.Count; i++)
        {
            statsTransforms[i].Find("Lives").GetComponent<TextMeshProUGUI>().SetText(WorldManager.Instance.PlayersLive[i].ToString());
        }
    }

    public IEnumerator WaveCounter()
    {
        while (true)
        {
            currentIncomeTimeLeft -= Time.deltaTime;
            if (currentIncomeTimeLeft < 0)
            {
                currentIncomeTimeLeft = totalIncomeTimer;
            }
            string nextIncomeString = "Next Income: " + currentIncomeTimeLeft.ToString("F2");
            nextIncomeText.Find("NextWave").GetComponent<TextMeshProUGUI>().SetText(nextIncomeString);
            yield return null;
        }
        
    }
}
