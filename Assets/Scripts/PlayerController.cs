using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using System;

namespace PacMan
{
    public class PlayerController : MonoBehaviour
    {
        #region public variables
        public static event Action PowerUpCollected;
        public static event Action ReduceLife;
        public static event Action FillBlock;
        #endregion

        #region private variables
        private Vector3 playerPos = new Vector3(-15, 11, 0);
        private Vector3 direction = Vector3.zero;
        private Vector3 position;
        private bool isMoving = true;
        private bool isPlayerMoving = false;
        private Sequence tweenSequence;
        private Vector2 lastDirection;
        private Vector2 lastPosition;
        [SerializeField] private List<Breakpoint> breakPoints = new List<Breakpoint>();
        [SerializeField] private Tilemap tilemap;
        IEnumerator iMovePlayer;
        private Vector2 startPos;
        private float startTime;
        private const float MAX_SWIPE_TIME = 0.5f;
        private const float MIN_SWIPE_DISTANCE = 0.17f;
        private enum DraggedDirection
        {
            Up,
            Down,
            Right,
            Left,
            None
        }
        private DraggedDirection currentDirection;
        private DraggedDirection lastDragDirection;
        #endregion

        #region private methods
        private void OnEnable()
        {
            Enemy.ResetPath += resetPath;   
        }
        private void OnDisable()
        {
            Enemy.ResetPath -= resetPath;
        }
        private void Start()
        {
            tweenSequence = DOTween.Sequence();
            iMovePlayer = movePlayer();
        }

        private void Update()
        {
            currentDirection = DraggedDirection.None;
            if (Input.touches.Length > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    startPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);
                    startTime = Time.time;
                }
                if (t.phase == TouchPhase.Ended)
                {
                    if (Time.time - startTime > MAX_SWIPE_TIME) // press too long
                        return;

                    Vector2 endPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);

                    Vector2 swipe = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);

                    if (swipe.magnitude < MIN_SWIPE_DISTANCE) // Too short swipe
                        return;

