using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using AI.Network;
using UnityEngine;
using Random = UnityEngine.Random;

public class GeneticManager : MonoBehaviour
{
    public bool learn = true;
    public GameInstance gameInstancePrefab;
    private const int InstanceCount = 510, SizeX = 25, SizeY = 15, RowSize = 10, PoblationSize = 170;
    public static readonly int[] Layers = new int[] { 15, 40, 30, 7 };
    public List<GameInstance> instances;
    public List<GeneticNetwork> enemyAI1, enemyAI2, enemyAI3;
    private GeneticNetwork bestA1, bestA2, bestA3;
    private int lastEnemySet = -1, generation = 0;
    public bool stop = false;

    [Header("Breeding Variables")] [SerializeField]
    private int iterationTime = 90;
    [Range(0f,1f)]
    [SerializeField] private float parentScale = 0.92f;
    [Range(0f, 1f)]
    [SerializeField] private float unbiasedMutationProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float biasedMutationProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float nodeMutationProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float crossoverWeightsProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float crossoverNodesProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float mutationProb = 0.2f;
    [SerializeField] private int nodesMutationAmount = 2;
    private IEnumerator Start()
    {
        Time.timeScale = 4f;
        enemyAI1 = new List<GeneticNetwork>(PoblationSize);
        enemyAI2 = new List<GeneticNetwork>(PoblationSize);
        enemyAI3 = new List<GeneticNetwork>(PoblationSize);
        instances = new List<GameInstance>(InstanceCount);
        if (learn)
        {
            int row = 0;
            int column = 0;
            float offsetX = -RowSize * SizeX / 2f;
            float offsetY = 0;
            for (int i = 0; i < InstanceCount; i++)
            {
                instances.Add(Instantiate(gameInstancePrefab, new Vector3(row * SizeX + offsetX, column * SizeY + offsetY, 0), Quaternion.identity, transform));
                instances[i].gameObject.name = "Instance_" + i;
                column++;
                if (column >= RowSize)
                {
                    column = 0;
                    row++;
                }
                yield return null;
            }
            FirstGeneration();
            
            StartCoroutine(Training());
        }
       
    }

    [ContextMenu("Force Stop")]
    public void ForceStop()
    {
        stop = true;
        StopAllCoroutines();
        WriteNetworks(5, 0);
        WriteNetworks(5, 1);
        WriteNetworks(5, 2);
    }

    public IEnumerator Training()
    {
        while (!stop)
        {
            generation++;
            ResetFitnesses();
            for (int i = 0; i < 4; i++)
            {
                SetTrainingEnemy(i, true);
                yield return new WaitForSeconds(iterationTime);
                UpdateFitnesses();
                Debug.Log("New training with AI " + generation + "_" + i);
            }
            Sort();
            Debug.Log("Completed generation " + generation);
            Debug.Log("Best A1: " + enemyAI1[0].fitness);
            foreach (var instance in instances)
            {
                if(((EnemyNeuralNetwork)instance.blueInput).network.Equals(enemyAI1[0]))
                    Debug.Log(instance.gameObject.name);
            }
            Debug.Log("Best A2: " + enemyAI2[0].fitness);
            foreach (var instance in instances)
            {
                if(((EnemyNeuralNetwork)instance.blueInput).network.Equals(enemyAI2[0]))
                    Debug.Log(instance.gameObject.name);
            }
            Debug.Log("Best A3: " + enemyAI3[0].fitness);
            foreach (var instance in instances)
            {
                if(((EnemyNeuralNetwork)instance.blueInput).network.Equals(enemyAI3[0]))
                    Debug.Log(instance.gameObject.name);
            }
            WriteNetworks(5, 0);
            WriteNetworks(5, 1);
            WriteNetworks(5, 2);
            GetBestNetworks();
            NextGeneration(0);
            NextGeneration(1);
            NextGeneration(2);
            UpdateAIs();
        }
        
    }
    
    private void Sort()
    {
        Debug.Log("Sorting");
        enemyAI1.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        enemyAI2.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        enemyAI3.Sort((a, b) => b.fitness.CompareTo(a.fitness));
    }
    private void UpdateFitnesses()
    {
        foreach (var instance in instances)
        {
            instance.UpdateFitness();
        }
    }
    
    private void ResetFitnesses()
    {
        foreach (var instance in instances)
        {
            instance.ResetFitness();
        }
    }
    public void SetTrainingEnemy(int learningIteration, bool reset)
    {
      
        foreach (var instance in instances)
        {
            switch (learningIteration)
            {
                case 1:
                    instance.redInput = new EnemyNeuralNetwork(bestA1);
                    break;
                case 2:
                    instance.redInput = new EnemyNeuralNetwork(bestA2);
                    break;
                case 3:
                    instance.redInput = new EnemyNeuralNetwork(bestA3);
                    break;
                default:
                    instance.redInput = new BaseAI();
                    break;
            }

            if (reset)
            {
                instance.Reset();
            }
            
        }
    }
    
