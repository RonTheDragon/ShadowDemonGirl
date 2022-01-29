using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    public GameObject Ghost;
    public float SpawnEveryXseconds = 5;
    public int MaxGhostsSpawnable = 20;
    public Transform[] SpawnLocations;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer <= 0)
        {
            timer = SpawnEveryXseconds;
            if (MaxGhostsSpawnable > GameObject.FindGameObjectsWithTag("Ghost").Length)
            {
                int r = Random.Range(0,SpawnLocations.Length);
                Instantiate(Ghost, SpawnLocations[r].position, SpawnLocations[r].rotation);
            }
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }
}
