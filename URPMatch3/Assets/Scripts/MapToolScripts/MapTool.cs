using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class MapTool : MonoBehaviour
{
    private Dictionary<Vector3Int, MapToolRect> map = new Dictionary<Vector3Int, MapToolRect>();
    private int[,] mapData = new int[9, 9];
    private int curSelectedNumber = 0;


    public InputAction InputAction;
    public InputAction ClickPosition;
    public MapToolRect compPrefabs;
    public Sprite[] rectSprites;
    public Button buttonPref;

    public Button saveButton;

    private void Update()
    {
        var pressedThisFrame = InputAction.IsPressed();
        var releasedThisFrame = InputAction.WasReleasedThisFrame();
        InputAction.IsPressed();
        Vector2 clickPos = ClickPosition.ReadValue<Vector2>();
        if (pressedThisFrame)
        {
            foreach (Vector3Int pos in map.Keys)
            {
                if (map[pos].IsClicked(clickPos))
                {
                    map[pos].ChangeType(curSelectedNumber, rectSprites[curSelectedNumber]);
                }
            }
        }
    }

    public void InitMapTool(int[,] _mapData)
    {

        for (int y = 0; y < 9; ++y)
        {
            for (int x = 0; x < 9; ++x)
            {
                Vector3Int key = new Vector3Int(x, y, 0);
                map.Add(key, Instantiate(compPrefabs, transform));
                map[key].transform.position = key;
                mapData[y, x] = _mapData[y,x];
                map[key].ChangeType(_mapData[y, x], rectSprites[_mapData[y,x]]);
            }
        }

        Canvas canvas = GetComponentInChildren<Canvas>();
        Vector3 btnStartPos = buttonPref.transform.position;
        RectTransform btnRt = buttonPref.GetComponentInChildren<RectTransform>();
        float offset = btnRt.sizeDelta.x;

        for (int i = 0; i < rectSprites.Length; ++i)
        {
            Button extrBtn = Instantiate(buttonPref, canvas.transform);
            extrBtn.transform.position = btnStartPos + new Vector3(offset * (i + 1), 0, 0);
            extrBtn.GetComponent<Image>().sprite = rectSprites[i];
            int idx = i;
            extrBtn.onClick.AddListener(() => { SelectCurType(idx); });
        }
        buttonPref.gameObject.SetActive(false);
        InputAction.Enable();
        ClickPosition.Enable();
       
    }

    private void UpdateMapdate()
    {
        foreach (Vector3Int idx in map.Keys)
        {
            mapData[idx.y, idx.x] = map[idx].Type;
        }
    }

    //////////////BUTTON EVENT
    public void SelectCurType(int _type)
    {
        Debug.Log("SETL : " + _type);
        curSelectedNumber = _type;
    }

    public void SaveMapDate()
    {
        UpdateMapdate();
        GameManager.Instance.gameData.SaveToJson(mapData);
    }

}
