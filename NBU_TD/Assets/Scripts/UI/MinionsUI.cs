using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MinionsUI : MonoBehaviour
{
    [SerializeField]
    private MinionListSo minionTypeList;
    [SerializeField]
    private Configuration config;
    [SerializeField]
    private Transform towerBtnTemplate;
    


    private void Awake()
    {
        if (config.buildType == BuildType.REMOTE_SERVER)
        {
            return;
        }

        towerBtnTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        int index = 0;
        foreach (MinionSO minionType in minionTypeList.minionList)
        {
            Transform btnTransform = Instantiate(towerBtnTemplate, transform);
            btnTransform.gameObject.SetActive(true);

            float offsetAmount = +120f;
            btnTransform.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetAmount * index, 0);

            btnTransform.Find("Image").GetComponent<Image>().sprite = minionType.sprite;

            btnTransform.GetComponent<Button>().onClick.AddListener(() => {
                //WorldManager.Instance.Player.CmdSpawnEnemy(minionTypeList.minionList.IndexOf(minionType));
                MinionManager.Instance.AddMinionForSpawn(minionTypeList.minionList.IndexOf(minionType));
            });

            index++;
        }
    }
}
