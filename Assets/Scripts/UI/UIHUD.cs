using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PacMan
{
    public class UIHUD : UIPage
    {
        #region private variables
        [SerializeField] private Text lifeCount;
        [SerializeField] private Text progress;
        private int noOfLife = 3;
        #endregion

        #region private methods
        private void OnEnable()
        {
            PlayerController.FillBlock += updateProgress;
            PlayerController.ReduceLife += OnReduceLife;
        }
        private void OnDisable()
        {
            PlayerController.FillBlock -= updateProgress;
            PlayerController.ReduceLife -= OnReduceLife;
        }
        private void updateProgress()
        {
            float t_FilledPercentage =(int)((float)GameManager.instance.noOfFilledTile / (float)GameManager.instance.totalNoOfTile * 100);
            progress.text = t_FilledPercentage + "/100%";
            if (t_FilledPercentage >= 80)
            {
                //Game Completed
                GameManager.instance.playerController.gameObject.SetActive(false);
                UIController.instance.ShowPage(PageType.GAME_WIN);
            }
        }
        private void OnReduceLife()
        {
            noOfLife--;
            lifeCount.text = noOfLife.ToString();
            if (noOfLife == 0)
            {
                //GameOver
                Debug.Log("Game Over........................");
                UIController.instance.ShowPage(PageType.GAME_OVER);
            }
        }
        #endregion

    }
}
