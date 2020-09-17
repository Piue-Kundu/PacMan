using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PacMan
{
    public class GameManager : MonoBehaviour
    {
        #region public variables
        public static GameManager instance;
        public Tilemap tilemap;
        public PlayerController playerController;
        public int totalNoOfTile;
        public int noOfFilledTile;
        #endregion
        #region private methods
        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
        }
        #endregion
    }
}
