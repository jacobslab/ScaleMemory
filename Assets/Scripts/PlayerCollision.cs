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
			StartCoroutine ("CoinAnimation",col.gameObject);

		}
	}

	IEnumerator CoinAnimation(GameObject coin)
	{
		float timer = 0f;
		StartCoroutine(racingTimelocked.uiController.PulseCoinImage ());
		while (timer < 1f && coin!=null) {
			timer += Time.deltaTime;
			coin.transform.position = Vector3.Lerp (coin.transform.position, coin.transform.position + (coin.transform.right * 3f) + coin.transform.up * 3f, timer);
			yield return 0;
		}
		Destroy (coin);
		coin = null;
		racingTimelocked.IncreaseCoinCount ();
		yield return null;
	}
}
