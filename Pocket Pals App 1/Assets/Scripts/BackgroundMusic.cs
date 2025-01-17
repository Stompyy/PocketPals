﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour {


	public static BackgroundMusic Instance { set; get; }

	AudioSource backgroundMusic;

    public bool playMusic = true;

	private void Start()
	{
		Instance = this;

		DontDestroyOnLoad(this.gameObject);
	}

	void Awake()
	{
		// Don't destroy on load does not stop new instances from being instantiated on scene load. This will check and delete
		if (FindObjectsOfType (typeof(BackgroundMusic)).Length > 1) {
			DestroyImmediate (gameObject);
		} else
			backgroundMusic = GetComponent<AudioSource> ();
	}

	public void StartBackgroundMusic ()
    {
        if (playMusic)
        {
            backgroundMusic.Play();
        }
        else
        {
            backgroundMusic.Stop();
        }
	}

    public void StopMusic()
    {
        Instance.backgroundMusic.Stop();
    }

    public void Mute()
    {
        StopMusic();
        playMusic = false;
    }

    public void UnMute()
    {
        if(!playMusic) StartBackgroundMusic();
        playMusic = true;

    }

	public void LowerBackgroundMusic () {

		backgroundMusic.volume = 0.1f;

		StartCoroutine (WaitThenFullSound());

	}

	IEnumerator WaitThenFullSound () {

		yield return new WaitForSeconds(5.0f);

		backgroundMusic.volume = 1.0f;
	}
}
