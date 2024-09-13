using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Minion", menuName = "ScriptableObjects/Minions", order = 1)]
public class Minion : ScriptableObject
{
    public RuntimeAnimatorController  bodyBlueSide, bodyRedSide;
    public int maxLife;
    public int attack;
    public float range;
    public float speed;
    public int goldCost;
    public Attack attackType;
    public float codification;
}

public enum Attack
{
    Melee,
    Arrow,
    TNT
}
