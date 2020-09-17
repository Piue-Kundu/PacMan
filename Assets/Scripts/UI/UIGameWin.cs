using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PacMan
{
    public class UIGameWin : UIPage
    {
        #region Public Methods
        public void ReplayButtonClick()
        {
            SceneManager.LoadScene(0);
        }
        #endregion
    }
}
