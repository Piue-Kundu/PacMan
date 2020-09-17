using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PacMan
{
    public class UIMenu : UIPage
    {
        #region Public Methods
        public void PlayButtonClick()
        {
            UIController.instance.ShowPage(PageType.HUD);
        }
        #endregion
    }
}
