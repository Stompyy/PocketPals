﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSkybox : MonoBehaviour {

	public float speedMultiplier;

	void Update ()
	{
        if (RenderSettings.skybox != null)
        {
            // Sets the float value of "_Rotation", adjust it by Time.time and a multiplier.
            RenderSettings.skybox.SetFloat("_Rotation", Time.time * speedMultiplier);
        }
	}
}
