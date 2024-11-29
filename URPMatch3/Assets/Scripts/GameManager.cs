using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class GameManager : MonoBehaviour
{
    public Board gameBoard;
    public GameData gameData;
    public MapTool maptool;
    private static GameManager s_Instance;
    private static bool s_IsShuttingDown = false;
    public static GameManager Instance
    {
        get
        {
#if UNITY_EDITOR
            if (s_Instance == null && !s_IsShuttingDown)
            {
                var newInstance = Instantiate(Resources.Load<GameManager>("GameManager"));
                newInstance.Awake();
            }
#endif
            return s_Instance;
        }

        private set => s_Instance = value;
    }

    public GameObject[] effectPrefabs;
    public Queue<EffectPlayer> effectQueue = new Queue<EffectPlayer>();


    private void Awake()
    {
        if(s_Instance == this)
        {
            return;
        }
        if(s_Instance == null) 
        {
            s_Instance = this;

            for (int i = 0; i < effectPrefabs.Length; ++i)
            {
                for (int j = 0; j < 50; ++j)
                {
                    EffectPlayer ep = Instantiate(effectPrefabs[i]).GetComponent<EffectPlayer>();
                    effectQueue.Enqueue(ep);
                    ep.endCallback = () =>
                    {
                        effectQueue.Enqueue(ep);
                        ep.gameObject.SetActive(false);
                    };


                    ep.gameObject.SetActive(false);
                }
            }

        }

        GameInit();


        
    }

    public void GameInit()
    {
        gameData.InitData();
        maptool.InitMapTool(gameData.boardData);
        
    }

    public void StartGame()
    {
        gameBoard.InitBoard(gameData.boardData);
        maptool.gameObject.SetActive(false);
    }

    public void StratExplosionAnim(Vector3Int _pos)
    {
        EffectPlayer go = effectQueue.Dequeue();
        go.gameObject.SetActive(true);
        go.transform.position = _pos;
        go.PlayExplosionEffect();
    }
    
}
