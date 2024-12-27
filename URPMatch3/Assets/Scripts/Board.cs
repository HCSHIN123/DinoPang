using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static Unity.Collections.AllocatorManager;

public class Board : MonoBehaviour
{
    public AudioSource popSound;
    

    public BlockFactory factory;

    public enum ComponentTypes
    {
        Normal = 0,
        NormalWall,
        RespawnWall,
    }
    // TODO:���� ������ ����, �ش� ������ ���� ���� ��ġ������ �˰Բ� ���������̼� ������ �Ǵ� �ý��� ����
    // 주석깨짐    
    public static readonly Vector3Int[] neighbours =
        {
            Vector3Int.up,
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.left
        };
    public enum SwapState
    {
        Swapping,
        Return,
        None,
    }
    private SwapState curSwapState = SwapState.None;

    public BoardComponent componentPrefab;
    public int width = 9;
    public int height = 9;

    public InputAction ClickAction;
    public InputAction ClickPosition;
    private Dictionary<int, Block> dicBlockTypes;

    // Dictionary�� �����ϴ� ���� : ��ġ�� �������� Block�� �����ϰ� �Ǵ� ��찡 ���Ƽ� Vector2Int�� Ű������ ������ ���� O(1)
    private Dictionary<Vector3Int, BoardComponent> dicComponents = new();
    private (Vector3Int, Vector3Int) tupleSwapKeys;
    private Vector3Int invalidVector3 = new Vector3Int(-1, -1, -1);
    private List<Vector3Int> removedList = new List<Vector3Int>();

    private List<Match> curMatches = new List<Match>();     // ���缺���� ��ġ
    private List<Vector3Int> emptyComponents = new List<Vector3Int>();  // ������ ������ ��������Ʈ���
    private List<Vector3Int> movingComponents = new List<Vector3Int>(); // �����̴� ������ �ִ� ������Ʈ���
    private List<Vector3Int> newMovingComponents = new List<Vector3Int>();  // �����߰��Ǵ� �����̴º��ϵ� ���
    private List<Vector3Int> extraCheckComponents = new List<Vector3Int>();  // ���Ϲ����� �Ϸ�ǰ� ����üũ�� ������Ʈ ���

    private bool canInput = true;
    void Update()
    {
        InputCheck();
        if(curSwapState != SwapState.None)
        {
            SwapBlocks();
            return;
        }
        if (movingComponents.Count > 0)
            MovingProcess();
        else
            canInput = true;
        if (extraCheckComponents.Count > 0)
            ExtraMatchCheck();

        if (curMatches.Count > 0)
            MatchingProcess();
        if(emptyComponents.Count > 0)
        {
            SetMovableBlocks();
            foreach(Vector3Int key in emptyComponents)
            {
                //Debug.Log("EMP" + key);
            }
        }
        if(newMovingComponents.Count > 0)
        {
           // Debug.Log("MOVECOMP" + emptyComponents.Count);
            movingComponents.AddRange(newMovingComponents);
            newMovingComponents.Clear();
        }
    }

