using UnityEngine;
using System.Collections;

public class Coconut : Breakable {

	public Rigidbody firstHalf;
	public Rigidbody secondHalf;
	float breakTime = 1.0f; //amount of time before breakable should be destroyed

	void Start(){
		firstHalf.constraints = RigidbodyConstraints.FreezeAll;
		secondHalf.constraints = RigidbodyConstraints.FreezeAll;
	}

	void Update(){

	}

	public override void Break(){
		transform.RotateAround (transform.position, Vector3.up, Random.Range (0.0f, 360.0f));

		firstHalf.constraints = RigidbodyConstraints.None;
		secondHalf.constraints = RigidbodyConstraints.None;

		firstHalf.AddExplosionForce (500.0f, firstHalf.transform.position, 50.0f);
		firstHalf.AddTorque (new Vector3 (-30.0f, 0.0f, 0.0f));
		secondHalf.AddExplosionForce (500.0f, firstHalf.transform.position, 50.0f);
		secondHalf.AddTorque (new Vector3 (30.0f, 0.0f, 0.0f));

		StartCoroutine (Die ());
	}

	IEnumerator Die(){
		yield return new WaitForSeconds(breakTime);
		Destroy (gameObject);
	}
}
