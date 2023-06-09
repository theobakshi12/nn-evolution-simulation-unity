using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Spawner;

public class FoodSpawner : MonoBehaviour
{
    public GameObject prefab;
    public int startingGrass;
    public float growthRate;
    public float worldSize;
    private float counter;
    private Simulation simulator;

    private void Start()
    {
        simulator = GetComponent<Simulation>();
        for(int i = 0; i < startingGrass; i++)
        {
            Instantiate(prefab, new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize)), Quaternion.identity);
        }
    }

    private void Update()
    {
        if(counter <= 0)
        {
            for(int i = 0; i < simulator.accelerator; i++)
            {
                Instantiate(prefab, new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize)), Quaternion.identity);
            }
            counter = growthRate;
        }
        counter -= Time.deltaTime;
    }
}
