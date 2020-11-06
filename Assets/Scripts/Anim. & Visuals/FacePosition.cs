using UnityEngine;
using System.Collections;
//using System.Runtime.Remoting;

public class FacePosition : MonoBehaviour {
	
	public Transform TargetPositionTransform;
	public bool ShouldFacePlayer = false;
	public bool ShouldFlip180 = false; //text meshes have their forward direction 180 degrees flipped...

	// Use this for initialization
	void Start () {
	//	if(gameObject.componen)
	if(gameObject.GetComponent<VisibilityToggler>()!=null)
		gameObject.GetComponent<VisibilityToggler>().TurnVisible(false);
	}
	
	// Update is called once per frame
	void Update () {
		if (!ShouldFacePlayer) {
			if(TargetPositionTransform != null){
				FaceThePosition (TargetPositionTransform);
			}
			else {
			//	Debug.Log("Face position transform is null! NAME: " + gameObject.name);
			}
		}
		else {
			FaceThePosition (Experiment.Instance.player.transform);
		}

		
	}

	
	public void FaceItemScreeningCam()
	{
		FaceThePosition(Experiment.Instance.itemScreeningCam.transform);
		ShouldFacePlayer = false;
	}

	void FaceThePosition(Transform transformToFace){
		Quaternion origRot = transform.rotation;
		transform.LookAt (transformToFace);
		float yRot = transform.rotation.eulerAngles.y;
//		Debug.Log ("facing");
		transform.rotation = Quaternion.Euler (origRot.eulerAngles.x, yRot, origRot.eulerAngles.z);

		if (ShouldFlip180) {
			transform.RotateAround(transform.position, Vector3.up, 180.0f);
		}
	}
}
