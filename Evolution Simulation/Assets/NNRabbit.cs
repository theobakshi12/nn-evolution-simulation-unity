using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class NNRabbit : MonoBehaviour
{
    public int generation;

    public float parentSpeed;
    public float speed, turnSpeed;

    public float maxEnergy, energyLoss;

    public int eyesightDetail;
    public float fieldOfView, viewDistance;

    public float eatingTime, breedingTime;

    public float mutationChance, mutationAmount;
    public float startingMutationChance, startingMutationAmount;

    public float energy;
    public float breedingUrge;
    public bool male;
    public LayerMask food;
    public LayerMask rabbit;
    public float worldSize;

    private bool eating;
    private bool breeding;

    private Spawner spawner;
    private FoodSpawner foodSpawner;
    private Simulation simulator;
    private NeuralNetwork nn;

    private void Start()
    {
        energy = maxEnergy;
        spawner = GameObject.Find("Simulation").GetComponent<Spawner>();
        foodSpawner = GameObject.Find("Simulation").GetComponent<FoodSpawner>();
        simulator = GameObject.Find("Simulation").GetComponent<Simulation>();
        nn = GetComponent<NeuralNetwork>();

        OnBirth();
    }

    private void OnBirth()
    {
        if (Random.Range(0f, 1f) > 0.5f) male = true;
        //speed = parentSpeed * Random.Range(0.8f, 1.2f);
        if (generation == 0) nn.MutateNetwork(startingMutationChance, startingMutationAmount);
        else nn.MutateNetwork(mutationChance, mutationAmount);
    }

    private void Update()
    {
        //Energy
        if (energy < 0)
            Die();

        if (eating || breeding)
            return;

        //Asexual reproduction
        if (energy > 3) for(int i = 0; i < 4; i++) Reproduce();
        energy -= energyLoss * Time.deltaTime * simulator.accelerator;

        //Brain
        float[] inputsToNN = Eyesight(eyesightDetail, fieldOfView, viewDistance);

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

    private float[] Eyesight(int detail, float FOV, float viewDistance)
    {
        float[] distances = new float[detail];

        for (int i = 0; i < detail; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward + new Vector3(0f, (FOV / (detail - 1) * i - FOV / 2), 0f), out hit, viewDistance, food))
            {
                if (hit.collider.CompareTag("Food"))
                    distances[i] = hit.distance;
            }
            else distances[i] = viewDistance;

            Debug.DrawRay(transform.position, transform.forward + new Vector3(0f, (FOV / (detail - 1) * i - FOV / 2), 0f) * viewDistance);
        }
        return distances;
    }

    private void Reproduce()
    {
        energy -= 2;
        GameObject offspring = Instantiate(spawner.agents[1].prefab, transform.position + new Vector3(Random.Range(-0.5f,0.5f),0,Random.Range(-0.5f,0.5f)), Quaternion.identity, transform.parent);
        offspring.GetComponent<NNRabbit>().parentSpeed = speed;
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
        if (other.CompareTag("Food"))
        {
            Destroy(other.gameObject);
            eating = true;
            if (simulator != null)
            {
                Invoke(nameof(FinishEating), eatingTime / simulator.accelerator);
            }
            else Invoke(nameof(FinishEating), eatingTime);
            energy += 1;
        }
    }
}
