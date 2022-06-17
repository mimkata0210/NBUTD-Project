using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIPage : MonoBehaviour
{
    #region Private.
    [SerializeField]
    private UIPageLIstSO uiPageList;
    [SerializeField]
    private float offsetAmount;
    [SerializeField]
    private Configuration config;
    [SerializeField]
    private Transform btnTemplate;
    [SerializeField]
    private List<Image> bgImages;
    #endregion

    private void Awake()
    {
        if (config.buildType == BuildType.REMOTE_SERVER)
        {
            return;
        }

        btnTemplate.gameObject.SetActive(false);

        int index = 0;
        foreach (UIPagesSO page in uiPageList.pageList)
        {
            Transform btnTransform = Instantiate(btnTemplate, transform);
            btnTransform.gameObject.SetActive(true);

            btnTransform.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetAmount * index, 0);

            btnTransform.Find("Image").GetComponent<Image>().sprite = page.sprite;
            bgImages.Add(btnTransform.Find("BG").GetComponent<Image>());
            
            btnTransform.GetComponent<Button>().onClick.AddListener(() => {
                this.bgImages[UIManager.Instance.prevActiveIndex].color = Color.white;
                UIManager.Instance.ActivatePage(page);
                this.bgImages[UIManager.Instance.prevActiveIndex].color = Color.green;
                
            });

            index++;
        }
    }
}
