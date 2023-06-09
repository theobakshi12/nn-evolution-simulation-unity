using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Spawner;

public class FoodSpawner : MonoBehaviour
{
    public GameObject prefab;
    public int startingFood;
    public float growthRate;
    public float worldSize;
    public int count;
    private float growthCounter;
    private Simulation simulator;

    private void Start()
    {
        simulator = GetComponent<Simulation>();
        for(int i = 0; i < startingFood; i++)
        {
            Instantiate(prefab, new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize)), Quaternion.identity);
        }
        count = startingFood;
    }

    private void Update()
    {
        if(growthCounter <= 0)
        {
            for(int i = 0; i < simulator.accelerator; i++)
            {
                Instantiate(prefab, new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize)), Quaternion.identity);
                count++;
            }
            growthCounter = growthRate;
        }
        growthCounter -= Time.deltaTime;
    }
}
