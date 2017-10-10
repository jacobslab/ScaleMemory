using UnityEngine;
using System.Collections;

public class PullObject : MonoBehaviour {

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Tornado") {
			StartCoroutine ("Death");
		}
	}
		
	public IEnumerator Death()
	{
		yield return new WaitForSeconds (3);
		Destroy (this.gameObject);
	}
}
