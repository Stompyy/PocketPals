﻿using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchHandler : MonoBehaviour {
	
	public GameObject player, buttonBlocker;
	public CameraController cameraController;
	public CaptureMiniGame miniGame;
	public VirtualSceneParent virtualGarden;

    public float ResourceSpotDistance = 30.0f;
    public float ResourceSpotUpness = 0.5f;

    // The state machine for the controls
    enum ControlScheme {
		map,
		mapCameraTransition,
		menu,
		miniGame,
		VirtualGarden,
		VirtualGardenCameraTransition,
		VirtualGardenInfo,
		VirtualGardenInfoTransition,
		ResourceSpotControls,
		ResourceSpotTransition,
		CharCust
	}

	ControlScheme controlScheme;

	public bool IsDebug = false;

	// The max distance allowed by the raycast hit detection
	public float maxCaptureDistance = 15.0f;

	// Used to determine swipe length in VirtualGarden controlScheme
	Vector2 startTouchPosition;
	bool virtualGardenTouchPlaced = false;

	// Portion of screen width needed to swipe to look at next inventory in virtual garden
	float swipeLengthToLookAtNext = 0.2f;

	public static TouchHandler Instance { get; set; }



	// Use this for initialization
	void Start () {

		Instance = this;

		// Initialise as the menu controls
		controlScheme = ControlScheme.map;//menu;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Used to stop the map updating when we dont want it to.
        CheckForMapUpdate();

		// Choose how to parse the touch controls based on the current control scheme
		switch (controlScheme) {

		case ControlScheme.map:
			{
                UseMapControls ();
             }
			break;

		case ControlScheme.mapCameraTransition:
			{
				cameraController.UpdateCaptureCam();
			}
			break;

		case ControlScheme.miniGame:
			{
				// Move the PPal between patrol points
				miniGame.MovePPal ();

				miniGame.UpdateTimer ();

				// Use the input to update the controls and timer
				if (IsDebug && Input.GetMouseButtonDown (0)) {
					miniGame.UpdateControls (Input.mousePosition);

				} else if (Input.touches.Length > 0) {

					// Look at each touch
					for (int i = 0; i < Input.touchCount; i++) {
						
						// If touch has just begun
						if (Input.GetTouch (i).phase.Equals (TouchPhase.Began)) {

							miniGame.SetTouchStartPosition (Input.GetTouch (0).position);
							break;

						} else if (Input.GetTouch (i).phase.Equals (TouchPhase.Moved)) {

							miniGame.UpdateControls (Input.GetTouch (0).position);
							break;
						}
					}
				}
			}
			break;

		case ControlScheme.menu:
			{

			}
			break;

		case ControlScheme.VirtualGarden:
			{
				UseVirtualGardenControls ();
				CheckForVGTap ();
			}
			break;

		case ControlScheme.VirtualGardenCameraTransition:
			{
				cameraController.VGMoveCamToNextPPal ();
			}
			break;

		case ControlScheme.VirtualGardenInfo:
			{
				CheckForVGTap ();
				UseVGInfoControls ();
			}
			break;

		case ControlScheme.VirtualGardenInfoTransition:
			{
				cameraController.VGUpdateInfoCam ();
			}
			break;

		case ControlScheme.ResourceSpotControls:
			{

			}
			break;

		case ControlScheme.ResourceSpotTransition:
			{
				cameraController.UpdateZoomCam ();
			}
			break;

		case ControlScheme.CharCust:
			{
				if (Input.touches.Length > 0) {
					cameraController.MenuCharRotate (Input.GetTouch (0));
				}

				cameraController.MenuCCMoveToTargetLocation ();
			}
			break;
		}
	}

	// Setters for all controlSchemes
	public void MapControls() {
		controlScheme = ControlScheme.map;
		miniGame.animManager.ResetMinigame ();
		cameraController.EnableJournalButton ();
		EnableButtons ();
    }
	
	public void MapCameraTransition() {
		controlScheme = ControlScheme.mapCameraTransition;
		cameraController.DisableJournalButton ();
		DisableButtons ();
	}

	public void MenuControls() {
		controlScheme = ControlScheme.menu;
		cameraController.DisableJournalButton ();
		EnableButtons ();
	}

	public void MiniGameControls() {
		controlScheme = ControlScheme.miniGame;
		cameraController.DisableJournalButton ();
		EnableButtons ();
	}

	public void VirtualGardenControls() {
		
		// Set a new start position for swipe controls to stop continuous swiping
		if (IsDebug && Input.GetMouseButtonDown (0))
			startTouchPosition = Input.mousePosition;
		else if (Input.touches.Length > 0)
			startTouchPosition = Input.GetTouch (0).position;
		
		controlScheme = ControlScheme.VirtualGarden;
		cameraController.DisableJournalButton ();
		EnableButtons ();
	}

	public void VirtualGardenCameraTransitionControls() {
		controlScheme = ControlScheme.VirtualGardenCameraTransition;
		cameraController.DisableJournalButton ();
		EnableButtons ();
	}

	public void VirtualGardenInfoControls() {
		controlScheme = ControlScheme.VirtualGardenInfo;
		cameraController.DisableJournalButton ();
		EnableButtons ();
	}

	public void VirtualGardenInfoTransitionControls () {
		controlScheme = ControlScheme.VirtualGardenInfoTransition;
		cameraController.DisableJournalButton ();
		EnableButtons ();
	}

	public void ResourceSpotControls() {
		controlScheme = ControlScheme.ResourceSpotControls;
		cameraController.DisableJournalButton ();
		DisableButtons ();
	}

	public void ResourceSpotTransition() {
		controlScheme = ControlScheme.ResourceSpotTransition;
		cameraController.DisableJournalButton ();
		DisableButtons ();
	}

	public void CharCustControls () {
		controlScheme = ControlScheme.CharCust;
		cameraController.DisableJournalButton ();
		EnableButtons ();
	}

	public void InitVirtualGardenControls() {
		virtualGardenTouchPlaced = false;
	}

	public void SetStartTouchPosition (Vector2 position) {
		startTouchPosition = position;
	}

	public bool IsInMapControls () {
		if (controlScheme == ControlScheme.map) return true;
		return false;
	}

    public void CheckForMapUpdate()
    {
        if (GPS.Insatance.mapGameObject == null) return;
        if (GPS.Insatance.mapGameObject.GetComponent<CameraBoundsTileProvider>() == null) return;
		if (controlScheme == ControlScheme.map || controlScheme == ControlScheme.menu)
        {
            GPS.Insatance.mapGameObject.GetComponent<CameraBoundsTileProvider>().ShouldUpdate = true;
        }
        else
        {
            GPS.Insatance.mapGameObject.GetComponent<CameraBoundsTileProvider>().ShouldUpdate = false;
        }
    }

	public bool CameraShouldFollowGPS() {
        if (controlScheme == ControlScheme.map || controlScheme == ControlScheme.menu)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

	void DebugTouch()
	{
        //declare a variable of RaycastHit struct
        RaycastHit hit;
		//Create a Ray on the tapped / clicked position
		Ray ray;
		//for unity editor
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		//for touch device

		//Check if the ray hits any collider
		if (Physics.Raycast(ray, out hit))
		{
            if (hit.transform.gameObject.GetComponentInParent<PocketPalParent>() && (hit.transform.position - player.transform.position).magnitude < maxCaptureDistance)
            {

                // Initialise the capture cam values
                //cameraController.CaptureCamInit(hit.transform.parent.gameObject);
                // Initialise the capture cam values
                PocketPalSpawnManager.Instance.PocketpalCollected(hit.transform.parent.gameObject);

                PocketPalParent hitPocketPal = hit.transform.gameObject.GetComponentInParent<PocketPalParent>();

                // Only need to find one, Don't bother checking other touches after this
                return;
            }
            else if (hit.transform.gameObject.GetComponent<ResourceSpotParent>() && (hit.transform.position - player.transform.position).magnitude < maxCaptureDistance)
            {
                TryResourceSpotSequence(hit.transform.gameObject);

            }
            else if (IsDebug)
			{
				player.GetComponent<GPS>().SetIsDebug(true);
				player.GetComponent<GPS>().FakeGPSRead(hit.transform.position);
			}

		}
	}

    private void TryResourceSpotSequence(GameObject gd)
    {
        GPS.Insatance.mapGameObject.GetComponent<CameraBoundsTileProvider>().ShouldUpdate = false;

        ResourceSpotParent rsp = gd.GetComponent<ResourceSpotParent>();
        if (rsp.IsUsed())
        {
            float timeleft = ResourceSpotManager.Instance.GetTimeOfCoolDown(rsp);
            if (timeleft > 0)
            {
                NotificationManager.Instance.ResourceSpotUsed(timeleft);
                return;
            }
            
        }
        //set the sign to be facing the player
        Quaternion targetRotation = Quaternion.LookRotation(GPS.Insatance.girl.transform.position - gd.transform.position);
        gd.transform.rotation = targetRotation;

        cameraController.ZoomInCamInit(rsp.gameObject, ResourceSpotUpness, ResourceSpotDistance);
    }

	public void CheckForMapTap() {

		// Create a rayCastHit object
		RaycastHit hit = new RaycastHit ();

		// Look at each touch
		for (int i = 0; i < Input.touchCount; i++)
		{
			// If touch has just begun
			if (Input.GetTouch (i).phase.Equals (TouchPhase.Began))
			{

                // Raycast from the touch position
                Ray ray = Camera.main.ScreenPointToRay (Input.GetTouch (i).position);

				// if hit
				if (Physics.Raycast (ray, out hit))
				{
                    bool tooFar = (hit.transform.position - player.transform.position).magnitude > maxCaptureDistance;
                    // If the hit gameObject has a component "PocketPalParent" and is within the capture distance from the player
                    if (hit.transform.gameObject.GetComponentInParent<PocketPalParent>())
                    {
                        if (tooFar)
                        {
                            NotificationManager.Instance.CustomHeaderNotification("Too far away!", "Get close for a chance to observe it!");
                            return;
                        }

                        // Initialise the capture cam values
                        cameraController.CaptureCamInit(hit.transform.parent.gameObject);

                        PocketPalParent hitPocketPal = hit.transform.gameObject.GetComponentInParent<PocketPalParent>();

                        // Only need to find one, Don't bother checking other touches after this
                        return;
                    }
                    else if (hit.transform.gameObject.GetComponent<ResourceSpotParent>())
                    {
                        if (tooFar)
                        {
                            if ((hit.transform.position - player.transform.position).magnitude > maxCaptureDistance * 4) return;
                            NotificationManager.Instance.CustomHeaderNotification("Too far away!", "Get closer to claim a prize!");
                            return;
                        }
                        TryResourceSpotSequence(hit.transform.gameObject);
                    }
                }
			}
		}
	}

	void UseMapControls() {

        if (IsDebug && Input.GetMouseButtonDown(0))
        {
            DebugTouch();
            return;
        }

		// Switch statement behaves differently with different number of touches
		switch (Input.touchCount) { 

		// Check for pinch to zoom control. The only control to use > two touches. Uses fallthrough cases
		case 4:
		case 3:
		case 2:
			cameraController.MapPinchZoom (Input.GetTouch(0), Input.GetTouch(1));
			break;

			// check for a single touch swiping
		case 1:
			cameraController.MapRotate (Input.GetTouch(0));
			CheckForMapTap ();
			break;
		}
	}

	void CheckForVGTap () {
		
		// Create a rayCastHit object
		RaycastHit hit = new RaycastHit ();

		// Look at each touch
		for (int i = 0; i < Input.touchCount; i++)
		{
			// Store the touch
			Touch touch = Input.GetTouch (i);

			// If touch has just begun
			if (touch.phase.Equals (TouchPhase.Began))
			{
				// Don't continue if user is clicking a button
				if (!TouchIsOverUIObject (touch)) {
					
					// Raycast from the touch position
					Ray ray = Camera.main.ScreenPointToRay (touch.position);

					// if hit
					if (Physics.Raycast (ray, out hit)) {
						// This might not be a good way. Need a cast to whatever data structure will be used here
						// Either PocketPalParent, or look for the int ID component of the VirtualGardenSpawn maybe
						if (hit.transform.gameObject.GetComponent ("VirtualGardenInfo")) {
							cameraController.VGToggleInspect ();

							// Only need to find one, Don't bother checking other touches after this
							return;
						}
					}
				}
			}
		}
	}

	private bool TouchIsOverUIObject(Touch touch)
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);

		eventDataCurrentPosition.position = new Vector2 (touch.position.x, touch.position.y);

		List<RaycastResult> results = new List<RaycastResult>();

		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

		return results.Count > 0;
	}

	void UseVirtualGardenControls() {
		
		if(Input.GetMouseButtonDown(0))Debug.Log("Cannot rotate camera using mouse button try touches,");

		int numberOfTouches = Input.touches.Length;

		if (numberOfTouches > 1) {

			// Disable the swipe control if pinch zooming
			virtualGardenTouchPlaced = false;

			// Pinch zoom for the virtual garden
			cameraController.VGPinchZoom (Input.GetTouch(0), Input.GetTouch(1));

		} else if (numberOfTouches == 1) {

			Touch touchZero = Input.GetTouch (0);

			// Controls begin after touch(0) is placed when in virtual garden. Previous touches are ignored
			if (touchZero.phase == TouchPhase.Began) {

				// Update the start touch position
				startTouchPosition = touchZero.position;

				// Virtual garden controls are not reachable until this is true. Ignores previous pre VG touches
				virtualGardenTouchPlaced = true;

			// if the last present touch has just ended. No touches present after this
			} else if (touchZero.phase == TouchPhase.Ended) {

				// Init the camera returning to a default direction when no touches present
				cameraController.VGInitReturn ();

			} else if (virtualGardenTouchPlaced) {

				// If the delta touch position.x is above the threshold needed to look at next PPal
				if (touchZero.position.x - startTouchPosition.x > swipeLengthToLookAtNext * Screen.width) {

					GameObject nextPPal = virtualGarden.GetPreviousPPal ();

					if (nextPPal != null) {
						// Init lerp values to look at the previous PPal
						cameraController.VGInitLookAtNextPPal (nextPPal);
					} else {
						// If no PPals in inventory just rotate
						cameraController.VGRotateCamera (touchZero);
					}

				} else if (touchZero.position.x - startTouchPosition.x < -swipeLengthToLookAtNext * Screen.width) {

					GameObject nextPPal = virtualGarden.GetNextPPal ();

					if (nextPPal != null) {
						// Init lerp values to look at the previous PPal
						cameraController.VGInitLookAtNextPPal (nextPPal);
					} else {
						// If no PPals in inventory just rotate
						cameraController.VGRotateCamera (touchZero);
					}

				} else {

					// Allow a rotation movement when not switching between inventory animals, add in small up and down?
					cameraController.VGRotateCamera (touchZero);
				}
			}
		} else {

			// With no touch present return the orientation of the camera to look directly at the targeted inventory PPal
			cameraController.VGReturnCamToTargetedPPal ();
		}
	}

	void UseVGInfoControls () {
		
		if (Input.touches.Length > 0) {

			Touch touchZero = Input.GetTouch (0);

			// Controls begin after touch(0) is placed when in virtual garden. Previous touches are ignored
			if (touchZero.phase == TouchPhase.Began) {

				// Update the start touch position
				startTouchPosition = touchZero.position;

				// Virtual garden controls are not reachable until this is true. Ignores previous pre VG touches
				virtualGardenTouchPlaced = true;

			// if the last present touch has just ended. No touches present after this
			} else if (virtualGardenTouchPlaced) {

				// If the delta touch position.x is above the threshold needed to look at next PPal
				if (touchZero.position.x - startTouchPosition.x > swipeLengthToLookAtNext * Screen.width) {

					// cameraController move to previous and swap ui
					cameraController.VGInspectPrevPPal ();

				} else if (touchZero.position.x - startTouchPosition.x < -swipeLengthToLookAtNext * Screen.width) {

					// cameraController move to next and swap ui
					cameraController.VGInspectNextPPal ();
				}
			}
		}
	}

	public void Vibrate () {
		Handheld.Vibrate ();
	}

	public void DisableButtons () {
		buttonBlocker.SetActive (true);
	}

	public void EnableButtons () {
		buttonBlocker.SetActive (false);
	}

	public void UpdateCurrentVirtualGarden (VirtualSceneParent _vsc) {
		virtualGarden = _vsc;
	}
}
