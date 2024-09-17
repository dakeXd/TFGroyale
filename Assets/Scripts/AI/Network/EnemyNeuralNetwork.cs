using System.Collections;
using System.Collections.Generic;
using AI.Network;
using UnityEngine;

public class EnemyNeuralNetwork :  InputDriver
{
   
    public GeneticNetwork network;

    public EnemyNeuralNetwork(GeneticNetwork network)
    {
        this.network = network;
    }
    
    public AiAction ProcessInput(NetworkInput input)
    {
        int output = network.GetMaxOutput(input.AsDoubleArray());
        return (AiAction) output;
    }

    public void UpdateFitness(NetworkStats stats)
    {
        //[0 - 100]
        network.fitness += stats.victory ? 100 : 0;
        //[-25. 25]
        network.fitness += stats.towerDamageInflicted / 40f;
        network.fitness -= stats.towerDamageReceived / 40f;
    }

    public void Reset()
    {
        network.fitness = 0;
    }

    public float GetFitness()
    {
        return network.fitness;
    }
}
