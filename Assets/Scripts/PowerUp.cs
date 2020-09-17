using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PacMan
{
    public class PowerUp : MonoBehaviour
    {
        #region private variables
        private int interval = 20;
        #endregion

        #region private methods
        private void Start()
        {
            StartCoroutine("genaratePowerUp");
        }

        private IEnumerator genaratePowerUp()
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
                this.transform.position = GameManager.instance.tilemap.GetRandomPos();
            }
        }
        #endregion
    }
}
