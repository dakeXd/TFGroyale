using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace AI.Network
{
    public enum Activation
    {
        Step,
        Sigmoid,
        ReLU,
        Softmax,
        Tanh
    }
    public class NeuralNetwork
    {
   
        public Layer[] layers;
        public bool backpropagation = false;
        public const float H = 0.0001f;
        public float inertia = 0;

    
        //public Activation activation = Activation.Step;
        public NeuralNetwork(int[] layerSizes, Activation activation, Activation outputActivation, bool backpropagation, float inertia = 0)
        {
            this.inertia = inertia;
            layers = new Layer[layerSizes.Length - 1];
            for (int i = 0; i < layerSizes.Length -1; i++)
            {
                layers[i] = new Layer(layerSizes[i], layerSizes[i  + 1], i == (layerSizes.Length - 2) ? outputActivation : activation, backpropagation, inertia);
            }
            //this.activation = activation;
            this.backpropagation = backpropagation;
        }

        public double[] Encode()
        {
            int size = 0;
            foreach (var layer in layers)
            {
                size += (layer.lengthIn * layer.lengthOut + layer.lengthOut);
            }
            double[] encodedNetwork = new double[size];
            int index = 0;
            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = layers[l];
                for (int i = 0; i < layer.weights.GetLength(1); i++)
                {
                    for (int j = 0; j < layer.weights.GetLength(0); j++)
                    {
                        encodedNetwork[index] = layer.weights[j, i];
                        index++;
                    }
                    encodedNetwork[index] = layer.biases[i];
                    index++;
                }
            }
            return encodedNetwork;
        }

        public void Decode(double[] coded)
        {
     
            int index = 0;
            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = layers[l];
                for (int i = 0; i < layer.weights.GetLength(1); i++)
                {
                    for (int j = 0; j < layer.weights.GetLength(0); j++)
                    {
                        layer.weights[j, i] = coded[index];
                        index++;
                    }
                    layer.biases[i] = coded[index];
                    index++;
                }
            }

        }



        public double Cost(double[] inputs, double[] expected)
        {
            double[] outputs = CalculateOutputs(inputs);
  
            double cost = 0;
            for (int i = 0; i < outputs.Length; i++)
            {
                var semicost = DataPointCost(outputs[i], expected[i]);
                cost += semicost;
                //Debug.Log($"Output: {outputs[i]}, Expected: {expected[i]}, Cost: " + semicost.ToString("N3"));
            }
            //Debug.Log("Data cost: " + cost.ToString("N3"));
            return cost * 0.5;
        }
    

        public double DataPointCost(double guess, double expected)
        {
       
            var semival =  (expected - guess);
            //Debug.Log("Guess: " + guess.ToString("F6") + " / " + expected + " cost: " + (semival * semival));
            return (semival * semival);
        }
    
        public  double DataPointCostDerivative(double guess, double expected)
        {
            return (guess - expected);
        }

    
    
        public double[] CalculateOutputs(double[] inputs)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                inputs = layers[i].CalculateOutputs(inputs);
            }
            return inputs;
        }

        public int GetMaxOutput(double[] input)
        {
            double[] outputs = CalculateOutputs(input);
            int maxIndex = 0;

            for (int i = 1; i < outputs.Length; i++)
            {
                if (outputs[i] > outputs[maxIndex])
                    maxIndex = i;
            }

            return maxIndex;
        }

        public double GetMaxOutputValue(double[] input)
        {
            double[] outputs = CalculateOutputs(input);
            int maxIndex = 0;

            for (int i = 1; i < outputs.Length; i++)
            {
                if (outputs[i] > outputs[maxIndex])
                    maxIndex = i;
            }

            return outputs[maxIndex];
        }


        public List<double> GetNodes()
        {
            List<double> nodes = new List<double>();
            for (int i = 0; i < layers.Length; i++)
            {
                for (int j = 0; j < layers[i].biases.Length; j++)
                {
                    nodes.Add(layers[i].biases[j]);
                }
            
            }

            return nodes;
        }
    
        public List<double> GetWeights()
        {
            List<double> weigths = new List<double>();
            for (int i = 0; i < layers.Length; i++)
            {
                for (int j = 0; j < layers[i].weights.GetLength(0); j++)
                {
                    for (int k = 0; k < layers[i].weights.GetLength(1); k++)
                    {
                        weigths.Add(layers[i].weights[j, k]);
                    }
               
                }
            
            }

            return weigths;
        }
    
        public void Learn(double[] input, double[] expectedOutput, float learnRate)
        {
            if (backpropagation)
            {
                CalculateBackpropagationGradient(input, expectedOutput);
            }
            else
            {
                CalculateGradient(input, expectedOutput);
            }
        
            //Importante no aplciar el gradiente en el mismo loop en el que se asigna, ya que estariamos actualizando con datos alterados.
            foreach (var layer in layers)
            {
                layer.ApplyGradient(learnRate / input.Length);
            }
            if(backpropagation)
                ClearAllCostGradients();
        
        
        }

        private void CalculateGradient(double[] input, double[] expected)
        {
            double cost = Cost(input, expected);
            foreach (var layer in layers)
            {
                for (int i = 0; i < layer.lengthOut; i++)
                {
                    layer.biases[i] += H;
                    double newCost = Cost(input, expected);
                    layer.biases[i] -= H;
                    layer.costGradientBiases[i] = (newCost - cost) / H;

                    for (int j = 0; j < layer.lengthIn; j++)
                    {
                        layer.weights[j, i] += H;
                        double newCostW = Cost(input, expected);
                        layer.weights[j, i] -= H;
                        layer.costGradientWeights[j, i] = (newCostW - cost) / H;
                    }
                }
            }
        }

        private void CalculateBackpropagationGradient(double[] learnData, double[] expectedOutput)
        {
            //actualizar todos los valores de la red
            CalculateOutputs(learnData);
            //Pesos en la ultima capa
            //
            //                                   L     L
            //                                 dz     da    dC
            //      L-1      L       L           j     j 
            //     a * sig'(dz) * 2(a - y)^2 = ___ * ___ * ___  
            //      j        j      j   j        L-1    L     L
            //                                 dw     dz    da
            //                                   jk    j     j
            Layer output = layers[layers.Length - 1];
            double[] neuronValues = output.CalculateOutpuNeuronDerivativeValues(expectedOutput);
            output.UpdateGradientsBackP(neuronValues);

            for (int hiddenLayer = layers.Length - 2; hiddenLayer >= 0; hiddenLayer--)
            {
                Layer l = layers[hiddenLayer];
                neuronValues = l.CalculateHiddenLayerNeuronDerivativeValues(neuronValues, layers[hiddenLayer+1]);
                l.UpdateGradientsBackP(neuronValues);
            }
        
        
        }
        public void ClearAllCostGradients()
        {
            foreach (var layer in layers)
            {
                layer.ClearGradients();
            }
        }
   
    }
    public class Layer
    {
        //layer normal info
        public int lengthIn, lengthOut;
        public double[,] weights;
        public double[] biases;
        //Learning layer data
        public double[,] costGradientWeights;
        public double[] costGradientBiases;
        private double[] weightedInputs;
        private double[] nodeActOutputs;
        private double[] inputs;
        private bool backpropagation;
        public Activation activation = Activation.Step;
        public const float H = 0.0001f;
        public float inertia = 0;
        private bool sigmoidAct = true;
        public Layer(int numIn,int numOut, Activation activation, bool backpropagation, float inertia = 0)
        {
            this.activation = activation;
            this.backpropagation = backpropagation;
            lengthIn = numIn;
            lengthOut = numOut;
            inputs = new double[numIn];
            weights = new double[numIn, numOut];
            costGradientWeights = new double[numIn, numOut];
            biases = new double[numOut];
            costGradientBiases = new double[numOut];
            nodeActOutputs = new double[numOut];
            weightedInputs = new double[numOut];
            this.inertia = inertia;
            //Debug.Log(activation);
        }

        public void ApplyGradient(float learnRate)
        {
            for (int i = 0; i < lengthOut; i++)
            {
                for (int j = 0; j < lengthIn; j++)
                {
                    weights[j, i] -= costGradientWeights[j, i] * learnRate;
                }
                biases[i] -= costGradientBiases[i] * learnRate;
            }
        }
        public void InitRandomWeights(bool sqrt = true)
        {
            for (int i = 0; i < weights.GetLength(1); i++)
            {
                for (int j = 0; j < weights.GetLength(0); j++)
                {
                    weights[j, i] = GetRandomInitValueWeights(sqrt);
                }
                biases[i] = GetRandomInitValueBias(sqrt);
            }
        }

        public double  GetRandomInitValueBias(bool sqrt = true)
        {
            float valueBias = Random.Range(-1f, 1f);
            if(sqrt)
                valueBias = valueBias / Mathf.Sqrt(lengthOut);
            return valueBias;
        }

        public double GetRandomInitValueWeights(bool sqrt = true)
        {
            float value = Random.Range(-1f, 1f);
            if (sqrt)
                value = value / Mathf.Sqrt(lengthIn);
            return value;
        }
        public double[] CalculateOutputs(double[] inputsIn)
        {
            this.inputs = inputsIn;
            weightedInputs = WeightResult(weights, inputs, biases);
            ActivateNeurons(weightedInputs);
            return nodeActOutputs;
        }

        public double[] ActivateNeurons(double[] weighted)
        {
            for (int i = 0; i < weighted.Length; i++)
            {
                nodeActOutputs[i] = ActivationFunction(weighted, i);
            }
            return nodeActOutputs;
        }

        public double StepActivation(double[] values, int index)
        {
            return values[index] > 0 ? 1 : 0;
        }
    
        public static double SigmoidActivation(double[] values, int index)
        {
            return 1 / (1 + Math.Exp(-values[index]));
        }

        public static double RELUActivation(double[] values, int index)
        {
            //Debug.Log($"vale {value} =>  { (value <= 0 ? 0 : value)}");
            return values[index] <= 0 ? 0: values[index];
        }

        public static double RELUActivationDerivative(double[] values, int index)
        {
            return values[index] <= 0 ? 0 : 1;
        }
        public static double TanhActivation(double[] values, int index)
        {
            //Debug.Log($"vale {value} =>  { (value <= 0 ? 0 : value)}");
            return Math.Tanh(values[index]);
        }

        public static double TanhActivationDerivative(double[] values, int index)
        {
            var tanh = TanhActivation(values, index);
            return 1 - (tanh * tanh);
        }

        public static double SigmoidActivationDerivative(double[] values, int index)
        {
            var activation = SigmoidActivation(values, index);
            return activation * (1 - activation);
            //return SigmoidActivation(1 - activation);
        }

        public static double SoftmaxActivation(double[] values, int index)
        {
            double expSum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                expSum += Math.Exp(values[i]);
            }

            double res = Math.Exp(values[index]) / expSum;
            //Debug.Log("Softmax"+ res);
            return res;
        }

        public static double SoftmaxActivationDerivative(double[] values, int index)
        {
            double expSum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                expSum += Math.Exp(values[i]);
            }

            double ex = Math.Exp(values[index]);
            //Debug.Log("Softmax deriva" + (ex * expSum - ex * ex) / (expSum * expSum));
            return (ex * expSum - ex * ex) / (expSum * expSum);
        }

        public  double[] WeightResult(double[,] w, double[] x, double[] b)
        {
            if (w.GetLength(0) != x.GetLength(0))
            {
                Debug.LogError("Matrix dimension not match: " + w.GetLength(1) + ", " + x.GetLength(0));
                return null;
            }
            if (w.GetLength(1) != b.GetLength(0))
            {
                Debug.LogError("Matrix dimension not match: " + w.GetLength(0) + ", " + b.GetLength(0));
                return null;
            }
            double[] result = new double[w.GetLength(1)];
            for (int j = 0; j < w.GetLength(1); j++)
            {
                double value = 0;
                for (int i = 0; i < w.GetLength(0); i++)
                {
                    value += w[i, j] * x[i];
                }
                result[j] = value + b[j];
            }

            return result;
        }
    
        public void ClearGradients(){
            for (int j = 0; j < weights.GetLength(1); j++)
            {
                for (int i = 0; i < weights.GetLength(0); i++)
                {
                    costGradientWeights[i, j] *= inertia;
                }

                costGradientBiases[j] *= inertia;
            }   
        }
        public  double DataPointCostDerivative(double guess, double expected)
        {
            return 2 * (guess - expected);
        }
        public double[] CalculateOutpuNeuronDerivativeValues(double[] expectedOut)
        {
            double[] neuronValues = new double[expectedOut.Length];
            for (int i = 0; i < expectedOut.Length; i++)
            {
                //                          L
                //                            da    dC
                //          L       L           j 
                // o= sig'(dz) * 2(a - y)^2 = ___ * ___ 
                //          j      j   j        L     L
                //                            dz    da
                //                              j     j
                neuronValues[i] = ActivationDerivative(weightedInputs, i) *
                                  DataPointCostDerivative(nodeActOutputs[i], expectedOut[i]);
            }

            return neuronValues;
        }
    
    
        public void UpdateGradientsBackP(double[] neuronValues)
        {
            //           L
            //   dC    dz
            //           j    L   L+1         L+n    L-1  L   L+1         L+n
            //  ___ = ___ * [h * h *   ... * o]  =  a * [h * h *   ... * o]
            //     L     L                           j
            //   cw    dw
            //     jk    jk
            // Para lso bias se cambio w por 1
            for (int out_ = 0; out_ < lengthOut ; out_++)
            {
                for (int in_ = 0; in_ < lengthIn ; in_++)
                {
                    //El coste de gradiente es el sumatorio de todas las varianzas causadas por el peso inicial
                    costGradientWeights[in_, out_] += inputs[in_] * neuronValues[out_];
                }

                costGradientBiases[out_] += neuronValues[out_]; //*1
            }
        }
    
        public double[] CalculateHiddenLayerNeuronDerivativeValues(double[] nextLayerValues, Layer nextLayer)
        {
            double[] neuronValues = new double[lengthOut];
            for (int originNeuron = 0; originNeuron < lengthOut; originNeuron++)
            {
                double newNeuronValue = 0;
                for (int objetiveNeuron = 0; objetiveNeuron < nextLayerValues.Length; objetiveNeuron++)
                {
                    //    L-1    L-1
                    //   dz    da
                    //    j      j        L-1        L-1
                    //  ___ * ___  * p  = w  *  sig'(z) = h
                    //    L-2    L-1      jk         j
                    //   da    dz
                    //    jk     j
                    //Debug.Log("[In: " + lengthIn + ", Out: " + lengthOut + "] origin " + originNeuron + ", objetive " + objetiveNeuron);
                    //Debug.Log("NextLayer weights [" + nextLayer.weights.GetLength(0) + "x" + nextLayer.weights.GetLength(1) +"], nextLayerValues: " + nextLayerValues.Length);
                    newNeuronValue += (nextLayer.weights[originNeuron, objetiveNeuron] * nextLayerValues[objetiveNeuron]);
                }


                newNeuronValue *= ActivationDerivative(weightedInputs, originNeuron);
                neuronValues[originNeuron] = newNeuronValue;
            }
        
            return neuronValues;
        }

        public double ActivationDerivative(double[] values, int index)
        {
            switch (activation)
            {
                case Activation.Step:
                    Debug.LogError("Step derivative not implemented");
                    return 0;
                case Activation.Sigmoid:
                    return SigmoidActivationDerivative(values, index);
                case Activation.ReLU:
                    return RELUActivationDerivative(values, index);
                case Activation.Softmax:
                    return SoftmaxActivationDerivative(values, index);
                case Activation.Tanh:
                    return TanhActivationDerivative(values, index);
                default:
                    Debug.LogError("Activation derivative not implemented");
                    return 0;
            }
        }

        public double ActivationFunction(double[] values, int index)
        {
            switch (activation)
            {
                case Activation.Step:
                    return StepActivation(values, index);
                case Activation.Sigmoid:
                    return SigmoidActivation(values, index);
                case Activation.ReLU:
                    return RELUActivation(values, index);
                case Activation.Softmax:
                    return SoftmaxActivation(values, index);
                case Activation.Tanh:
                    return TanhActivation(values, index);
                default:
                    Debug.LogError("Activation function not implemented");
                    return 0;
            }
        }
    }

    public class GeneticNetwork : NeuralNetwork
    {
        private int nodes = 0;
        public float fitness = 0;
        public enum GeneticOperation
        {
            None,
            PartialMutation,
            ImpartialMutation,
            WeightCrossover,
            NodeCrossover,
            NodeMutation
        }
        public GeneticOperation operation = GeneticOperation.None;
        //private int variables = 0;
        public GeneticNetwork(int[] layerSizes, Activation activation, Activation outputActivation) : base(layerSizes, activation, outputActivation, true, 0)
        {
            nodes = 0;
            for (int l = 0; l < layers.Length; l++)
            {
                nodes += layers[l].weights.GetLength(1);
            }
            //CalculateVariableAmount();
        }
        /*
    public int GetVariableAmount()
    {
        return variables;
    }
    private void CalculateVariableAmount()
    {
        int count = 0;
        foreach(var layer in layers)
        {
            count += layer.biases.Length;
            count += layer.weights.Length;
        }
        variables = count;
    }
    */
        public void PartialMutation(GeneticNetwork parent, float probability)
        {
            operation = GeneticOperation.PartialMutation;
            for(int l = 0; l < layers.Length; l++)
            {
                Layer layer = layers[l];
                for (int i = 0; i < layer.weights.GetLength(1); i++)
                {
                    for (int j = 0; j < layer.weights.GetLength(0); j++)
                    {
                        layer.weights[j, i] = parent.layers[l].weights[j, i];
                        if (Random.Range(0f, 1f)<probability)
                            layer.weights[j, i] += layer.GetRandomInitValueWeights(false);
                    }
                    layer.biases[i] = parent.layers[l].biases[i];
                    if (Random.Range(0f, 1f) < probability)
                        layer.biases[i] += layer.GetRandomInitValueBias(false);
                }
            }
        }

        public void ImpartialMutation(GeneticNetwork parent, float probability)
        {
            operation = GeneticOperation.ImpartialMutation;
            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = layers[l];
                for (int i = 0; i < layer.weights.GetLength(1); i++)
                {
                    for (int j = 0; j < layer.weights.GetLength(0); j++)
                    {
                        layer.weights[j, i] = parent.layers[l].weights[j, i];
                        if (Random.Range(0f, 1f) < probability)
                            layer.weights[j, i] = layer.GetRandomInitValueWeights(false);
                    }
                    layer.biases[i] = parent.layers[l].biases[i];
                    if (Random.Range(0f, 1f) < probability)
                        layer.biases[i] = layer.GetRandomInitValueBias(false);
                }
            }
        }

        public void NoMutation(GeneticNetwork parent)
        {
            operation = GeneticOperation.None;
            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = layers[l];
                for (int i = 0; i < layer.weights.GetLength(1); i++)
                {
                    for (int j = 0; j < layer.weights.GetLength(0); j++)
                    {
                        layer.weights[j, i] = parent.layers[l].weights[j, i];
                    }
                    layer.biases[i] = parent.layers[l].biases[i];
                }
            }
        }

        public void MutateNodes(GeneticNetwork parent, int nodes)
        {
            List<int> selectedNodes = new List<int>(nodes);
            for(int i = 0; i < nodes; i++)
            {
                int newNode = Random.Range(0, nodes);
                if (selectedNodes.Contains(newNode))
                {
                    i--;
                }
                else
                {
                    selectedNodes.Add(newNode);
                }
            }
            operation = GeneticOperation.NodeMutation;
            int nodeId = 0;
            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = layers[l];
           
                for (int i = 0; i < layer.weights.GetLength(1); i++)
                {
                    bool randomize = selectedNodes.Contains(nodeId);
                    for (int j = 0; j < layer.weights.GetLength(0); j++)
                    {
                        layer.weights[j, i] = parent.layers[l].weights[j, i];
                        if (randomize)
                            layer.weights[j, i] += layer.GetRandomInitValueWeights(false);
                    }
                    layer.biases[i] = parent.layers[l].biases[i];
                    if (randomize)
                        layer.biases[i] += layer.GetRandomInitValueBias(false);
                    nodeId++;
                }
            }
        }

        public void WeightCrossover(GeneticNetwork parent1, GeneticNetwork parent2)
        {
            operation = GeneticOperation.WeightCrossover;
            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = layers[l];
                for (int i = 0; i < layer.weights.GetLength(1); i++)
                {
                    for (int j = 0; j < layer.weights.GetLength(0); j++)
                    {
                        layer.weights[j, i] = Random.Range(0, 2) == 0 ? parent1.layers[l].weights[j, i] : parent2.layers[l].weights[j, i];
                    }
                    layer.biases[i] = Random.Range(0, 2) == 0 ? parent1.layers[l].biases[i] : parent2.layers[l].biases[i];
                }
            }
        }

        public void NodeCrossover(GeneticNetwork parent1, GeneticNetwork parent2)
        {
            operation = GeneticOperation.NodeCrossover;
            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = layers[l];
                for (int i = 0; i < layer.weights.GetLength(1); i++)
                {
                    GeneticNetwork parent = Random.Range(0, 2) == 0 ? parent1 : parent2;
                    for (int j = 0; j < layer.weights.GetLength(0); j++)
                    {
                        layer.weights[j, i] = parent.layers[l].weights[j, i];
                    }
                    layer.biases[i] = parent.layers[l].biases[i];
                }
            }
        }
    }
}