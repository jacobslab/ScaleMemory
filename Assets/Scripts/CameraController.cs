using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject mainCam;
	public GameObject blackoutCam;
	public GameObject canvas;
	// Use this for initialization
	void Start () {
		DisableBlackout ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void EnableBlackout()
	{
		blackoutCam.SetActive (true);
//		canvas.SetActive (false);
	}

	public void DisableBlackout()
	{
		blackoutCam.SetActive (false);
//		canvas.SetActive (true);
	}
}
