using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public enum BlockState
    {
        None,
        Fall,
        Bounce,
        Deactivated,
    }

    public bool canMove = true;

    public Vector3Int boardPos;
    public Vector2 targetPosition;

    public int typeNumber = -1;
    public bool IsMatching = false;
    public Match curMatch = null;

    public BlockState curState = BlockState.None;
    public float bouncingTime = 0.0f;
    virtual public void Reset()
    {
        canMove = true;
        IsMatching = false;
        curMatch = null;
        curState = BlockState.None;
    }

    public void InitBlock(Vector3Int _boardPos)
    {
        boardPos = _boardPos;
        transform.localPosition = boardPos;
    }
    public void Deactivated()
    {
        curState = BlockState.Deactivated;
    }

    public void RenewPosition(Vector3Int _boardPos)
    {
        boardPos = _boardPos;
    }
    public void EndFalling()
    {
        bouncingTime = 0.0f;
        curState = BlockState.None;
    }
    public void AddBouncingTime(float _time)
    {
        bouncingTime += _time;
    }

    
}
