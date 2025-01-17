﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentGenerator : MonoBehaviour
{
	//instance callable from anywhere
	public static ContentGenerator Instance { set; get; }

	//List of all Resource stops currently in the players area
	//private List<Vector2> ResourceStopLocations = new List<Vector2>();

    //Basically the size of the grids to generate, less equals a greater area.
    public static int DecimalPlacesToRound = 2;

	//current in to look at for the pocketpal
	public int currentIndex = 0;

    public string ResourceSpotSeed = "Doombar";

	//random seeds
	private int ppCurrentSeed =0;
	private int ppNewSeed = 0;

    private int rsCurrentSeed = 0;
    private int rsNewSeed = 0;

	void Start()
	{
		Instance = this;
	}

    public List<Vector2> GenerateResourceSpots(double lat, double lon, int number)
    {
        //Generate the seed that should be the same for everyone in the rough area.
        double roundedLat = System.Math.Round(lat, DecimalPlacesToRound);
        double roundedLon = System.Math.Round(lon, DecimalPlacesToRound);
        string seed = ResourceSpotSeed + roundedLat + roundedLon;

        if (roundedLat < 0.001 && roundedLat > -0.001 && roundedLon <0.001 && roundedLat > -0.001) return null;

        rsNewSeed = seed.GetHashCode();

        //If the two seeds are equal (the player has not moved out of this zone) stop generating
        if (rsCurrentSeed == rsNewSeed)
        {
            return null;
        }
        else
        {
        }

        rsCurrentSeed = rsNewSeed;

        System.Random r = new System.Random(rsNewSeed);

        double maxVariance = Math.Pow(10, -1*(DecimalPlacesToRound));

        List<Vector2> ResourceStopLocations = new List<Vector2>();
        for (int i = 0; i < number; i++)
        {
            Vector2 latLon = new Vector2();
            latLon.x =  (float)GetRandomBetweenRange(r, maxVariance, roundedLat);
            latLon.y = (float)GetRandomBetweenRange(r, maxVariance, roundedLon);
            ResourceStopLocations.Add(latLon);
        }
        return ResourceStopLocations;
    }


    private static int GetSeed(string seed, double lat, double lon)
	{
		//Get the time init variables
		String timeSeed = "";
		DateTime dt = DateTime.UtcNow;
		int minute = dt.Minute;

		//We only want to look at the 10th minute.
		minute -= minute % 10;

		//Create the time seed, Animals will repeat if players are in the same spot on the same day next year.
		timeSeed += dt.Date.DayOfYear.ToString() + dt.Hour.ToString() + minute.ToString();

		//Round the lat lon positions to get rough areas.
		double roundedLat = System.Math.Round(lat, DecimalPlacesToRound);
		double roundedLon = System.Math.Round(lon, DecimalPlacesToRound);

		seed += roundedLat.ToString() + roundedLon.ToString() + timeSeed;

		return seed.GetHashCode();
	}

    public bool CheckForNewAnimalSeed(string seed, double lat, double lon, bool newList)
    {
        ppNewSeed = GetSeed(seed, lat, lon);

        if (ppNewSeed != ppCurrentSeed || newList)
        {
            ppCurrentSeed = ppNewSeed;
            return true;
        }
        return false;
    }

	public List<int> TryGenerateNewAnimalList(string seed, double lat, double lon, int numberOfAnimals, List<float>samples, bool newList)
	{

        List<int> GeneratedAnimalsIDs = new List<int>();


		//create the random using a seed which is shared by all in the same rough area.
		System.Random r = new System.Random(ppCurrentSeed);
		//Generate the 5 unique animals to be used for this areas spawns. 
		for (int i = 0; i < numberOfAnimals; i++)
		{
			GeneratedAnimalsIDs.Add(PocketPalSpawnManager.Sampler(r, samples));
		}
        return GeneratedAnimalsIDs;    
	}

    public static double GetRandomBetweenRange(System.Random r, double variance, double scale)
    {
        double halfVar = variance / 2;
        double randDub = (r.NextDouble()*variance)-halfVar;
        return randDub + scale;

    }

}