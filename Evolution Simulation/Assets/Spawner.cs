using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Agent[] agents;
    public float worldSize;

    private void Start()
    {
        foreach(Agent agent in agents)
        {
            for (int i = 0; i < agent.startingCount; i++) {
                GameObject spawn = Instantiate(agent.prefab, new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize)), Quaternion.identity, transform);
            }
            agent.textDisplay.SetText(agent.name + ": " + agent.startingCount);
            agent.currentCount = agent.startingCount;
        }
    }

    public void UpdateDisplay(Agent agent)
    {
        agent.textDisplay.SetText(agent.name + "s: " + agent.currentCount);
    }

    [System.Serializable]
    public class Agent
    {
        public GameObject prefab;
        public int startingCount;
        public int currentCount;
        public string name;
        public TextMeshProUGUI textDisplay;
    }
}
