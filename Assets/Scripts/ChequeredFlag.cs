using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChequeredFlag : MonoBehaviour {

	public static int lapsCompleted=0;
	private bool carLapped=false;
	// Use this for initialization

	void Awake()
	{
		StartCoroutine ("WaitForRaceToStart");

	}
	void Start () {
		
	}

	IEnumerator WaitForRaceToStart()
	{
		GetComponent<BoxCollider> ().enabled = false;
		yield return new WaitForSeconds (3f);
		GetComponent<BoxCollider> ().enabled = true;
		yield return null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator WaitForCarToLap()
	{
		while (!carLapped) {
			yield return 0;
		}
		carLapped = false;
		yield return null;
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player") {
			carLapped = true;
			lapsCompleted++;
			Debug.Log ("laps completed: " + lapsCompleted.ToString ());
		}
	}
}