                    if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                    { // Horizontal swipe
                        if (swipe.x > 0)
                        {
                            if (currentDirection != DraggedDirection.Left)
                            {
                                currentDirection = DraggedDirection.Right;
                            }
                        }
                        else
                        {
                            if (currentDirection != DraggedDirection.Right)
                            {
                                currentDirection = DraggedDirection.Left;
                            }
                        }
                    }
                    else
                    { // Vertical swipe
                        if (swipe.y > 0)
                        {
                            if (currentDirection != DraggedDirection.Down)
                            {
                                currentDirection = DraggedDirection.Up;
                            }
                        }
                        else
                        {
                            if (currentDirection != DraggedDirection.Up)
                            {
                                currentDirection = DraggedDirection.Down;
                            }
                        }
                    }
                }
            }
            //for editor
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentDirection = DraggedDirection.Right;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentDirection = DraggedDirection.Left;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentDirection = DraggedDirection.Down;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentDirection = DraggedDirection.Up;
            }
            checkMove();
        }

        private void checkMove()
        {
            lastDirection = direction;
            switch (currentDirection)
            {
                case DraggedDirection.Left:
                    this.transform.localEulerAngles = Vector3.zero;
                    this.transform.localScale = new Vector3(-1, 1, 1);
                    direction = new Vector3(-1, 0, 0);
                    isMoving = false;
                    break;
                case DraggedDirection.Right:
                    this.transform.localEulerAngles = Vector3.zero;
                    this.transform.localScale = Vector3.one;
                    direction = new Vector3(1, 0, 0);
                    isMoving = false;
                    break;
                case DraggedDirection.Up:
                    this.transform.localEulerAngles = new Vector3(0, 0, 90);
                    this.transform.localScale = Vector3.one;
                    direction = new Vector3(0, 1, 0);
                    isMoving = false;
                    break;
                case DraggedDirection.Down:
                    this.transform.localEulerAngles = new Vector3(0, 0, 270);
                    this.transform.localScale = Vector3.one;
                    direction = new Vector3(0, -1, 0);
                    isMoving = false;
                    break;
            }

            if (!isMoving)
            {
                isMoving = true;
                StopCoroutine(iMovePlayer);
                StartCoroutine(iMovePlayer);
            }
        }
        IEnumerator movePlayer()
        {
            isPlayerMoving = true;
            while (isMoving)
            {
                lastPosition = this.transform.position;
                yield return new WaitForSeconds(0.2f);
                position = this.transform.position + direction;
                this.transform.position = new Vector3(Mathf.Clamp(position.x, -tilemap.NO_OF_COLUMN / 2 + 1, tilemap.NO_OF_COLUMN / 2), Mathf.Clamp(position.y, -tilemap.NO_OF_ROWS / 2, tilemap.NO_OF_ROWS / 2 - 1), 0);
               
                Tile t_tile = tilemap.GetTileByPos(position.x, position.y);
                if (t_tile)
                {
                    if (breakPoints.FindIndex(x => x.position == new Vector2(this.transform.position.x,this.transform.position.y)) >= 0)
                    {
                        Debug.Log("Lose life...................");
                        isPlayerMoving = false;
                        StopCoroutine(iMovePlayer);
                        resetPath();
                    }
                    else if (t_tile.IS_FILLED)
                    {
                        isPlayerMoving = false;
                        StopCoroutine(iMovePlayer);
                        Tile previousTile = tilemap.GetTileByPos(lastPosition.x, lastPosition.y);
                        if (previousTile != null)
                        {
                            if (previousTile.name != "Wall")
                            {
                                //Debug.Log(lastPosition + "   " + previousTile);
                                breakPoints.Add(new Breakpoint
                                {
                                    direction = lastDirection,
                                    position = lastPosition
                                });
                                calculateFillArea();
                            }
                        }
                        //breakPoints.Add(new Vector2(this.transform.position.x, this.transform.position.y));
                    }
                    else if (breakPoints.FindIndex(x => x.position == new Vector2(this.transform.position.x,this.transform.position.y)) >= 0)
                    {
                        isPlayerMoving = false;
                        StopCoroutine(iMovePlayer);
                        resetPath();
                    }
                    else
                            {
                        breakPoints.Add(new Breakpoint
                        {
                            direction = lastDirection,
                            position = lastPosition
                        }) ;
                        t_tile.IS_FILLED = true;
                        //t_tile.name = "Wall";
                    }
                }
            }
        }
         
        private void calculateFillArea()
        {
            Vector2 t_EndDirection = breakPoints[breakPoints.Count-1].direction;
            Vector2 t_StartPosition = breakPoints[0].position;
            Vector2 t_EndPosition = breakPoints[breakPoints.Count-1].position;
            if (t_EndDirection == new Vector2(0, 1))//End up
            {
                //Debug.Log("End Up");
                if (t_StartPosition.x < t_EndPosition.x)
                {
                    //fill up left
                    fillCheckByX(new Vector2(0, -1),new Vector2(-1,0),"Left");
                }
                else if (t_StartPosition.x == t_EndPosition.x)
                {
                    int t_distanceFromEnd = (int)math.abs(t_StartPosition.x - tilemap.NO_OF_COLUMN / 2);
                    int t_distanceFromStart = (int)math.abs((- tilemap.NO_OF_COLUMN / 2) - t_StartPosition.x);
                    if (t_distanceFromEnd > t_distanceFromStart)
                    {
                        fillCheckByX(new Vector2(0, -1),new Vector2(-1,0),"Left");
                    }
                    else
                    {
                        fillCheckByX(new Vector2(0, -1), new Vector2(1, 0), "Right");
                    }
                }
                else
                {
                    //fill up right
                    fillCheckByX(new Vector2(0, -1), new Vector2(1, 0), "Right");
                }
            }
            else if (t_EndDirection == new Vector2(0, -1))//End down
            {
                if (t_StartPosition.x < t_EndPosition.x)
                {
                    //fill down left
                    fillCheckByX(new Vector2(0, 1), new Vector2(-1, 0), "Left");
                }
                else if (t_StartPosition.x == t_EndPosition.x)
                {
                    int t_distanceFromEnd = (int)math.abs(t_StartPosition.x - tilemap.NO_OF_COLUMN / 2);
                    int t_distanceFromStart = (int)math.abs((-tilemap.NO_OF_COLUMN / 2) - t_StartPosition.x);
                    if (t_distanceFromEnd > t_distanceFromStart)
                    {
                        fillCheckByX(new Vector2(0, 1), new Vector2(-1, 0), "Left");
                    }
                    else
                    {
                        fillCheckByX(new Vector2(0, 1), new Vector2(1, 0), "Right");
                    }
                }
                else
                {
                    //fill down right
                    fillCheckByX(new Vector2(0, 1), new Vector2(1, 0), "Right");
                }
            }
            else if (t_EndDirection == new Vector2(1, 0))//End right
            {
                if (t_StartPosition.y < t_EndPosition.y)
                {
                    //fill down right
                    fillCheckByY(new Vector2(-1, 0), new Vector2(0, -1), "Down");
                }
                else if (t_StartPosition.y == t_EndPosition.y)
                {
                    int t_distanceFromEnd = (int)math.abs(t_StartPosition.y - tilemap.NO_OF_ROWS / 2);
                    int t_distanceFromStart = (int)math.abs((-tilemap.NO_OF_ROWS / 2) - t_StartPosition.y);
                    if (t_distanceFromEnd > t_distanceFromStart)
                    {
                        fillCheckByY(new Vector2(-1, 0), new Vector2(0, -1), "Down");
                    }
                    else
                    {
                        fillCheckByY(new Vector2(-1, 0), new Vector2(0, 1), "Up");
                    }
                }
                else
                {
                    //fill up left
                    fillCheckByY(new Vector2(-1, 0), new Vector2(0, 1), "Up");
                }
            }
            else if (t_EndDirection == new Vector2(-1, 0))//End left
            {
                if (t_StartPosition.y < t_EndPosition.y)
                {
                    //fill down left
                    fillCheckByY(new Vector2(1, 0), new Vector2(0, -1), "Down");
                }
                else if (t_StartPosition.y == t_EndPosition.y)
                {
                    int t_distanceFromEnd = (int)math.abs(t_StartPosition.y - tilemap.NO_OF_ROWS / 2);
                    int t_distanceFromStart = (int)math.abs((-tilemap.NO_OF_ROWS / 2) - t_StartPosition.y);
                    if (t_distanceFromEnd > t_distanceFromStart)
                    {
                        fillCheckByY(new Vector2(1, 0), new Vector2(0, -1), "Down");
                    }
                    else
                    {
                        fillCheckByY(new Vector2(1, 0), new Vector2(0, 1), "Up");
                    }
                }
                else
                {
                    //fill up right
                    fillCheckByY(new Vector2(1, 0), new Vector2(0, 1), "Up");
                }
            }
        }

        private void fillCheckByX(Vector2 a_ExcludeDirection, Vector2 a_ExcludeAnotherDirection,string a_Direction)
        {
            for (int i = breakPoints.Count-1; i > 0; i--)
            {
                if (breakPoints[i].direction != a_ExcludeDirection && breakPoints[i].direction != a_ExcludeAnotherDirection)
                {
                    //scan by x to left  --> 
                    if (a_Direction == "Left")
                    {
                        for (int j = (int)breakPoints[i].position.x - 1; j > -tilemap.NO_OF_COLUMN / 2; j--)
                        {
                            Tile t_tile = tilemap.GetTileByPos(j, breakPoints[i].position.y);
                            if (t_tile != null)
                            {
                                if (t_tile.IS_FILLED == false)
                                {
                                    t_tile.name = "Wall";
                                    GameManager.instance.noOfFilledTile++;
                                    t_tile.IS_FILLED = true;
                                }
                                else if (breakPoints.FindIndex(x => x.position == new Vector2(j, breakPoints[i].position.y)) >= 0)
                                {
                                    break;
                                }
                            }
                        }
                        checkEmpty(a_Direction,breakPoints[breakPoints.Count-1].position);
                    }
                    else if(a_Direction == "Right")
                    {
                        for (int j = (int)breakPoints[i].position.x + 1; j < tilemap.NO_OF_COLUMN / 2; j++)
                        {
                            Tile t_tile = tilemap.GetTileByPos(j, breakPoints[i].position.y);
                            if (t_tile != null)
                            {
                                if (t_tile.IS_FILLED == false)
                                {
                                    t_tile.name = "Wall";
                                    GameManager.instance.noOfFilledTile++;
                                    t_tile.IS_FILLED = true;
                                }
                                else if (breakPoints.FindIndex(x => x.position == new Vector2(j, breakPoints[i].position.y)) >= 0)
                                {
                                    break;
                                }
                            }
                        }
                        checkEmpty(a_Direction,breakPoints[breakPoints.Count-1].position);
                    }
                }
            }
            fillPath();
        }

        private void fillCheckByY(Vector2 a_ExcludeDirection, Vector2 a_ExcludeAnotherDirection, string a_Direction)
        {
            for (int i = breakPoints.Count - 1; i > 0; i--)
            {
                if (breakPoints[i].direction != a_ExcludeDirection && breakPoints[i].direction != a_ExcludeAnotherDirection)
                {
                    if (a_Direction == "Down")
                    {
                        //scan by y to down
                        for (int j = (int)breakPoints[i].position.y - 1; j > -tilemap.NO_OF_ROWS / 2; j--)
                        {
                            Tile t_tile = tilemap.GetTileByPos(breakPoints[i].position.x, j);
                            if (t_tile != null)
                            {
                                if (t_tile.IS_FILLED == false)
                                {
                                    t_tile.name = "Wall";
                                    GameManager.instance.noOfFilledTile++;
                                    t_tile.IS_FILLED = true;
                                }
                                else if(breakPoints.FindIndex(x=> x.position == new Vector2(breakPoints[i].position.x, j))>=0)
                                {
                                    break;
                                }
                            }
                        }
                        checkEmpty(a_Direction, breakPoints[breakPoints.Count - 1].position);
                    }
                    else
                    {
                        for (int j = (int)breakPoints[i].position.y + 1; j < tilemap.NO_OF_ROWS / 2; j++)
                        {
                            Tile t_tile = tilemap.GetTileByPos(breakPoints[i].position.x, j);
                            if (t_tile != null)
                            {
                                if (t_tile.IS_FILLED == false)
                                {
                                    t_tile.name = "Wall";
                                    GameManager.instance.noOfFilledTile++;
                                    t_tile.IS_FILLED = true;
                                }
                                else if (breakPoints.FindIndex(x => x.position == new Vector2(breakPoints[i].position.x, j)) >= 0)
                                {
                                    break;
                                }
                            }
                        }
                        checkEmpty(a_Direction, breakPoints[breakPoints.Count - 1].position);
                    }
                }
            }
            fillPath();
        }

        private void checkEmpty(string a_FillDirection, Vector2 a_LastPoint)
        {
            switch (a_FillDirection)
            {
                case "Left":
                    for (int i = (int)a_LastPoint.x; i > -tilemap.NO_OF_COLUMN / 2; i--)
                    {
                        bool t_IsFullLineFilled = true;
                        bool t_IsFullLineEmpty = true;
                        for (int j = -tilemap.NO_OF_ROWS / 2; j < tilemap.NO_OF_ROWS / 2; j++)
                        {
                            Tile t_tile = tilemap.GetTileByPos(i, j);
                            if (t_tile)
                            {
                                if(t_tile.IS_FILLED == false)
                                {
                                    t_IsFullLineFilled = false;
                                    break;
                                }
                                else
                                {
                                    t_IsFullLineEmpty = false;
                                }
                            }
                        }
                        if (t_IsFullLineEmpty)
                        {
                            return;
                        }
                        if (t_IsFullLineFilled)
                        {
                            //fill all left tiles
                            fillLeftTiles(i - 1);
                        }
                       
                    }
                    break;
                case "Right":
                    for (int i = (int)a_LastPoint.x; i < tilemap.NO_OF_COLUMN / 2; i++)
                    {
                        bool t_IsFullLineFilled = true;
                        bool t_IsFullLineEmpty = true;
                        for (int j = -tilemap.NO_OF_ROWS / 2; j < tilemap.NO_OF_ROWS / 2; j++)
                        {
                            Tile t_tile = tilemap.GetTileByPos(i, j);
                            if (t_tile)
                            {
                                if (t_tile.IS_FILLED == false)
                                {
                                    t_IsFullLineFilled = false;
                                    break;
                                }
                                else
                                {
                                    t_IsFullLineEmpty = false;
                                }
                            }
                        }
                        if (t_IsFullLineEmpty)
                        {
                            return;
                        }
                        if (t_IsFullLineFilled)
                        {
                            //fill all right tiles
                            fillRightTiles(i + 1);
                        }
                    }
                    break;
                case "Down":
                    for (int i = (int)a_LastPoint.x; i > -tilemap.NO_OF_ROWS / 2; i--)
                    {
                        bool t_IsFullLineFilled = true;
                        bool t_IsFullLineEmpty = true;
                        for (int j = -tilemap.NO_OF_COLUMN / 2; j < tilemap.NO_OF_COLUMN / 2; j++)
                        {
                            Tile t_tile = tilemap.GetTileByPos(j, i);
                            if (t_tile)
                            {
                                if (t_tile.IS_FILLED == false)
                                {
                                    t_IsFullLineFilled = false;
                                    break;
                                }
                                else
                                {
                                    t_IsFullLineEmpty = false;
                                }
                            }
                        }
                        if (t_IsFullLineEmpty)
                        {
                            return;
                        }
                        if (t_IsFullLineFilled)
                        {
                            //fill all down tiles
                            fillDownTiles(i - 1);
                        }
                    }
                    break;
                case "Up":
                    for (int i = (int)a_LastPoint.y; i < tilemap.NO_OF_ROWS / 2; i++)
                    {
                        bool t_IsFullLineFilled = true;
                        bool t_IsFullLineEmpty = true;
                        for (int j = -tilemap.NO_OF_COLUMN / 2; j < tilemap.NO_OF_COLUMN / 2; j++)
                        {
                            Tile t_tile = tilemap.GetTileByPos(j, i);
                            if (t_tile)
                            {
                                if (t_tile.IS_FILLED == false)
                                {
                                    t_IsFullLineFilled = false;
                                    break;
                                }
                                else
                                {
                                    t_IsFullLineEmpty = false;
                                }
                            }
                        }
                        if (t_IsFullLineEmpty)
                        {
                            return;
                        }
                        if (t_IsFullLineFilled)
                        {
                            //fill all up tiles
                            fillUpTiles(i + 1);
                        }
                    }
                    break;
            }    
        }

        private void fillLeftTiles(int a_XPos)
        {
            for (int i = a_XPos; i > -tilemap.NO_OF_COLUMN / 2; i--)
            {
                for (int j = -tilemap.NO_OF_ROWS / 2; j < tilemap.NO_OF_ROWS / 2; j++)
                {
                    Tile t_tile = tilemap.GetTileByPos(i,j);

                    if (t_tile != null)
                    {
                        //if (t_tile.name != "Wall")
                        if (t_tile.IS_FILLED == false)
                        {
                            t_tile.name = "Wall";
                            GameManager.instance.noOfFilledTile++;
                            t_tile.IS_FILLED = true;
                        }
                    }
                }
            }
        }

        private void fillRightTiles(int a_XPos)
        {
            for (int i = a_XPos; i < tilemap.NO_OF_COLUMN / 2; i++)
            {
                for (int j = -tilemap.NO_OF_ROWS / 2; j < tilemap.NO_OF_ROWS / 2; j++)
                {
                    Tile t_tile = tilemap.GetTileByPos(i, j);

                    if (t_tile != null)
                    {
                        //if (t_tile.name != "Wall")
                        if (t_tile.IS_FILLED == false)
                        {
                            t_tile.name = "Wall";
                            GameManager.instance.noOfFilledTile++;
                            t_tile.IS_FILLED = true;
                        }
                    }
                }
            }
        }
        private void fillUpTiles(int a_YPos)
        {
            for (int i = a_YPos; i < tilemap.NO_OF_ROWS / 2; i++)
            {
                for (int j = -tilemap.NO_OF_COLUMN / 2; j < tilemap.NO_OF_COLUMN / 2; j++)
                {
                    Tile t_tile = tilemap.GetTileByPos(j, i);

                    if (t_tile != null)
                    {
                        //if (t_tile.name != "Wall")
                        if (t_tile.IS_FILLED == false)
                        {
                            t_tile.name = "Wall";
                            GameManager.instance.noOfFilledTile++;
                            t_tile.IS_FILLED = true;
                        }
                    }
                }
            }
        }

        private void fillDownTiles(int a_YPos)
        {
            for (int i = a_YPos; i > -tilemap.NO_OF_ROWS / 2; i--)
            {
                for (int j = -tilemap.NO_OF_COLUMN / 2; j < tilemap.NO_OF_COLUMN / 2; j++)
                {
                    Tile t_tile = tilemap.GetTileByPos(j, i);

                    if (t_tile != null)
                    {
                        //if (t_tile.name != "Wall")
                        if (t_tile.IS_FILLED == false)
                        {
                            t_tile.name = "Wall";
                            GameManager.instance.noOfFilledTile++;
                            t_tile.IS_FILLED = true;
                        }
                    }
                }
            }
        }

        private void fillPath()
        {
            breakPoints.Add(new Breakpoint
            {
                direction = lastDirection,
                position = lastPosition,
            });
            for (int i = 1; i < breakPoints.Count; i++)
            {
                Tile t_tile = tilemap.GetTileByPos(breakPoints[i].position.x, breakPoints[i].position.y);
                if (t_tile)
                {
                    t_tile.name = "Wall";
                    GameManager.instance.noOfFilledTile++;
                }
            }
            breakPoints.Clear();
            FillBlock?.Invoke();
        }

        private void resetPath()
        {
            if (breakPoints.Count > 0)
            {
                breakPoints.Add(new Breakpoint
                {
                    direction = lastDirection,
                    position = lastPosition,
                });
                for (int i = 1; i < breakPoints.Count; i++)
                {
                    Tile t_tile = tilemap.GetTileByPos(breakPoints[i].position.x, breakPoints[i].position.y);
                    if (t_tile)
                    {
                        t_tile.IS_FILLED = false;
                    }
                }
                breakPoints.Clear();
                this.transform.position = playerPos;
                this.transform.localEulerAngles = Vector3.zero;
                this.transform.localScale = Vector3.one;
                StopCoroutine(iMovePlayer);
                ReduceLife?.Invoke();
            }
        }

        private void OnTriggerEnter2D(Collider2D a_Collition)
        {

            if (a_Collition.gameObject.name == "SlowDown")
            {
                Debug.Log("Collect PowerUp");
                a_Collition.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                PowerUpCollected?.Invoke();
            }
        }

        #endregion
    }
}
[System.Serializable]
public struct Breakpoint
{
    public Vector2 position;
    public Vector2 direction;
}
