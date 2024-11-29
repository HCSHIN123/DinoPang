using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFactory : MonoBehaviour
{
    [SerializeField]
    private int blockCount = 162;
    public Block[] normalBlockPrefabs;
    public Block_Arrow arrowPrefab;
    public Block_Bomb bombPrefab;
    public Block_Find findPrefab;

    private Queue<Block>[] normalBlockPool;
    private Queue<Block_Arrow> arrowBlockPool;
    private Queue<Block_Bomb> bombBlockPool;
    private Queue<Block_Find> findBlockPool;

    public Sprite[] GreenSprites;
    public Sprite[] OrangeSprites;
    public Sprite[] YellowSprites;
    public Sprite[] SkySprites;
    public Sprite[] PinkSprites;


   
    public void InitFactory()
    {
        normalBlockPool = new Queue<Block>[normalBlockPrefabs.Length];
        arrowBlockPool = new Queue<Block_Arrow>();
        bombBlockPool = new Queue<Block_Bomb>();
        findBlockPool = new Queue<Block_Find>();
        //arrowSprites = new Sprite[normalBlockPrefabs.Length,2];
        //specialBlockPool = new Queue<Block_Special>[normalBlockPrefabs.Length];
        for (int i = 0; i < normalBlockPool.Length; ++i)
        {
            normalBlockPool[i] = new Queue<Block>();
            for(int j =0; j < blockCount; ++j)
            {
                Block block = Instantiate(normalBlockPrefabs[i]);
                normalBlockPool[i].Enqueue(block);
                block.gameObject.SetActive(false);


                if (j % 4 == 0)
                {
                    Block_Arrow arrow = Instantiate(arrowPrefab);
                    arrowBlockPool.Enqueue(arrow);
                    arrow.gameObject.SetActive(false);

                    Block_Bomb bomb = Instantiate(bombPrefab);
                    bombBlockPool.Enqueue(bomb);
                    bomb.gameObject.SetActive(false);

                    Block_Find find = Instantiate(findPrefab);
                    findBlockPool.Enqueue(find);
                    find.gameObject.SetActive(false);

                }


            }
                
        }
    }

    public Block GetNewBlock(int _typeNum)
    {
        Block block = normalBlockPool[_typeNum].Dequeue();
        block.gameObject.SetActive(true);
        return block;
    }

    public void RemoveBlock(Block _block)
    {
        if(_block is Block_Arrow)
        {
            _block.Reset();
            arrowBlockPool.Enqueue(_block as Block_Arrow);
            _block.gameObject.SetActive(false);
            return;
        }
        else if (_block is Block_Bomb)
        {
            _block.Reset();
            bombBlockPool.Enqueue(_block as Block_Bomb);
            _block.gameObject.SetActive(false);
            return;
        }
        else if (_block is Block_Find)
        {
            _block.Reset();
            findBlockPool.Enqueue(_block as Block_Find);
            _block.gameObject.SetActive(false);
            return;
        }



        if (_block.typeNumber >= normalBlockPrefabs.Length)
            return;
        _block.Reset();
        normalBlockPool[_block.typeNumber].Enqueue(_block);
        _block.gameObject.SetActive(false);
    }
    

    public Block_Arrow GetNewArrowBlock(Block_Special.SpecialBlockType _type, int _typeNum)
    {
        Block_Arrow block = arrowBlockPool.Dequeue();
        Sprite sprite = null;
        
        switch(_typeNum)
        {
            case 0:
                {
                    if (_type <= Block_Special.SpecialBlockType.row)
                        sprite = _type == Block_Special.SpecialBlockType.col ? GreenSprites[0] : GreenSprites[1];
                    else if (_type == Block_Special.SpecialBlockType.bomb)
                        sprite = GreenSprites[2];
                    else if(_type == Block_Special.SpecialBlockType.finder)
                        sprite = GreenSprites[3];
                    break;
                }
            case 1:
                {
                    if (_type <= Block_Special.SpecialBlockType.row)
                        sprite = _type == Block_Special.SpecialBlockType.col ? OrangeSprites[0] : OrangeSprites[1];
                    else if (_type == Block_Special.SpecialBlockType.bomb)
                        sprite = OrangeSprites[2];
                    else if (_type == Block_Special.SpecialBlockType.finder)
                        sprite = OrangeSprites[3];
                    break;
                }
            case 2:
                {
                    if (_type <= Block_Special.SpecialBlockType.row)
                        sprite = _type == Block_Special.SpecialBlockType.col ? SkySprites[0] : SkySprites[1];
                    else if (_type == Block_Special.SpecialBlockType.bomb)
                        sprite = SkySprites[2];
                    else if (_type == Block_Special.SpecialBlockType.finder)
                        sprite = SkySprites[3];
                    break;
                }
            case 3:
                {
                    if (_type <= Block_Special.SpecialBlockType.row)
                        sprite = _type == Block_Special.SpecialBlockType.col ? YellowSprites[0] : YellowSprites[1];
                    else if (_type == Block_Special.SpecialBlockType.bomb)
                        sprite = YellowSprites[2];
                    else if (_type == Block_Special.SpecialBlockType.finder)
                        sprite = YellowSprites[3];
                    break;
                }
            case 4:
                {
                    if (_type <= Block_Special.SpecialBlockType.row)
                        sprite = _type == Block_Special.SpecialBlockType.col ? PinkSprites[0] : PinkSprites[1];
                    else if (_type == Block_Special.SpecialBlockType.bomb)
                        sprite = PinkSprites[2];
                    else if (_type == Block_Special.SpecialBlockType.finder)
                        sprite = PinkSprites[3];
                    break;
                }
        }

        block.Init(_type, _typeNum, sprite);

        block.gameObject.SetActive(true);
        return block;
    }
}
