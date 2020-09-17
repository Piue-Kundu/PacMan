using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PacMan
{
    public class UIController : MonoBehaviour
    {
        #region Public Variables
        public static UIController instance;
        #endregion
        #region Private Variables
        private Dictionary<PageType, UIPage> dicAllPage = new Dictionary<PageType, UIPage>();
        #endregion
        #region Private Methods
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            AddPage();
        }

        private void Start()
        {
            ShowPage(PageType.MENU);
        }
        #endregion
        #region Public Methods
        public void AddPage()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                UIPage uiPage = transform.GetChild(i).GetComponent<UIPage>();
                dicAllPage.Add(uiPage.pageType, uiPage);
            }
        }

        public void ShowPage(PageType pageType)
        {
            HideAllPage();
            dicAllPage[pageType].gameObject.SetActive(true);
        }

        public void HidePage(PageType pageType)
        {
            dicAllPage[pageType].gameObject.SetActive(false);
        }

        public void HideAllPage()
        {
            foreach (KeyValuePair<PageType, UIPage> entry in dicAllPage)
            {
                dicAllPage[entry.Key].gameObject.SetActive(false);
            }
        }
        #endregion

    }


    public enum PageType
    {
        MENU,
        HUD,
        GAME_WIN,
        GAME_OVER
    }
}

