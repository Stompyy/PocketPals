﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VirtualSceneParent : MonoBehaviour
{
	// More than one instance now
//	VirtualSceneParent Instance { set; get; }

	public enum EnvironmentType {
		woodland, wetland, coast
	}

	public EnvironmentType environmentType;


	public VirtualGardenSpawn[] AnimalObjects;
	public VGUIManager gUIManager;
	public GameObject centreOfMap;
	Vector3 centreOfMapPosition;

	public GameObject ARButton;

	// The default distance of the camera from the targeted PPal in the virtual garden in inspect
	float VGPPalInspectDistance = 2.0f;

	// For default VG view
	float VGPPalViewDistance = 4.0f;

	// Used to cycle through the virtual garden's PPals
	int currentLookedAtPPalIndex;

	bool hasAPocketPal = false;

	/*
    private void OnEnable()
  {
 
  }
*/
	public void InitVGTour () {

		Camera gameCamera = Camera.main;

        hasAPocketPal = false;

		currentLookedAtPPalIndex = GlobalVariables.VGCurrentIndex;


		centreOfMapPosition = centreOfMap.transform.position;

        foreach (VirtualGardenSpawn obj in AnimalObjects)
        {
            //make sure all are inactive and not used, unless check is correct.
			obj.animalObj.SetActive (false);
			obj.Used = false;

            //Check to see if the player has the pocketpal.
            PocketPalData data = LocalDataManager.Instance.TryGetPocketPal(obj.ID);
            if (data != null)
            {
                hasAPocketPal = true;

                //set the animal obj to have the data of the collected pocketpal.
                obj.SetAnimalData(data);

                //we Know the player owns the pocketpal so make used true and make it active in scene
				obj.Used = true;
                obj.animalObj.SetActive(true);

				// Set the inspect position field
				Vector3 PPPosition = obj.animalObj.transform.position;

				float camDistanceModifier = obj.animalObj.GetComponent<VirtualGardenInfo> ().camDistanceModifier; 
				obj.camDistanceModifier = camDistanceModifier; 

				// Get the real world distance between the centre of the viewport and the 0.25f, 0.25f of the viewport at PPal distance
				float VGInfoCamOffsetHorizontal = (gameCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, VGPPalInspectDistance * camDistanceModifier)) -
					gameCamera.ViewportToWorldPoint(new Vector3(0.25f, 0.0f, VGPPalInspectDistance * camDistanceModifier))).magnitude;

				float VGInfoCamOffsetVertical = (gameCamera.ViewportToWorldPoint(new Vector3(0.0f, 0.5f, VGPPalInspectDistance * camDistanceModifier)) -
					gameCamera.ViewportToWorldPoint(new Vector3(0.0f, 0.3f, VGPPalInspectDistance * camDistanceModifier))).magnitude;

				obj.camInspectPosition = PPPosition - (PPPosition - centreOfMapPosition).normalized * VGPPalInspectDistance * camDistanceModifier;

				// Cross product of the direction vector to the PPal and V3.up will add the relative offset 
				obj.camInspectLookAtPosition = PPPosition + Vector3.Cross((centreOfMapPosition - PPPosition).normalized, Vector3.up) * VGInfoCamOffsetHorizontal + 
					Vector3.up * VGInfoCamOffsetVertical;

            }
        }
		if (hasAPocketPal)
        {
            if (!AnimalObjects[currentLookedAtPPalIndex].Used)
            {
                for (int i = 0; i < AnimalObjects.Length; i++)
                {
                    if (AnimalObjects[i].Used)
                        currentLookedAtPPalIndex = i;
                }
            }
			// Initialise the idle camera action variables when no touches
			CameraController.Instance.VGInitLookAtNextPPal (GetCurrentPPal ());
		} else {
			ARButton.SetActive (false);
		}
    }

    public void SetObtained(int id)
    {
        GetGardenSpawn(id).animalObj.SetActive(true);
    }

    public VirtualGardenSpawn GetGardenSpawn(int id)
    {
        foreach (VirtualGardenSpawn vgs in AnimalObjects)
        {
            if (vgs.ID == id) return vgs;
        }
        return null;
    }

	public GameObject GetNextPPal () {

		foreach (VirtualGardenSpawn PPal in AnimalObjects) {
			// If the current index is the end of the array, then set as zero
			if (currentLookedAtPPalIndex >= AnimalObjects.Length - 1)
				currentLookedAtPPalIndex = 0;
			else
				// else increment index
				currentLookedAtPPalIndex++;

			// Look at that index value
			var indexVGS = AnimalObjects [currentLookedAtPPalIndex];

			// Next check whether it is in the inventory
			if (indexVGS.Used)
            {

                //Set the inspect data in the virtual garden UI manager
                gUIManager.SetInspectData(AnimalObjects[currentLookedAtPPalIndex]);

				// Update the global variable
				GlobalVariables.VGCurrentIndex = currentLookedAtPPalIndex;

                // Return the GameObject of that index in the AnimalObjects
                return indexVGS.animalObj;
			}
		}
		return null;
	}

	public GameObject GetPreviousPPal () {

		foreach (VirtualGardenSpawn PPal in AnimalObjects) {
			// If the current index is the start of the array, then set as the last
			if (currentLookedAtPPalIndex == 0)
				currentLookedAtPPalIndex = AnimalObjects.Length - 1;
			else
			// else deccrement index
			currentLookedAtPPalIndex--;

			// Look at that index value
			var indexVGS = AnimalObjects [currentLookedAtPPalIndex];

			// Next check whether it is in the inventory
			if (indexVGS.Used) {

                //Set the inspect data in the virtual garden UI manager
				gUIManager.SetInspectData(AnimalObjects[currentLookedAtPPalIndex]);

				// Update the global variable
				GlobalVariables.VGCurrentIndex = currentLookedAtPPalIndex;

				// Return the GameObject of that index in the AnimalObjects
				return indexVGS.animalObj;
			}
		}
		return null;
	}

	public GameObject GetCurrentPPal () {

		if (hasAPocketPal) {

			// Next check whether it is in the inventory
			if (AnimalObjects [currentLookedAtPPalIndex].Used) {

				//Set the inspect data in the virtual garden UI manager
				gUIManager.SetInspectData (AnimalObjects [currentLookedAtPPalIndex]);

				// Update the global variable
				GlobalVariables.VGCurrentIndex = currentLookedAtPPalIndex;

				return AnimalObjects [currentLookedAtPPalIndex].animalObj;
			}
		}

		return null;
	}

	public Vector3 GetInspectLookAtPosition () {

		// Get the current looked at PPal's inspect look at position
		return AnimalObjects[currentLookedAtPPalIndex].camInspectLookAtPosition;
	}

	public Vector3 GetInspectPosition () {

		// Get the current looked at PPal's inspect position
		return AnimalObjects[currentLookedAtPPalIndex].camInspectPosition;
	}

	public Vector3 GetViewPosition () {
		var PPalTarget = AnimalObjects [currentLookedAtPPalIndex]; 
		var PPPos = PPalTarget.animalObj.transform.position; 
		return PPPos - (PPPos - centreOfMapPosition).normalized * VGPPalViewDistance * PPalTarget.camDistanceModifier; 
	}

	public int GetPPalIndex() {
		return currentLookedAtPPalIndex;
	}
}

[Serializable]
public class VirtualGardenSpawn
{
    public int ID;
    public bool Used;
    public GameObject animalObj;
    private PocketPalData animalData;
	public Vector3 camInspectLookAtPosition { get; set; }
	public Vector3 camInspectPosition { get; set; }
	public float camDistanceModifier { get; set; }

    public PocketPalData GetAnimalData() { return animalData; }
    public void SetAnimalData(PocketPalData ppd) { animalData = ppd; }
}
