using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    public int[] networkShape = { 5, 32, 2 };
    public Layer[] layers;

    public void Awake()
    {
        layers = new Layer[networkShape.Length - 1];

        for(int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(networkShape[i], networkShape[i + 1]);
        }
    }

    public float[] Brain(float[] inputs)
    {
        for(int i = 0; i < layers.Length; i++)
        {
            if (i == 0)
            {
                layers[i].Forward(inputs);
                layers[i].Activation();
            }
            else if(i == layers.Length - 1)
            {
                layers[i].Forward(layers[i - 1].nodeArray);
            }
            else
            {
                layers[i].Forward(layers[i - 1].nodeArray);
                layers[i].Activation();
            }
        }
        return layers[layers.Length - 1].nodeArray;
    }

    public Layer[] CopyLayers()
    {
        Layer[] newLayers = new Layer[networkShape.Length - 1];
        for(int i = 0; i < layers.Length; i++)
        {
            newLayers[i] = new Layer(networkShape[i], networkShape[i + 1]);
            System.Array.Copy(layers[i].weightsArray, newLayers[i].weightsArray, layers[i].weightsArray.GetLength(0) * layers[i].weightsArray.GetLength(1));
            System.Array.Copy(layers[i].biasesArray, newLayers[i].biasesArray, layers[i].biasesArray.GetLength(0));
        }
        return newLayers;
    }

    public void MutateNetwork(float chance, float amount)
    {
        for(int i = 0; i < layers.Length; i++)
        {
            layers[i].MutateLayer(chance, amount);
        }
    }

    public class Layer
    {
        public float[,] weightsArray;
        public float[] biasesArray;
        public float[] nodeArray;

        private int n_nodes;
        private int n_inputs;

        public Layer(int n_inputs, int n_nodes)
        {
            this.n_nodes = n_nodes;
            this.n_inputs = n_inputs;

            weightsArray = new float[n_nodes, n_inputs];
            biasesArray = new float[n_nodes];
            nodeArray = new float[n_nodes];
        }

        public void Forward(float[] inputsArray)
        {
            nodeArray = new float[n_nodes];

            for(int i = 0; i < n_nodes; i++)
            {
                for(int j = 0; j < n_inputs; j++)
                {
                    nodeArray[i] += weightsArray[i,j] * inputsArray[j];
                }

                nodeArray[i] += biasesArray[i];
            }
        }

        public void Activation()
        {
            for(int i = 0; i < n_nodes; i++)
            {
                //implement ReLU activation function
                if (nodeArray[i] < 0)
                {
                    nodeArray[i] = 0;
                }
            }
        }
        public void MutateLayer(float chance, float amount)
        {
            for(int i = 0; i < n_nodes; i++)
            {
                for(int j = 0; j < n_inputs; j++)
                {
                    if (Random.Range(0f, 1f) < chance)
                        weightsArray[i, j] += Random.Range(-1f, 1f) * amount;
                }
                if (Random.Range(0f, 1f) < chance)
                    biasesArray[i] += Random.Range(-1f, 1f) * amount;
            }
        }
    }

    
}
