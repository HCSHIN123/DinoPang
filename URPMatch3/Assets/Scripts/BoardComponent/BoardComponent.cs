using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class BoardComponent : MonoBehaviour
{
    public Block curBlock;
    public Block nextBlock;

    public RectTransform rt;
    public bool isFixed = false;
    public bool isMoving = false;

    public float moveSpeed = 5f;

    public bool CanFall => (curBlock == null || (curBlock.canMove && curBlock.curMatch == null)) && !isFixed;
    public bool BlockFall => isFixed || (curBlock != null && !curBlock.canMove);


    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }
    public bool HasBlock()
    {
        return curBlock != null;
    }

    public bool CanRemove()
    {
        return !isFixed;
    }      

    public bool Empty()
    {
        return curBlock == null && nextBlock == null;
    }
    
    

    public void NextBlockMoveToMe()
    {
        nextBlock.transform.localPosition = nextBlock.transform.localPosition = Vector3.MoveTowards(nextBlock.transform.localPosition,
            transform.localPosition, moveSpeed * Time.deltaTime);
        if (nextBlock?.transform.localPosition == transform.localPosition)
            EndMoveToMe();
        else 
            isMoving = true;
    }

    private void EndMoveToMe()
    {
        
        curBlock = nextBlock;
        nextBlock = null;
        isMoving = false;
    }

}
