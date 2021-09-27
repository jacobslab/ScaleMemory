using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMover : MonoBehaviour {

	public Transform startTransform;
	public Transform endTransform;

	public float factor=1000f;
	public float t=0f;
	private Transform trainTransform;
	// Use this for initialization
	void Start () {
		trainTransform = gameObject.transform;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void ResetPlayer()
	{
		transform.position = startTransform.position;
	}

	public IEnumerator TrainMove()
	{
		t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime /factor;
			transform.position = Vector3.Lerp (startTransform.position, endTransform.position, t);
			yield return 0;
		}
		yield return null;
	}

	public IEnumerator WaitTillPlayerStopped()
	{
		while (t < 1f) {
			yield return 0;
		}
		Debug.Log ("player stopped");
		yield return null;
	}
}
