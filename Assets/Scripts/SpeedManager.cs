using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedManager : MonoBehaviour {

	public Rigidbody carBody;
	public float speed=10f;

	private float centerX = 204.37f;
	private float leftX = 201f;
	private float rightX = 209f;
	// Use this for initialization
	void Start () {
		
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			StartCoroutine(MoveLeft());
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			StartCoroutine(MoveRight ());
		}
	}

	IEnumerator MoveLeft()
	{
		float timer = 0f;
		float newX = carBody.transform.position.x - 4f;
		if (carBody.transform.position.x > leftX) {
			while (timer < 1f) {
				timer += Time.deltaTime;
				carBody.transform.position = Vector3.Lerp (carBody.transform.position, new Vector3 (newX, carBody.transform.position.y, carBody.transform.position.z), timer);
				yield return 0;
			}
		}

		yield return null;
		
	}

	IEnumerator MoveRight()
	{
		float timer = 0f;
		float newX = carBody.transform.position.x + 4f;
		if (carBody.transform.position.x < rightX) {
			while (timer < 1f) {
				timer += Time.deltaTime;
				carBody.transform.position = Vector3.Lerp (carBody.transform.position, new Vector3 (newX, carBody.transform.position.y, carBody.transform.position.z), timer);
				yield return 0;
			}
		}
		yield return null;
		
	}
	// Update is called once per frame
	void FixedUpdate () {
		carBody.velocity = Vector3.forward * speed;
	}
}
