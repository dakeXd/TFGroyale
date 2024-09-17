using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InputDriver {
    public abstract AiAction ProcessInput(NetworkInput input);
    public abstract void UpdateFitness(NetworkStats stats);
    public abstract void Reset();

    public abstract float GetFitness();
}

public class NetworkStats
{
    public float towerDamageInflicted;
    public float towerDamageReceived;
    public bool victory;

    public NetworkStats(float towerDamageInflicted, float towerDamageReceived, bool victory)
    {
        this.towerDamageInflicted = towerDamageInflicted;
        this.towerDamageReceived = towerDamageReceived;
        this.victory = victory;
    }
}