    public void FirstGeneration()
    {
        int instance = 0;
        for (int i = 0; i < PoblationSize; i++)
        {
            
            var net = new GeneticNetwork(Layers, Activation.Sigmoid, Activation.Sigmoid);
            foreach (var layer in net.layers)
            {
                layer.InitRandomWeights(false);
            }
            enemyAI1.Add(net);
            instances[instance].blueInput = new EnemyNeuralNetwork(net);
            instances[instance].AI_Type = 1;
            instance++;
            
            var net2 = new GeneticNetwork(Layers, Activation.Sigmoid, Activation.Sigmoid);
            foreach (var layer in net2.layers)
            {
                layer.InitRandomWeights(false);
            }
            enemyAI2.Add(net2);
            instances[instance].blueInput = new EnemyNeuralNetwork(net2);
            instances[instance].AI_Type = 2;
            instance++;
            
            var net3 = new GeneticNetwork(Layers, Activation.Sigmoid, Activation.Sigmoid);
            foreach (var layer in net3.layers)
            {
                layer.InitRandomWeights(false);
            }
            enemyAI3.Add(net3);
            instances[instance].blueInput = new EnemyNeuralNetwork(net3);
            instances[instance].AI_Type = 3;
            instance++;
        }
        //Load prev data if existent
        ReadNetworks(0);
        ReadNetworks(1);
        ReadNetworks(2);
        GetBestNetworks();
    }

    public void UpdateAIs()
    {
        int instance = 0;
        for (int i = 0; i < PoblationSize; i++)
        {
            instances[instance].blueInput = new EnemyNeuralNetwork(enemyAI1[i]);
            instances[instance].AI_Type = 1;
            instance++;
            
            instances[instance].blueInput = new EnemyNeuralNetwork(enemyAI2[i]);
            instances[instance].AI_Type = 2;
            instance++;
            
            instances[instance].blueInput = new EnemyNeuralNetwork(enemyAI3[i]);
            instances[instance].AI_Type = 3;
            instance++;

        }
    }

    private void GetBestNetworks()
    {
        bestA1 = new GeneticNetwork(Layers, Activation.Sigmoid, Activation.Sigmoid);
        bestA1.Decode(enemyAI1[0].Encode());
        bestA2 = new GeneticNetwork(Layers, Activation.Sigmoid, Activation.Sigmoid);
        bestA2.Decode(enemyAI2[0].Encode());
        bestA3 = new GeneticNetwork(Layers, Activation.Sigmoid, Activation.Sigmoid);
        bestA3.Decode(enemyAI3[0].Encode());
    }
    #region Breeding

    private GeneticNetwork[] GetTwoParent(List<GeneticNetwork> parentNetwork)
    {
        GeneticNetwork[] parents = new GeneticNetwork[2];
        for (int i = 0; i < parentNetwork.Count; i++)
        {
            float p = Random.Range(0f, 1f);
            if (p < parentScale)
            {
                parents[0] = parentNetwork[i];
                break;
            }
             
        }
        if (parents[0] == null)
            parents[0] = parentNetwork[0];
        for (int i = 0; i < parentNetwork.Count; i++)
        {
            float p = Random.Range(0f, 1f);
            if (p < parentScale)
            {
                if(parents[0] != parentNetwork[i])
                {
                    parents[1] = parentNetwork[i];
                    break;
                }
            }    
        }
        if (parents[1] == null)
            parents[1] = parents[0] == parentNetwork[0] ? parentNetwork[1] : parentNetwork[0];
        //Debug.Log("Parent 1: "+ parentNetworks.IndexOf(parents[0]) + ", " + "Parent 2: " + parentNetworks.IndexOf(parents[1]));
        return parents;
    }

    private void NextGeneration(int aiIndex)
    {
        List<GeneticNetwork> parentNetwork;
        switch (aiIndex)
        {
            case 0:
                parentNetwork = enemyAI1;
                break;
            case 1:
                parentNetwork = enemyAI2;
                break;
            case 2:
                parentNetwork = enemyAI3;
                break;
            default:
                Debug.LogError("Inexpected AI index " + aiIndex);
                return;
        }
        var nextGeneration = new List<GeneticNetwork>();
        nextGeneration.Add(parentNetwork[0]);
        nextGeneration.Add(parentNetwork[1]);
        
        // Create 2 children for every breeding of NN
        for (int i = 0; i < parentNetwork.Count / 2; i++)
        {
            // Select parents to breed
            var parents = GetTwoParent(parentNetwork);


            //Breed the two selected parents and add them to the next generation
            //Debug.Log("Breeding: " + parent1Index + " with fitness " + population[parent1Index].fitness);
            //Debug.Log("and " + parent2Index + " with fitness " + population[parent2Index].fitness);

            GeneticNetwork[] children = Breed(parents[0], parents[1]);

            // Mutate children
            Mutate(children[0], GetRandomMutateOperation(), parentNetwork);
            Mutate(children[1], GetRandomMutateOperation(), parentNetwork);

            // Add the children to the next generation
            nextGeneration.Add(children[0]);
            nextGeneration.Add(children[1]);
        } //  End foor loop -- Breeding

        // Make the children adults
        for (int i = 1; i < parentNetwork.Count; i++)
        {
            parentNetwork[i] = nextGeneration[i];
        }
    } 
    public GeneticNetwork.GeneticOperation GetRandomMutateOperation()
    {
        float operationAdd = unbiasedMutationProb + biasedMutationProb + nodeMutationProb;
        float selection = Random.Range(0, operationAdd);
        if (selection < unbiasedMutationProb)
            return GeneticNetwork.GeneticOperation.ImpartialMutation;
        if (selection < unbiasedMutationProb + biasedMutationProb)
            return GeneticNetwork.GeneticOperation.PartialMutation;
        return GeneticNetwork.GeneticOperation.NodeMutation;
      
    }
    
