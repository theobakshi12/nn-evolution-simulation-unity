using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Rabbit : MonoBehaviour
{
    public float maxEnergy;
    public float energyLoss;
    public float speed;
    public float perceptionRadius;
    public float eatingTime;
    public float breedingTime;

    public float energy;
    public float breedingUrge;
    public bool male;
    public GameObject objectTarget;
    public Vector3 roamTarget;
    public LayerMask food;
    public LayerMask rabbit;
    public float worldSize;

    private bool roaming;
    private bool eating;
    private bool findingMate;
    private bool breeding;
    public bool waitingForPartner;

    private Spawner spawner;
    private Simulation simulator;

    public float fathersSpeed;


    private void Start()
    {
        energy = maxEnergy;
        spawner = GameObject.Find("Simulation").GetComponent<Spawner>();
        simulator = GameObject.Find("Simulation").GetComponent<Simulation>();
        OnBirth();
    }

    private void OnBirth()
    {
        if (Random.Range(0f, 1f) > 0.5f) male = true;
        speed = fathersSpeed + Random.Range(-0.5f, 0.5f);
    }

    private void Update()
    {
        SimulationStep();
    }

    public void SimulationStep()
    {
        //Energy
        if (energy < 0)
        {
            Destroy(gameObject);
            spawner.agents[0].currentCount--;
            spawner.UpdateDisplay(spawner.agents[0]);
        }
        if (eating || waitingForPartner)
        {
            return;
        }
        if (breeding)
        {
            return;
        }
        energy -= energyLoss * Time.deltaTime * simulator.accelerator;

        //Breeding
        if (male)
        {
            breedingUrge += 0.05f * Time.deltaTime * simulator.accelerator;
            if (breedingUrge >= 1)
            {
                FindMate();
                findingMate = true;
            }
        }

        //Finding target
        if (objectTarget == null || objectTarget.Equals(null))
        {
            //If no nearby food start roaming
            if (!FindFood())
            {
                if ((transform.position - roamTarget).magnitude < 0.1f) roaming = false;
                //point in random direction
                if (!roaming)
                {
                    roamTarget = SetRoamTarget();
                    transform.LookAt(roamTarget);
                    roaming = true;
                }
            }
        }
        else
        {
            transform.LookAt(objectTarget.transform.position);
            if ((transform.position - objectTarget.transform.position).magnitude < 0.5f)
            {
                if (findingMate)
                {
                    breeding = true;
                    Invoke(nameof(FinishBreeding), breedingTime/simulator.accelerator);
                    breedingUrge = 0;

                }

                objectTarget = null;
            }

        }
        transform.position += transform.forward * speed * Time.deltaTime * simulator.accelerator;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grass"))
        {
            print("eating");
            objectTarget = null;
            eating = true;
            Invoke(nameof(FinishEating), eatingTime/simulator.accelerator);
            energy = maxEnergy;
            Destroy(other.gameObject);
        }
    }

    private bool FindFood()
    {
        Collider[] nearbyFood = Physics.OverlapSphere(transform.position, perceptionRadius, food);
        if (nearbyFood.Length > 0)
        {
            objectTarget = nearbyFood[0].gameObject;
            for (int i = 0; i < nearbyFood.Length; i++)
            {
                if ((transform.position - nearbyFood[i].transform.position).magnitude < (transform.position - objectTarget.transform.position).magnitude)
                {
                    objectTarget = nearbyFood[i].gameObject;

                }
            }      
            roaming = false;
            return true;
        }
        return false;
    }

    private bool FindMate()
    {
        Collider[] nearbyRabbits = Physics.OverlapSphere(transform.position, perceptionRadius, rabbit);
        if (nearbyRabbits.Length > 0)
        {
            objectTarget = nearbyRabbits[0].gameObject;
            for (int i = 0; i < nearbyRabbits.Length; i++)
            {
                if ((transform.position - nearbyRabbits[i].transform.position).magnitude < (transform.position - objectTarget.transform.position).magnitude && !nearbyRabbits[i].GetComponent<Rabbit>().male)
                {
                    objectTarget = nearbyRabbits[i].gameObject;

                }
            }
            if (objectTarget.GetComponent<Rabbit>().male) return false;
            roaming = false;
            objectTarget.GetComponent<Rabbit>().waitingForPartner = true;
            return true;
        }
        return false;
    }

    private float[] Eyesight(int detail, float FOV, float viewDistance)
    {
        float[] distances = new float[detail];

        for(int i =0; i < detail; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward + new Vector3(0f, (FOV / (detail - 1) * i - FOV / 2), 0f), out hit, viewDistance))
            {
                if (hit.collider.CompareTag("Food"))
                    distances[i] = hit.distance;
            }
            else distances[i] = viewDistance;
        }
        return distances;
    }

    private Vector3 SetRoamTarget()
    {
        Vector3 roamTarget = transform.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        if (roamTarget.x > worldSize) roamTarget.x = worldSize;
        if (roamTarget.x < -worldSize) roamTarget.x = -worldSize;
        if (roamTarget.z > worldSize) roamTarget.z = worldSize;
        if (roamTarget.z < -worldSize) roamTarget.z = -worldSize;

        return roamTarget;
    }

    private void CreateOffspring()
    {
        GameObject offspring = Instantiate(spawner.agents[0].prefab, transform.position + new Vector3(0.25f,0,0), Quaternion.identity, transform.parent);
        offspring.GetComponent<Rabbit>().fathersSpeed = speed;
        spawner.agents[0].currentCount++;
        spawner.UpdateDisplay(spawner.agents[0]);
    }

    private void FinishEating()
    {
        eating = false;
    }
    private void FinishBreeding()
    {
        breeding = false;
        CreateOffspring();
    }
}
