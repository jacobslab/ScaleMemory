using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour {

	public RacingTimelocked racingTimelocked;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Coin") {
			Destroy (col.gameObject);
			racingTimelocked.IncreaseCoinCount ();
		}
	}
}
