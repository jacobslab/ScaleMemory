using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PrintLists : MonoBehaviour {


	private Object[] germanAudio;
	// Use this for initialization
	void Start () {



		germanAudio = Resources.LoadAll ("Prefabs/Objects");

		string contents = "";
		for (int i = 0; i < germanAudio.Length; i++) {
            GameObject tempObj = (GameObject)germanAudio[i];
            UnityEngine.Debug.Log("adding " + tempObj.gameObject.name);
            contents += tempObj.gameObject.name + "\n";
		}
		System.IO.File.WriteAllText (Application.dataPath+"/list.txt", contents);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
