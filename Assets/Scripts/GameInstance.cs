using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameInstance : MonoBehaviour
{
    public bool visualsActive = false;
    public List<GameObject> visuals;
    public SideManager blueSide, redSide;
    public InputDriver blueInput, redInput;
    public Transform blueSideStart, redSideStart;
    public float AIUpdateTime = 0.5f;
    public int AI_Type = -1;
    public bool playerInput = false;
    private float _nextUpdate = 0f;
    [NonSerialized] public List<MinionController> blueSideMinions, redSideMinions;
    

    private void Awake()
    {
        UpdateVisuals();
        blueSideMinions = new List<MinionController>();
        redSideMinions = new List<MinionController>();
        if(blueInput==null)
            blueInput = new BaseAI();
        if(redInput == null)
            redInput = new BaseAI();
        blueSide.OnMinionSpawn += BlueSideSpawn;
        redSide.OnMinionSpawn += RedSideSpawn;
        //INitial offset
        _nextUpdate = Time.time + AIUpdateTime + Random.Range(0f, AIUpdateTime);
        //Time.timeScale = 3f;
    }

    void Start()
    {
        
    }

    public void Reset()
    {
        DeleteUnits();
        blueSideMinions.Clear();
        redSideMinions.Clear();
        _nextUpdate = Time.time + AIUpdateTime + Random.Range(0f, AIUpdateTime);
        blueSide.Reset();
        redSide.Reset();
    }

    private float Normalize(float value, float min, float max, bool clamp = false)
    {
        float normValue = (value - min) / (max - min);
        if (clamp)
            normValue = Mathf.Clamp01(normValue);
        return normValue;
    }
    public void BlueSideSpawn(MinionController minion)
    {
        blueSideMinions.Add(minion);
    }
    
    public void RedSideSpawn(MinionController minion)
    {
        redSideMinions.Add(minion);
    }

    private void UpdateLists()
    {
        blueSideMinions.RemoveAll(minion => minion == null || minion.IsDead());
        redSideMinions.RemoveAll(minion => minion == null || minion.IsDead());
        blueSideMinions.Sort((a, b) => b.transform.position.x.CompareTo(a.transform.position.x));
        redSideMinions.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        /*
        if(blueSideMinions.Count > 0)
            Debug.Log(($"First blue {blueSideMinions[0].name}"));
        if(redSideMinions.Count > 0)
            Debug.Log(($"First red {redSideMinions[0].name}"));
        */
    }
    // Update is called once per frame

    public void UpdateFitness()
    {
        
        blueInput.UpdateFitness(new NetworkStats(1000-redSide.state.towerLife, 1000-blueSide.state.towerLife, redSide.state.towerLife <= 0));
        Debug.Log($"{gameObject.name} AI{AI_Type}: Inflicted {(1000- redSide.state.towerLife).ToString()} Received {(1000- blueSide.state.towerLife).ToString()} Victory {(redSide.state.towerLife <= 0).ToString()}  Fitness: {blueInput.GetFitness().ToString()}");
        //Solo se testea el blue
        //redInput.UpdateFitness(new NetworkStats(1000-blueSide.state.towerLife, 1000-redSide.state.towerLife, blueSide.finished));
    }

    public void ResetFitness()
    {
        blueInput.Reset();
        redInput.Reset();
    }
    void Update()
    {
        if (Time.time >= _nextUpdate)
        {
            if (CheckEnd())
            {
                //Debug.Log("Stop updating");
                blueSide.Finish();
                redSide.Finish();
               
                _nextUpdate = Single.MaxValue; 
                DeleteUnits();
                //Invoke(nameof(Reset), 1f);
                return;
            }
                
            UpdateLists();
            NetworkInput blue = new NetworkInput();
            NetworkInput red = new NetworkInput();
            CalculateInputs(blue, red);
            if(!playerInput)
                ProccesAction(blueInput.ProcessInput(blue), true);
            ProccesAction(redInput.ProcessInput(red), false);
            //Debug.Log(blueFirstDistance + " , " + redFirstDistance);
           
            //Si el juego esta acelerado la decision se tomara antes.
            _nextUpdate = Time.time + (AIUpdateTime / Time.timeScale);
        }
    }

    private void DeleteUnits()
    {
        for (int i = blueSideMinions.Count -1 ; i  >= 0; i--)
        {
            //Destroy(blueSideMinions[i].gameObject);
            blueSideMinions[i].Remove();
            blueSideMinions.RemoveAt(i);
        }
        
        for (int i = redSideMinions.Count -1 ; i  >= 0; i--)
        {
            //Destroy(redSideMinions[i].gameObject);
            redSideMinions[i].Remove();
            redSideMinions.RemoveAt(i);
        }
    }
    private bool CheckEnd()
    {
        return (blueSide.finished || redSide.finished);

    }

    private void ProccesAction(AiAction action, bool blue)
    {
        SideManager side = blue ? blueSide : redSide;
        switch (action)
        {
            case AiAction.None:
                break;
            case AiAction.Mine:
                side.LevelUpMine();
                break;
            case AiAction.Pawn:
                side.SpawnMinion(2);
                break;
            case AiAction.Torch:
                side.SpawnMinion(3);
                break;
            case AiAction.Archer:
                side.SpawnMinion(1);
                break;
            case AiAction.TNT:
                side.SpawnMinion(4);
                break;
            case AiAction.Knight:
                side.SpawnMinion(0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
    public void CalculateInputs(  NetworkInput blue,   NetworkInput red)
    {
        float blueFirstDistance = blueSideMinions.Count > 0
            ? Normalize(blueSideMinions[0].transform.position.x, blueSideStart.position.x, redSideStart.position.x)
            : 0;
        float redFirstDistance = redSideMinions.Count > 0
            ? Normalize(redSideMinions[0].transform.position.x, redSideStart.position.x, blueSideStart.position.x)
            : 0;
        float[] allys = new float[5];
        float[] enemys = new float[5];
        for (int i = 0; i < 5; i++)
        {
            if (i < blueSideMinions.Count)
                allys[i] = blueSideMinions[i].stats.codification;
            else
                allys[i] = 0;
            
            if (i < redSideMinions.Count)
                enemys[i] = redSideMinions[i].stats.codification;
            else
                enemys[i] = 0;
        }
        
        //blue
        blue.gold = Normalize(blueSide.state.gold, 0f, 1000f, true);
        blue.enemyGold = Normalize(redSide.state.gold, 0f, 1000f, true);
        blue.mineLv = Normalize(blueSide.state.goldMineLV, 0f, 2f);
        blue.allyDistance = blueFirstDistance;
        blue.enemyDistance = redFirstDistance;
        blue.UnitsFromArray(allys, enemys);
        
        //red
        red.gold = Normalize(redSide.state.gold, 0f, 1000f, true);
        red.enemyGold = Normalize(blueSide.state.gold, 0f, 1000f, true);
        red.mineLv = Normalize(redSide.state.goldMineLV, 0f, 2f);
        red.allyDistance = redFirstDistance;
        red.enemyDistance = blueFirstDistance;
        red.UnitsFromArray(enemys, allys);
       
    }
    [ContextMenu("Update visuals")]
    public void UpdateVisuals()
    {
        foreach(var item in visuals)
        {
            item.SetActive(visualsActive);
        }
    }
}
