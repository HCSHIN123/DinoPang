using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_Special : Block
{
    public enum SpecialBlockType
    {
        col, row, bomb, finder, rainbow, end,
    }

    public SpecialBlockType specialType;
    protected SpriteRenderer sr;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    virtual public void Init(SpecialBlockType _specialType, int _type, Sprite _sprite)
    {
        base.Reset();
        specialType = _specialType;
        typeNumber = _type;
        sr.sprite = _sprite;
    }
}
