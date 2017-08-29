using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMover : MonoBehaviour {

	public Transform startTransform;
	public Transform endTransform;

	public float factor=1000f;

	private Transform trainTransform;
	// Use this for initialization
	void Start () {
		trainTransform = gameObject.transform;
		StartCoroutine ("TrainMove");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator TrainMove()
	{
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime /factor;
			transform.position = Vector3.Lerp (startTransform.position, endTransform.position, t);
			yield return 0;
		}
		yield return null;
	}
}