    public void InitBoard(int[,] _boardInfo)
    {
        
        factory.InitFactory();
        dicBlockTypes = new Dictionary<int, Block>();
        foreach (var block in factory.normalBlockPrefabs)
        {
            dicBlockTypes.Add(block.typeNumber, block);
        }
        // BoardComponent�� ���� �ʱ�ȭ
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                
                if (_boardInfo[y, x] == 1)
                    continue;

                Vector3Int key = new Vector3Int(x, y);
                BoardComponent bc = Instantiate(componentPrefab);
                dicComponents.Add(key, bc);
                bc.transform.localPosition = key;
            }
        }

        // ���� Ÿ���� üũ�ϰ� ������ ��ġ
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                if (_boardInfo[y, x] == 1)
                    continue;
                Vector3Int curKey = new Vector3Int(x, y, 0);

                // ������ �̹� �ִ��� üũ
                if (!dicComponents.TryGetValue(curKey, out BoardComponent cur) || cur.curBlock != null)
                    continue;

                List<int> usableTypes = new List<int>(dicBlockTypes.Keys);

                // �ֺ� ���� Ÿ���� Ȯ���ϰ� ����� �� ���� Ÿ���� ����
                CheckNeighbourTypes(curKey, usableTypes);

                // ��� ������ Ÿ�Կ��� ������ �����ϰ� ����
                int selected = usableTypes[Random.Range(0, usableTypes.Count)];
                CreateBlockAt(curKey, selected);
                
            }
        }

        ClickAction.Enable();
        ClickPosition.Enable();
    }

    // �ֺ� ���� Ÿ���� üũ�ϰ� ��� ������ Ÿ�� ��Ͽ��� �����ϴ� �Լ�
    private void CheckNeighbourTypes(Vector3Int curKey, List<int> usableTypes)
    {
        int leftType = -1, downType = -1, rightType = -1, upType = -1;

        if (dicComponents.TryGetValue(curKey + Vector3Int.left, out BoardComponent leftComp) && leftComp.curBlock != null)
        {
            leftType = leftComp.curBlock.typeNumber;
            if (dicComponents.TryGetValue(curKey + Vector3Int.left * 2, out BoardComponent leftleft) &&
                leftleft.curBlock != null && leftType == leftleft.curBlock.typeNumber)
            {
                usableTypes.Remove(leftType);
            }
        }

        if (dicComponents.TryGetValue(curKey + Vector3Int.down, out BoardComponent downComp) && downComp.curBlock != null)
        {
            downType = downComp.curBlock.typeNumber;
            if (dicComponents.TryGetValue(curKey + Vector3Int.down * 2, out BoardComponent downdown) &&
                downdown.curBlock != null && downType == downdown.curBlock.typeNumber)
            {
                usableTypes.Remove(downType);
            }
            if (leftType != -1 && leftType == downType &&
                dicComponents.TryGetValue(curKey + Vector3Int.left + Vector3Int.down, out BoardComponent downleft) &&
                downleft.curBlock != null && downleft.curBlock.typeNumber == downType)
            {
                usableTypes.Remove(leftType);
            }
        }

        if (dicComponents.TryGetValue(curKey + Vector3Int.right, out BoardComponent rightComp) && rightComp.curBlock != null)
        {
            rightType = rightComp.curBlock.typeNumber;
            if (dicComponents.TryGetValue(curKey + Vector3Int.right * 2, out BoardComponent rightright) &&
                rightright.curBlock != null && rightType == rightright.curBlock.typeNumber)
            {
                usableTypes.Remove(rightType);
            }
            if (rightType == downType &&
                dicComponents.TryGetValue(curKey + Vector3Int.right + Vector3Int.down, out BoardComponent downright) &&
                downright.curBlock != null && downright.curBlock.typeNumber == rightType)
            {
                usableTypes.Remove(rightType);
            }
        }

        if (dicComponents.TryGetValue(curKey + Vector3Int.up, out BoardComponent upComp) && upComp.curBlock != null)
        {
            upType = upComp.curBlock.typeNumber;
            if (dicComponents.TryGetValue(curKey + Vector3Int.up * 2, out BoardComponent upup) &&
                upup.curBlock != null && upType == upup.curBlock.typeNumber)
            {
                usableTypes.Remove(upType);
            }
            if (upType == rightType &&
                dicComponents.TryGetValue(curKey + Vector3Int.right + Vector3Int.up, out BoardComponent upright) &&
                upright.curBlock != null && upright.curBlock.typeNumber == upType)
            {
                usableTypes.Remove(upType);
            }
            if (upType == leftType &&
                dicComponents.TryGetValue(curKey + Vector3Int.left + Vector3Int.up, out BoardComponent upleft) &&
                upleft.curBlock != null && upleft.curBlock.typeNumber == upType)
            {
                usableTypes.Remove(upType);
            }
        }
    }

    void ExtraMatchCheck()
    {
        foreach(Vector3Int pos in extraCheckComponents)
        {
            CheckMatch(pos);
        }
        extraCheckComponents.Clear();
    }


    public Match CreateNewMatch(Vector3Int _mainPos)
    {
        Match match = new Match()
        {
            MainPos = _mainPos,
        };
        curMatches.Add(match);
        return match;
    }

    
    private Block CreateBlockAt(Vector3Int _key, int _selected)
    {
        //if (_blockPrefab == null)
        //    _blockPrefab = factory.blockPrefabs[Random.Range(0, factory.blockPrefabs.Length)];

        if (dicComponents[_key].curBlock != null)
        {
            Destroy(dicComponents[_key].curBlock.gameObject);
        }

        Block block = factory.GetNewBlock(_selected);
        dicComponents[_key].curBlock = block;
        block.InitBlock(_key);
        return block;
    }

    private void RespawnBlock(Vector3Int _spawnPos)
    {
        Block block = factory.GetNewBlock(Random.Range(0, factory.normalBlockPrefabs.Length));
        dicComponents[_spawnPos].nextBlock = block;
        block.curState = Block.BlockState.Fall;
        newMovingComponents.Add(_spawnPos);
        block.InitBlock(_spawnPos);
        block.transform.localPosition = _spawnPos + Vector3Int.up;
        if (emptyComponents.Contains(_spawnPos))
            emptyComponents.Remove(_spawnPos);

    }

    private bool CanSpawn(Vector3Int _curPos)
    {
        for(int i = _curPos.y; i < height; ++i)
        {
            Vector3Int pos = new Vector3Int(_curPos.x, i);
            if (!dicComponents.ContainsKey(pos))
                return false;
        }
        return true;
    }

    private void SetMovableBlocks()
    { 
        for(int i = 0; i < emptyComponents.Count; ++i)
        {
            Vector3Int curPos = emptyComponents[i];

            if (!dicComponents[curPos].Empty())
            {
                emptyComponents.RemoveAt(i);
                i--;
                continue;
            }


            Vector3Int upPos = curPos + Vector3Int.up;
            bool isUpAvailable = dicComponents.TryGetValue(upPos, out BoardComponent up);
            bool isUpEmpty = isUpAvailable && up.Empty() && !CanSpawn(curPos); // ���� ���� ������ �Ұ����� ��Ȳ

            if (isUpAvailable && up.curBlock != null && up.CanFall)
            {
                Block nextBlock = up.curBlock;
                dicComponents[curPos].nextBlock = nextBlock;
                up.curBlock = null;

                // moveTimer
                nextBlock.curState = Block.BlockState.Fall;
                newMovingComponents.Add(curPos);
                emptyComponents.Add(upPos);
                emptyComponents.Remove(curPos);
            }
            else if((isUpEmpty || !isUpAvailable || up.BlockFall) && dicComponents.TryGetValue(upPos + Vector3Int.right, out BoardComponent upright) &&
                upright.curBlock != null && upright.CanFall)
            {
                Block nextBlock = upright.curBlock;
                dicComponents[curPos].nextBlock= nextBlock;
                upright.curBlock = null;

                // moveTimer
                nextBlock.curState = Block.BlockState.Fall;
                newMovingComponents.Add(curPos);

                emptyComponents.Add(upPos + Vector3Int.right);
                emptyComponents.Remove(curPos);
            }
            else if((isUpEmpty || !isUpAvailable || up.BlockFall) && dicComponents.TryGetValue(upPos + Vector3Int.left, out BoardComponent upleft) &&
                upleft.curBlock != null && upleft.CanFall)
            {
                Block nextBlock = upleft.curBlock;
                dicComponents[curPos].nextBlock = nextBlock;
                upleft.curBlock = null;
                nextBlock.curState = Block.BlockState.Fall;
                // moveTimer
                newMovingComponents.Add(curPos);

                emptyComponents.Add(upPos + Vector3Int.left);
                emptyComponents.Remove(curPos);
            }
            else if ((curPos + Vector3Int.up).y >= height)  // curPos�� ������ �����̶�� �ű⼭ �������ǰ�
                RespawnBlock(curPos);
            //else if()


        }
    }

   

    private void MovingProcess()
    {
        canInput = false;

        movingComponents.Sort((a, b) => // �Ʒ��� �ִ� ���� ���� �̵��� ������ �������ֱ� ���� ����
        {
            int yCmp = a.y.CompareTo(b.y);
            if (yCmp == 0)
            {
                return a.x.CompareTo(b.x); // �ϴ������켱 TODO:�������� �ϴ� �͵� ����
            }
            return yCmp;
        });

        for(int i = 0; i < movingComponents.Count; i++) // �����ϴܺ��� ��ȸ
        {
            Vector3Int curPos = movingComponents[i];
            BoardComponent me = dicComponents[curPos];

            if(me.nextBlock != null && me.curBlock != null) // ���������� ��Ȳ ����ó��
            {
                //error
                Debug.Log("MOVING_ERRER");
                continue;
            }

            if(me.nextBlock?.curState == Block.BlockState.Fall) // if(���� ���������� �������»���) ��ǥ��ġ�� �̵�
            {
                Block block = me.nextBlock;
                float fallSpeed = 7.0f; // TODO : ����ȭ

                block.transform.localPosition = Vector3.MoveTowards(block.transform.localPosition, curPos, Time.deltaTime*fallSpeed); // �̵�
                int rotDir; // ȸ��
                if(curPos.x != block.transform.localPosition.x)
                {
                    rotDir = curPos.x > block.transform.localPosition.x ? 1 : -1;
                    block.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(curPos.y, curPos.x) * rotDir * Mathf.Rad2Deg, Vector3.forward);
                }

                if(block.transform.localPosition == curPos) // ��ǥ��ġ����
                {
                    block.transform.rotation = Quaternion.identity;
                    movingComponents.RemoveAt(i); // �����̴º�����Ͽ��� ����
                    i--;

                    me.nextBlock = null; // ����������Ʈ�� �������� ����
                    me.curBlock = block; // ������ ������ ����������� ����
                    block.RenewPosition(curPos); // ������ ������ġ ����

                    // �Ʒ��ʿ� ������� �ִٸ� ���ο� ��ġ�� �ٽ� �����̱�
                    if (emptyComponents.Contains(curPos + Vector3Int.down) && 
                        dicComponents.TryGetValue(curPos + Vector3Int.down, out BoardComponent down)) // �ٷξƷ�
                    {
                        me.curBlock = null; // ����������Ʈ�� ���� �ٸ� ������ ���� �� �ְ� �Ѵ�
                        down.nextBlock = block; // �Ʒ������� ���������� ���� ������ ������ �������� ����

                        Vector3Int target = curPos + Vector3Int.down; // ��ǥ��ġ
                        newMovingComponents.Add(target); // �߰��� �����̴º��ϸ�Ͽ� �ش���ǥŰ �߰�

                        emptyComponents.Remove(target); // �ش���ǥŰ�� �̹��� ä�������� ���Ͽ��� ����
                        emptyComponents.Add(curPos);    // ���� ���� ��ǥŰ�� �̹��� ������������ ���Ͽ� �߰�

                        if ((curPos + Vector3Int.up).y >= height)
                            RespawnBlock(curPos);

                    }
                    // ���� ���ʿ� ������Ʈ�� ���µ� ���ʾƷ����� ��������Ʈ�� �����Ҷ�, ���� ä���
                    else if ((!dicComponents.TryGetValue(curPos + Vector3Int.left, out BoardComponent left) || left.BlockFall) &&
                        emptyComponents.Contains(curPos + Vector3Int.down + Vector3Int.left) &&
                        dicComponents.TryGetValue(curPos + Vector3Int.down + Vector3Int.left, out BoardComponent downleft))
                    {
                        me.curBlock = null;
                        downleft.nextBlock = block;

                        Vector3Int target = curPos + Vector3Int.down + Vector3Int.left;
                        newMovingComponents.Add(target);

                        emptyComponents.Remove(target);
                        emptyComponents.Add(curPos);

                        if ((curPos + Vector3Int.up).y >= height)
                            RespawnBlock(curPos);
                    } // �Ʒ� �����ʵ� ��������
                    else if((!dicComponents.TryGetValue(curPos + Vector3Int.right, out BoardComponent right) || right.BlockFall) &&
                        emptyComponents.Contains(curPos + Vector3Int.down + Vector3Int.right) && dicComponents.TryGetValue(curPos + Vector3Int.down + 
                        Vector3Int.right, out BoardComponent downright))
                    {
                        me.curBlock = null;
                        downright.nextBlock = block;

                        Vector3Int target = curPos + Vector3Int.down + Vector3Int.right;
                        newMovingComponents.Add(target);
                        emptyComponents.Remove(target);
                        emptyComponents.Add(curPos);

                        ////////////////// ������ ��Ȳ///////////////////
                        if ((curPos + Vector3Int.up).y >= height)
                            RespawnBlock(curPos);
                    }
                    else
                    {
                        block.EndFalling(); //None���� �����ϴ� �Լ�
                        extraCheckComponents.Add(curPos); // ��ġ�� ���Ӱ� ���õǾ� �߰����� ��ġüũ �ʿ�
                    }

                }
                //bouncin
                else if(me.curBlock?.curState == Block.BlockState.None)
                {
                    movingComponents.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    private void MatchingProcess()
    {
        for(int i = 0; i < curMatches.Count; ++i) 
        {
            Match match = curMatches[i];
            for (int j = 0; j < match.MatchingBlocks.Count; j++)
            {
                Vector3Int pos = match.MatchingBlocks[j];
                Block block = dicComponents[pos].curBlock;
                if (block == null)
                {
                    match.MatchingBlocks.RemoveAt(j);
                    j--;
                    continue;
                }

                if(block.curState == Block.BlockState.None)
                {
                    if(movingComponents.Contains(pos))
                        movingComponents.Remove(pos);
                    if(newMovingComponents.Contains(pos))
                        newMovingComponents.Remove(pos);
                }
                //Destroy(dicComponents[pos].curBlock?.gameObject);
                factory.RemoveBlock(block);
                dicComponents[pos].curBlock = null;

                match.MatchingBlocks.RemoveAt(j);
                j--;

                if (match.specialBlock != null && match.MainPos == pos)
                {
                    dicComponents[pos].curBlock = match.specialBlock;
                    dicComponents[pos].curBlock.transform.localPosition = pos;
                    dicComponents[pos].curBlock.boardPos = pos;
                }
                else
                    emptyComponents.Add(pos);
            }
            if(match.MatchingBlocks.Count == 0)
            {
                curMatches.RemoveAt(i);
                i--;
            }
        }

    }
    #region INPUT+SWAP
    private void InputCheck()
    {
        var pressedThisFrame = ClickAction.WasPressedThisFrame();
        var releasedThisFrame = ClickAction.WasReleasedThisFrame();
        Vector2 clickPos = ClickPosition.ReadValue<Vector2>();

        if (pressedThisFrame)
        {
            GetBlockAtPosition(clickPos);
        }
        else if (tupleSwapKeys.Item1 != invalidVector3 && releasedThisFrame)
        {
            foreach (var dir in neighbours)
            {
                Vector3Int curPos = dir + tupleSwapKeys.Item1;
                
                if (dicComponents.TryGetValue(curPos, out var com) && com.curBlock != null &&
                    RectTransformUtility.RectangleContainsScreenPoint(com.rt, clickPos, Camera.main))
                {
                    tupleSwapKeys.Item2 = curPos;

                    if (dicComponents.TryGetValue(tupleSwapKeys.Item1, out var com1) && dicComponents.TryGetValue(tupleSwapKeys.Item2, out var com2))
                    {
                        com1.nextBlock = com2.curBlock;
                        com2.nextBlock = com1.curBlock;

                        curSwapState = SwapState.Swapping;
                    }
                }
            }
        }
    }
    private void GetBlockAtPosition(Vector2 screenPosition)
    {
        foreach(Vector3Int curKey in dicComponents.Keys)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(dicComponents[curKey].rt, screenPosition, Camera.main) &&
                dicComponents[curKey].curBlock != null)
            {
                tupleSwapKeys.Item1 = curKey;
                return;
            }
        }
    }
    
    private void SwapBlocks()
    {
        if(curSwapState == SwapState.Swapping)
        {
            dicComponents[tupleSwapKeys.Item1].NextBlockMoveToMe();
            dicComponents[tupleSwapKeys.Item1].curBlock.RenewPosition(tupleSwapKeys.Item1);
            dicComponents[tupleSwapKeys.Item2].NextBlockMoveToMe();
            dicComponents[tupleSwapKeys.Item2].curBlock.RenewPosition(tupleSwapKeys.Item2);

            if(!dicComponents[tupleSwapKeys.Item1].isMoving && !dicComponents[tupleSwapKeys.Item2].isMoving)
            {
                if (dicComponents.TryGetValue(tupleSwapKeys.Item1, out var com1) && dicComponents.TryGetValue(tupleSwapKeys.Item2, out var com2))
                {
                    bool checkItem1 = CheckMatch(tupleSwapKeys.Item1);
                    bool checkItem2 = CheckMatch(tupleSwapKeys.Item2);

                    if (checkItem1 || checkItem2)
                    {
                        EndSwap();
                        return;
                    }
                    else
                    {
                        com1.nextBlock = com2.curBlock;
                        com2.nextBlock = com1.curBlock;
                        curSwapState = SwapState.Return;
                        return;
                    }
                }

                EndSwap();
            }
            return;
        }
        else if(curSwapState == SwapState.Return)
        {
            dicComponents[tupleSwapKeys.Item1].NextBlockMoveToMe();
            dicComponents[tupleSwapKeys.Item1].curBlock.RenewPosition(tupleSwapKeys.Item1);
            dicComponents[tupleSwapKeys.Item2].NextBlockMoveToMe();
            dicComponents[tupleSwapKeys.Item2].curBlock.RenewPosition(tupleSwapKeys.Item2);

            if (!dicComponents[tupleSwapKeys.Item1].isMoving && !dicComponents[tupleSwapKeys.Item2].isMoving)
                EndSwap();
        }
    }

    private void EndSwap()
    {
        dicComponents[tupleSwapKeys.Item1].nextBlock = null;
        dicComponents[tupleSwapKeys.Item2].nextBlock = null;
        tupleSwapKeys.Item1 = invalidVector3;
        tupleSwapKeys.Item2 = invalidVector3;
        curSwapState = SwapState.None;
    }
    #endregion
    private bool CheckMatch(Vector3Int _mainPos)
    {
        if (!dicComponents.TryGetValue(_mainPos, out var mainCom) || mainCom.curBlock == null) // ������ �ִ� ��Ȳ
            return false;
        if (mainCom.curBlock.curMatch != null)
            return false;

        List<Vector3Int> sameList = new List<Vector3Int>(); // ���� Ÿ�� ���� ��ǥ���
        List<Vector3Int> checkedList = new List<Vector3Int>(); //�̹� üũ�� ���
        Queue<Vector3Int> checkingQueue = new Queue<Vector3Int>(); // �˻��� ��ǥ�� ť

        checkingQueue.Enqueue(_mainPos); 
        while (checkingQueue.Count > 0) // �ʺ�켱Ž�� ����
        {
            Vector3Int curPos = checkingQueue.Dequeue();

            sameList.Add(curPos);
            checkedList.Add(curPos);

            foreach (Vector3Int dir in neighbours)
            {
                Vector3Int nextPos = curPos + dir;

                if (checkedList.Contains(nextPos))
                    continue;

                if (dicComponents.TryGetValue(curPos + dir, out var compared) && compared.curBlock?.typeNumber == mainCom.curBlock.typeNumber)
                {
                    checkingQueue.Enqueue(nextPos);
                }
            }
        }


        List<Vector3Int> shapeMatch = new();
        bool[] specialMatchs = new bool[(int)DefineInfo.SpecialMatchType.end]{ false, false, false, false };// 5,  cross, 4, 2x2

        if (sameList.Count > 3)
        {
            foreach(Vector3Int pos in sameList)
            {
                Vector3Int nextPos;
                List<Vector3Int> rowList = new();
                List<Vector3Int> colList = new();

                foreach(Vector3Int dir in neighbours)
                {
                    nextPos = pos + dir;
                    while (sameList.Contains(nextPos))
                    {
                        if (dir.x == 0)
                            colList.Add(nextPos);
                        else
                            rowList.Add(nextPos);
                        nextPos += dir;
                    }
                }

                if (rowList.Count >= 2 && colList.Count >= 2) //ũ�ν�����
                {
                    shapeMatch.AddRange(rowList);
                    shapeMatch.AddRange(colList);
                    shapeMatch.Add(pos);
                    specialMatchs[1] = true;
                }
                else if(rowList.Count >= 1 && colList.Count >= 1) // 2x2����
                {
                    Vector3Int[] updown = { Vector3Int.up, Vector3Int.down };

                    foreach(Vector3Int rowPos in rowList)
                    {
                        foreach(Vector3Int rowupdown in updown)
                        {
                            if(sameList.Contains(rowPos + rowupdown) && sameList.Contains(pos + rowupdown))
                            {
                                shapeMatch.AddRange(rowList);
                                shapeMatch.AddRange(colList);
                                shapeMatch.Add(pos);
                                specialMatchs[3] = true;
                            }
                        }
                        
                    }
                }

            }
        }
        




        List<Vector3Int> lineMatch = new List<Vector3Int>();
        foreach (Vector3Int pos in sameList)
        {
            foreach (Vector3Int dir in neighbours)
            {
                if (!sameList.Contains(pos + dir))
                {
                    List<Vector3Int> curList = new List<Vector3Int>() { pos, };
                    Vector3Int nextPos = pos - dir;
                    while (sameList.Contains(nextPos)) 
                    {
                        curList.Add(nextPos);
                        nextPos -= dir;
                    }

                    if (curList.Count >= 3)
                    {
                        lineMatch = curList;
                        if (curList.Count == 5)
                            specialMatchs[0] = true;
                        else if (curList.Count == 4)
                            specialMatchs[2] = true;
                    }
                }
            }
        }




        if (lineMatch.Count == 0 && shapeMatch.Count == 0)
            return false;

        Match match = CreateNewMatch(_mainPos);

        for(int i = 0; i < specialMatchs.Length; ++i)
        {
            if (specialMatchs[i])
            {
                if(i == (int)DefineInfo.SpecialMatchType.five)
                {

                }
                else if(i == (int)DefineInfo.SpecialMatchType.cross)
                {
                    match.specialBlock = factory.GetNewArrowBlock(Block_Special.SpecialBlockType.bomb, dicComponents[_mainPos].curBlock.typeNumber);
                    break;
                }
                else if(i == (int)DefineInfo.SpecialMatchType.four)
                {
                    if(lineMatch[0].x != lineMatch[1].x)
                        match.specialBlock = factory.GetNewArrowBlock(Block_Special.SpecialBlockType.col, dicComponents[_mainPos].curBlock.typeNumber);
                    else
                        match.specialBlock = factory.GetNewArrowBlock(Block_Special.SpecialBlockType.row, dicComponents[_mainPos].curBlock.typeNumber);
                    break;
                }
                else
                {
                    match.specialBlock = factory.GetNewArrowBlock(Block_Special.SpecialBlockType.finder, dicComponents[_mainPos].curBlock.typeNumber);
                    break;
                }
            }
        }

        

        foreach(Vector3Int pos in lineMatch)
        {
            match.AddMatchingBlock(dicComponents[pos].curBlock);
            popSound.Play();
            GameManager.Instance.StratExplosionAnim(pos);
        }

        foreach(Vector3Int pos in shapeMatch)
        {
            match.AddMatchingBlock(dicComponents[pos].curBlock);
            popSound.Play();
            GameManager.Instance.StratExplosionAnim(pos);
        }
       

        return true;
    }

    
    

}
