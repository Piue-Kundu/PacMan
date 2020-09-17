using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PacMan
{
    public class Tilemap : MonoBehaviour
    {
        #region private variables
        [SerializeField] private int row = 24;
        [SerializeField] private int column = 32;
        [SerializeField] private Tile tile;
        private int startXPos = -15;
        private int startYPos = -12;
        private Tile[,] tileArr;
        #endregion

        #region public fields
        public int NO_OF_ROWS
        {
            get
            {
                return row;
            }
        }
        public int NO_OF_COLUMN
        {
            get
            {
                return column;
            }
        }
        #endregion

        #region private methods
        private void Start()
        {
            tileArr = new Tile[row,column];
            drawMap();
        }

        private void drawMap()
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    Tile t_tile = Instantiate(tile, this.transform) as Tile;
                    t_tile.gameObject.name = "Tile";// i + "_" + j;
                    t_tile.transform.position = new Vector3(startXPos + j, startYPos + i, 0);
                    tileArr[i,j] = t_tile;
                    if (j == 0 ||i==0 || j==column-1 || i== row-1)
                    {
                        t_tile.IS_FILLED = true;
                        t_tile.name = "Wall";
                    }
                    else
                    {
                        GameManager.instance.totalNoOfTile++;
                        t_tile.IS_FILLED = false;
                    }
                }
            }
        }
        #endregion

        #region public methods
        public Tile GetTileByPos(float a_XPos,float a_YPos)
        {
            int t_XIndex = (int)a_XPos + (-startXPos);
            int t_YIndex = (int)a_YPos + (-startYPos);
            //Debug.Log(t_XIndex + "  " + t_YIndex+"    " + a_XPos+"  "+a_YPos);
            if (t_YIndex >= row || t_XIndex >= column || t_YIndex < 0 || t_XIndex < 0)
            {
                return null;
            }
            else
            {
                return tileArr[t_YIndex, t_XIndex];
            }
        }
        public Vector2 GetPosByTileNo(int a_XIndex,int a_YIndex)
        {
            int t_XPos = a_XIndex + startXPos;
            int t_YPos = a_YIndex + startYPos;
            return new Vector2(t_XPos, t_YPos);

        }
        public Vector2 GetRandomPos()
        {
            return GetPosByTileNo(Random.Range(1, column-2), Random.Range(1, row-2));
        }
        #endregion
    }
}
