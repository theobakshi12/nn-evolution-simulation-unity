using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class NNRabbit : MonoBehaviour
{
    public int generation;

    public float speed, turnSpeed;
    public float size;

    public float maxEnergy, energyConsumptionMultiplier;

    public int eyesightDetail;
    public float fieldOfView, viewDistance;

    public float eatingTime, breedingTime;

    public float variationChance, variationAmount;
    public float mutationChance, mutationAmount;
    public float startingMutationChance, startingMutationAmount;

    public float energy;
    public float breedingUrge;
    public bool male;
    public LayerMask food;
    public LayerMask rabbit;
    public float worldSize;

    private float energyConsumption;

    private bool eating;
    private bool breeding;

    private Spawner spawner;
    private FoodSpawner foodSpawner;
    private Simulation simulator;
    private NeuralNetwork nn;

    private void Start()
    {
        spawner = GameObject.Find("Simulation").GetComponent<Spawner>();
        foodSpawner = GameObject.Find("Simulation").GetComponent<FoodSpawner>();
        simulator = GameObject.Find("Simulation").GetComponent<Simulation>();
        nn = GetComponent<NeuralNetwork>();

        OnBirth();
    }

    private void OnBirth()
    {
        if (Random.Range(0f, 1f) > 0.5f) male = true;
        nn.AdjustNetwork(variationChance, variationAmount);
        if (generation == 0)
        {
            nn.MutateNetwork(startingMutationChance, startingMutationAmount);
        }
        else nn.MutateNetwork(mutationChance, mutationAmount);

        transform.localScale = new Vector3(size,size,size);
        energyConsumption = Mathf.Sqrt(speed * size);
        energy = maxEnergy * energyConsumption;
    }

    private void Update()
    {
        //Energy
        if (energy < 0)
            Die();

        if (eating || breeding)
            return;

        //Asexual reproduction
        if (energy > 3 * energyConsumption) for(int i = 0; i < 4; i++) Reproduce();

        energy -= energyConsumption * energyConsumptionMultiplier * Time.deltaTime * simulator.accelerator;

        //Brain
        float[] inputsToNN = Input(eyesightDetail, fieldOfView, viewDistance);
        float[] outputsFromNN = nn.Brain(inputsToNN);

        float FB = outputsFromNN[0];
        float LR = outputsFromNN[1];

        SimpleMove(FB, LR);
    }

    private void SimpleMove(float FB, float LR)
    {
        FB = Mathf.Clamp(FB, 0, 1);
        LR = Mathf.Clamp(LR, -1, 1);
        transform.position += transform.forward * FB * speed * Time.deltaTime * simulator.accelerator;
        transform.Rotate(0f, LR * turnSpeed * Time.deltaTime * simulator.accelerator,0f); 
    }

    private float[] Input(int detail, float FOV, float viewDistance)
    {
        float[] input = new float[detail * 3 + 1];

        for (int i = 0; i < detail; i++)
        {
            RaycastHit hit;

            float angle = FOV / (detail - 1) * i - FOV / 2;

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward;
            Vector3 rayPosition = transform.position + new Vector3(0, 0.05f, 0);

            if (Physics.Raycast(rayPosition, rayDirection, out hit, viewDistance, food))
            {
                input[i] = hit.distance;
            }
            else input[i] = viewDistance;

            if (Physics.Raycast(rayPosition, rayDirection, out hit, viewDistance, rabbit))
            {
                input[i + 5] = hit.distance;
                input[i + 10] = hit.collider.GetComponent<NNRabbit>().size;
            }
            else
            {
                input[i + 5] = viewDistance;
                input[i + 10] = 0;
            }
            input[15] = size;

            Debug.DrawRay(rayPosition, rayDirection * viewDistance, Color.red);
        }

        return input;
    }

    private void Reproduce()
    {
        energy -= 2 * energyConsumption;
        GameObject offspring = Instantiate(spawner.agents[1].prefab, transform.position + new Vector3(Random.Range(-0.5f,0.5f),0,Random.Range(-0.5f,0.5f)), Quaternion.identity, transform.parent);
        //offspring.GetComponent<NNRabbit>().speed = speed * Random.Range(0.9f,1.1f);
        //offspring.GetComponent<NNRabbit>().size = size * Random.Range(0.9f, 1.1f);
        offspring.GetComponent<NNRabbit>().generation = generation+1;
        offspring.GetComponent<NeuralNetwork>().layers = GetComponent<NeuralNetwork>().CopyLayers();
        spawner.agents[1].currentCount++;
        spawner.UpdateDisplay(spawner.agents[1]);
    }

    private void Die()
    {
        Destroy(gameObject);
        spawner.agents[1].currentCount--;
        spawner.UpdateDisplay(spawner.agents[1]);
    }

    private void FinishEating()
    {
        eating = false;
        foodSpawner.count--;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food") && size > 0.25f)
        {
            energy += 1;
            Destroy(other.gameObject);
            eating = true;
            if (simulator != null)
            {
                Invoke(nameof(FinishEating), eatingTime / simulator.accelerator);
            }
            else Invoke(nameof(FinishEating), eatingTime);
        }
        if (other.CompareTag("Rabbit") && size > other.GetComponent<NNRabbit>().size)
        {
            energy += 5 * other.GetComponent<NNRabbit>().size;
            Destroy(other.gameObject);
            eating = true;
            if (simulator != null)
            {
                Invoke(nameof(FinishEating), eatingTime / simulator.accelerator);
            }
            else Invoke(nameof(FinishEating), eatingTime);
        }
    }
}
