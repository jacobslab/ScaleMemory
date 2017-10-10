using UnityEngine;
using System.Collections;

public class TornadoVortex : MonoBehaviour {

	private GameObject PullOBJ;
	public float PullSpeed;

	public void OnTriggerStay(Collider coll)
	{
		if (coll.gameObject.tag == "Pullable") {
			PullOBJ = coll.gameObject;

			PullOBJ.transform.position = Vector3.MoveTowards (PullOBJ.transform.position, this.transform.position, PullSpeed * Time.deltaTime);
		}
	}

}
