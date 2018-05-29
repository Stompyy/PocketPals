﻿using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is responsible for the spawning of Pocket Pals
// The script is not as sophisticated as required, but is approproate for the prototype and testing

public class PocketPalSpawnManager : MonoBehaviour
{

    public static PocketPalSpawnManager Instance { set; get; }

    // How long between each spawn
    public float avgSpawnTime = 2f;

    //Spawn Variance in percent
    [Tooltip("Max percentage change of the avereage spawn time.")]
    public float spawnTimeVariance = 50.0f;
    private float normalisedVariance = 0.0f;

    // An array of spawn locations Pocket Pals can spawn from.
    public Transform[] spawnPoints;

    //List of the rarities of the index
    private List<float> rarityList = new List<float>();

    bool shouldSpawn = true;

    //Used for girl distance and To get the gps map
    public GameObject girl;
    
    //Ref to the map, to set the parent of the pocketpals once they spawn
    private GPS gpsMap;

    // The max number of Pocket Pals that can spawn at any one time
    public int maxPocketPals = 20;

    //distance before pocketpals despawn
	public int maxPocketPalDistance = 20;

	// The minimum distance allowed between spawning pocket pals to avoid overlaps
	float minimumDistanceBetweenSpawns = 4.0f;

    //List of all the spawned pocketpals
    private List<GameObject> spawnedPocketPals = new List<GameObject>();

	void Start ()
	{
        Instance = this;

        gpsMap = girl.GetComponent<GPS>();

        //Iter throught the PocketPalscripts and set their IDs
        foreach (GameObject o in AssetManager.Instance.PocketPals)
        {
            rarityList.Add(o.GetComponent<PocketPalParent>().Rarity);
        }

        //making it percentage based seem easier too understand, but harder to work with.
        normalisedVariance = spawnTimeVariance / 100;

        // Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time
        StartCoroutine(Spawn());
	}

    public void PocketpalCollected(GameObject obj)
    {
        LocalDataManager.Instance.AddPocketPal(obj);
        DespawnPocketPal(obj);
    }

    private GameObject GetWeightedPocketPal()
    {
        int index = Sampler(rarityList);
        return AssetManager.Instance.PocketPals[index];
    }

    private float GetSpawnDelay()
    {
        return Random.Range(avgSpawnTime - normalisedVariance, avgSpawnTime + normalisedVariance);
    }

    private IEnumerator Spawn()
    {
        bool StartDelay = true;

        while (shouldSpawn)
        {
            if (StartDelay)
            {
                StartDelay = false;
                yield return new WaitForSeconds(GetSpawnDelay());
            }

            //check to see if any pps are too far away. If so destroy...
            CheckForDespawns();

            // Checks whether the max number of Pocket Pals have been spawned 
            if (spawnedPocketPals.Count >= maxPocketPals || !gpsMap.GetMapInit())    
            {
                yield return new WaitForSeconds(GetSpawnDelay());
            }
            else
            {
                // Find a random index between zero and one less than the number of spawn points

                int RandomPocketPal = Random.Range(0, AssetManager.Instance.PocketPals.Length);

				// Even though these are set, the compiler requires them to be initialised here
				Vector3 spawnPosition = Vector3.zero;
				int spawnPointIndex = 0, tempCount = 0, maxCount = 10;
				bool validSpawnFound = false;

				// Find a randomly chosen spawn position that does not overlap an existing pocketPal
				// As a safety measure, include a count to break out if no valid positions can be found
				while (!validSpawnFound || tempCount < maxCount) {

					// Find a randomly chosen spawn position
					// Better way may be to randomise the spawnPoints and go through them all
					spawnPointIndex = Random.Range (0, spawnPoints.Length);
					spawnPosition = spawnPoints [spawnPointIndex].position;

					// Check if a valid spawn position
					if (DoesNotOverlapExistingPPal (spawnPosition)) {
						validSpawnFound = true;
					}

					// Increment count
					tempCount++;
				}

				// If valid spawn position found then spawn, otherwise wait and repeat
				if (validSpawnFound) {

					Quaternion rot = spawnPoints [spawnPointIndex].rotation;

					// Create an instance of the prefab at select pocketpal via rarity 
					GameObject clone = Instantiate (GetWeightedPocketPal (), spawnPosition, rot);
					spawnedPocketPals.Add (clone);

					// Increases the currentPocketPals value by 1
					clone.transform.parent = gpsMap.currentMap.transform;
				}

                yield return new WaitForSeconds(GetSpawnDelay());
            }
        }
    }

    private static int Sampler(List<float> weightings)
    {
        int index = 0;
        float total = 0;

        //get total of floats
        foreach (float f in weightings)
        {
            total += f;
        }

        //if they all have no probability return random
        if (total == 0) return Random.Range(0, weightings.Count);

        //seudo-random sampling
        float accum = 0;
        float tmp = Random.Range(0, total);
        for (int i = 0; i < weightings.Count; i++)
        {
            accum += weightings[i];
            if (accum >= tmp)
            {
                index = i;
                break;
            }
        }

        return index;
    }

    private void DespawnPocketPal(GameObject obj)
    {
        spawnedPocketPals.Remove(obj);
        Destroy(obj);
    }

    public void DespawnAll()
    {
        for (int i = 0; i < spawnedPocketPals.Count; i++)
        {
            DespawnPocketPal(spawnedPocketPals[i]);
        }
    }

    private void CheckForDespawns()
    {
        for(int i = 0; i < spawnedPocketPals.Count; i++)
        {
            //not sure why this check is makes it work. Just does.
            if (spawnedPocketPals[i] == null || girl == null) break;

            if (Vector3.Magnitude(girl.transform.position - spawnedPocketPals[i].transform.position) > maxPocketPalDistance)
            {
                DespawnPocketPal(spawnedPocketPals[i]);
            }
        }
    }

	bool DoesNotOverlapExistingPPal (Vector3 spawnPosition) {

		// Check position against all currently spawned PPal positions
		foreach (GameObject PPal in spawnedPocketPals) {

			// If deemed too close
			if ((spawnPosition - PPal.transform.position).magnitude < minimumDistanceBetweenSpawns) {

				// Break out early returning false
				return false;
			}
		}

		// If all PPal distance checks pass then return true
		return true;
	}
}