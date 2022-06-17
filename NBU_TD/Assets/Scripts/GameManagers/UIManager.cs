using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Private.
    [SerializeField]
    private List<GameObject> pages;
    [SerializeField]
    private UIPageLIstSO pageList;
    #endregion

    #region Public.
    public static UIManager Instance;
    public int prevActiveIndex { get; private set; }
    #endregion

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        prevActiveIndex = 0;
    }
    private void Start()
    {
        prevActiveIndex = 1;
        for(int i = 1; i < pages.Count; i++)
        {
            pages[i].SetActive(false);
        }
    }

    public void ActivatePage(UIPagesSO page)
    {
        int br = 0;
        foreach(var listPage in pageList.pageList)
        {
            if(listPage == page)
            {
                break;
            }
            br++;
        }
        pages[prevActiveIndex].SetActive(true);
        pages[br].SetActive(false);
        prevActiveIndex = br;
    }
}
