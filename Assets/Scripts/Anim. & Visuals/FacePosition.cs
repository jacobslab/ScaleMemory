using UnityEngine;
using System.Collections;
//using System.Runtime.Remoting;

public class FacePosition : MonoBehaviour {
	
	public Transform TargetPositionTransform;
	public bool ShouldFacePlayer = false;
	public bool ShouldFlip180 = false; //text meshes have their forward direction 180 degrees flipped...
	public bool ShouldFaceOverheadCam = false; // to point both object and text to face overhead cam during spatial retrieval
	// Use this for initialization
	void Start () {
	if(gameObject.GetComponent<VisibilityToggler>()!=null)
		gameObject.GetComponent<VisibilityToggler>().TurnVisible(false);
	}
	
	// Update is called once per frame
	void Update () {
		if (!ShouldFacePlayer && ShouldFaceOverheadCam) {
			
			if(TargetPositionTransform != null){
				FaceThePosition (TargetPositionTransform,true);
			}
			else {
			//	Debug.Log("Face position transform is null! NAME: " + gameObject.name);
			}
		}
		else {
			FaceThePosition (Experiment.Instance.player.transform,false);
		}

		
	}

	
	public void FaceItemScreeningCam()
	{
		FaceThePosition(Experiment.Instance.itemScreeningCam.transform,false);;
		ShouldFacePlayer = false;
	}

	void FaceThePosition(Transform transformToFace,bool facingOverhead){
		Quaternion origRot = transform.rotation;
		transform.LookAt (transformToFace);
		float yRot = transform.rotation.eulerAngles.y;
		float xRot = transform.rotation.eulerAngles.x;
		//		Debug.Log ("facing");
		if (!facingOverhead)
			transform.rotation = Quaternion.Euler(origRot.eulerAngles.x, yRot, origRot.eulerAngles.z);
		else
			transform.rotation = Quaternion.Euler(-xRot, yRot, origRot.eulerAngles.z);
		if (ShouldFlip180) {
			transform.RotateAround(transform.position, Vector3.up, 180.0f);
		}
	}
}
