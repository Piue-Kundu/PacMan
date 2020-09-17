using UnityEngine;

namespace PacMan
{
    public class Tile : MonoBehaviour
    {
        #region private variables
        private bool isFilled;
        private BoxCollider2D collider2D;
        #endregion

        #region public fields
        public bool IS_FILLED
        {
            get
            {
                return isFilled;
            }set
            {
                isFilled = value;
                if (!value)
                {
                    GetComponent<SpriteRenderer>().color = Color.black;
                    if (collider2D)
                    {
                        collider2D.enabled = false;
                    }
                }
                else
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                    if (collider2D)
                    {
                        collider2D.enabled = true;
                    }
                }
            }
        }
        #endregion

        #region private methods
        private void Start()
        {
            collider2D = GetComponent<BoxCollider2D>();
            if (isFilled)
            {
                collider2D.enabled = true;
            }
            else
            {
                collider2D.enabled = false;
            }
        }
        #endregion
    }
}
