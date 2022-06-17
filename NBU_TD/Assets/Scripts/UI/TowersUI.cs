using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class TowersUI : MonoBehaviour
{

    [SerializeField]
    private BuildingTypeListSO buildingTypeList;
    [SerializeField]
    private Configuration config;
    [SerializeField]
    private Transform towerBtnTemplate;



    // Initializing the UI..
    private void Awake()
    {
        if(config.buildType == BuildType.REMOTE_SERVER)
        {
            return;
        }

        towerBtnTemplate.gameObject.SetActive(false);

        int index = 0;
        foreach(BuildingTypeSO towerType in buildingTypeList.towersList)
        {
            Transform btnTransform = Instantiate(towerBtnTemplate, transform);
            btnTransform.gameObject.SetActive(true);

            float offsetAmount = +120f;
            btnTransform.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetAmount * index,0);

            btnTransform.Find("Image").GetComponent<Image>().sprite = towerType.sprite;

            btnTransform.GetComponent<Button>().onClick.AddListener(() => {
                BuildingSystem.Instance.SetActiveBuildingType(towerType);
            });

            index++;
        }
    }

}
    