using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using System.Diagnostics;
public class TestScript : MonoBehaviour
{
    AssetBundleLoader loader;
    // Start is called before the first frame update
    void Start()
    {
        loader = gameObject.GetComponent<AssetBundleLoader>();
        StartCoroutine("Test");

    }


    IEnumerator Test()
    {
        yield return StartCoroutine(loader.LoadCamTransform());
        yield return null;


    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