    private GeneticNetwork[] Breed(GeneticNetwork mother, GeneticNetwork father)
    {
        GeneticNetwork child1 = new GeneticNetwork(Layers, Activation.Sigmoid, Activation.Sigmoid);
        GeneticNetwork child2 = new GeneticNetwork(Layers, Activation.Sigmoid, Activation.Sigmoid);

        double[] motherChromosome = mother.Encode();
        double[] fatherChromosome = father.Encode();

        Crossover(ref motherChromosome, ref fatherChromosome);

        child1.Decode(motherChromosome);
        child2.Decode(fatherChromosome);

        return new GeneticNetwork[] { child1, child2 };
    }
    
    private void Crossover(ref double[] p1,ref double[] p2)
    {
        double[] ch1 = new  double[p1.Length];
        double[] ch2 = new double[p1.Length];

        for(int i = 0; i < p1.Length; i++)
        {
            bool swap = Random.Range(0f, 1f) < 0.5f;
            ch1[i] = swap ? p2[i] : p1[i];
            ch2[i] = swap ? p1[i] : p2[i];
        }
        p1 = ch1;
        p2 = ch2;
        
    }
    private void Mutate(GeneticNetwork creature, GeneticNetwork.GeneticOperation op, List<GeneticNetwork> parentNetworks)
    {
        switch (op)
        {
            case GeneticNetwork.GeneticOperation.None:
                creature.NoMutation(parentNetworks[0]);
                break;
            case GeneticNetwork.GeneticOperation.PartialMutation:
                creature.PartialMutation(creature, mutationProb);
                break;
            case GeneticNetwork.GeneticOperation.ImpartialMutation:
                creature.ImpartialMutation(creature, mutationProb);
                break;
            case GeneticNetwork.GeneticOperation.WeightCrossover:
                break;
            case GeneticNetwork.GeneticOperation.NodeCrossover:
                break;
            case GeneticNetwork.GeneticOperation.NodeMutation:
                creature.MutateNodes(creature, nodesMutationAmount);
                break;
        }

    }
    #endregion


    #region Files

    public void WriteNetworks(int amount, int ai)
    {
        List<GeneticNetwork> network;
       
        var file = "error.txt";
        switch (ai)
        {
            case 0:
                network = enemyAI1;
                file = "AI1.txt";
                break;
            case 1:
                network = enemyAI2;
                file = "AI2.txt";
                break;
            case 2:
                network = enemyAI3;
                file = "AI3.txt";
                break;
            default:
                Debug.LogError("Inexpected AI index " + ai);
                return;
        }
        Debug.Log("Writing " + file);
        //File.Create(file);
        StreamWriter sw = new StreamWriter(file);
        for (int i = 0; i < amount; i++)
        {
            var code = network[i].Encode();
            for (int j = 0; j < code.Length; j++)
            {
                sw.Write(code[j].ToString(CultureInfo.InvariantCulture));
                if(j< code.Length-1)
                    sw.Write(",");
            }
            sw.Write("\n");
         
           
        }
        sw.Flush();
        sw.Close();
    }
    
    public bool ReadNetworks(int ai)
    {
        List<GeneticNetwork> network;
       
        var file = "error.txt";
        switch (ai)
        {
            case 0:
                network = enemyAI1;
                file = "Assets/Presets/AI1.txt";
                break;
            case 1:
                network = enemyAI2;
                file = "Assets/Presets/AI2.txt";
                break;
            case 2:
                network = enemyAI3;
                file = "Assets/Presets/AI3.txt";
                break;
            default:
                Debug.LogError("Inexpected AI index " + ai);
                return false;
        }
        Debug.Log("Reading " + file);
        if (!File.Exists(file))
            return false;
        //File.Create(file);
        StreamReader sr = new StreamReader(file);
        int index = 0;
        while (sr.Peek() >= 0)
        {
    
            var line = sr.ReadLine().Split(',');
            double[] coded = new double[line.Length];
            for (int i = 0; i < line.Length; i++)
            {
                coded[i] = Double.Parse(line[i], CultureInfo.InvariantCulture);
            }
            network[index].Decode(coded);
            index++;
        }
       
        sr.Close();
        return true;
    }

    

    #endregion
}
