using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MapToolRect : MonoBehaviour
{
    private int typeNumber = 1;
    private SpriteRenderer sr;
    private RectTransform rt;


    public int Type => typeNumber;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rt = GetComponent<RectTransform>();
    }

    public bool IsClicked(Vector2 _clickPos)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, _clickPos, Camera.main))
        {
            return true;
        }
        return false;
    }

    public void ChangeType(int _type, Sprite _sprite)
    {
        if (typeNumber == _type)
            return;

        typeNumber = _type;
        sr.sprite = _sprite;
    }
}
