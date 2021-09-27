using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine (InitBarriers ());
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator InitBarriers()
	{
		int chosenBarrier = Random.Range (0, transform.childCount);
		transform.GetChild (chosenBarrier).GetComponent<BoxCollider> ().isTrigger = true;
		yield return null;
	}

}
