using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_particale : MonoBehaviour {
    public Vector3 offset;
    public Transform target;
    public Transform movetransform;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        target.position = movetransform.position + offset;	
	}
}
