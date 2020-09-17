using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PacMan
{
    public class Enemy : MonoBehaviour
    {
        #region piblic variables
        public static event Action ResetPath;
        #endregion

        #region private variables
        //private Rigidbody2D rb;
        [SerializeField] private float speed = 1f;
        private Vector3 dir;
        private Vector3 currentPos;
        private bool play = true;
        private Vector3 direction;
        #endregion

        private void OnEnable()
        {
            PlayerController.PowerUpCollected += onPowerUpCollection;
            PlayerController.FillBlock += onFillBlock;
        }
        private void OnDisable()
        {
            PlayerController.FillBlock -= onFillBlock;
            PlayerController.PowerUpCollected -= onPowerUpCollection;
        }
        private void Start()
        {
            dir = Vector3.up;
            InvokeRepeating("startMove", 0f, 5f);

        }

        private void startMove()
        {
            play = true;
            direction = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), UnityEngine.Random.Range(-10.0f, 10.0f), 0);
        }
        private void Update()
        {
            currentPos = transform.position;
            if (play)
            {
                dir = direction - currentPos;

                dir.z = 0;
                dir.Normalize();
                play = false;
            }
            Vector3 target = dir * speed + currentPos;
            transform.position = Vector3.Lerp(currentPos, target, Time.deltaTime);
        }
        private void OnCollisionEnter2D(Collision2D a_collition)
        {

            if (a_collition.collider.gameObject.name == "Wall")
            {
                CancelInvoke();
                direction = new Vector3(UnityEngine.Random.Range(-3.0f, 3.0f), UnityEngine.Random.Range(-4.0f, 4.0f), 0);
                play = true;
            }
            else if(a_collition.collider.gameObject.name == "Tile" || a_collition.collider.gameObject.name == "Pac")
            {
                //Lose life
                Debug.Log("Lose life");
                ResetPath?.Invoke();
            }
        }
        private void OnCollisionExit2D()
        {
            InvokeRepeating("startMove", 2f, 5f);
        }
        private void onPowerUpCollection()
        {
            speed = speed / 2;
        }
        private void onFillBlock()
        {
            Tile t_tile = GameManager.instance.tilemap.GetTileByPos(this.transform.position.x, this.transform.position.y);
            if (t_tile)
            {
                if (t_tile.IS_FILLED)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
