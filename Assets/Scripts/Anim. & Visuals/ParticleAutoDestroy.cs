using UnityEngine;
using System.Collections;

public class ParticleAutoDestroy : MonoBehaviour {

	bool canDestroy = false;
	public ParticleSystem myParticleSystem;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (myParticleSystem.isPlaying) {
			canDestroy = true;
		}
		else{
			if(canDestroy){
				Destroy(gameObject);
			}
		}
	}
}
