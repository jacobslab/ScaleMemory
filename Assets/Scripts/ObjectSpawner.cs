using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
public class ObjectSpawner : MonoBehaviour
{
    private int loopCount = 0;
    public GameObject car;
    public Text objText;

    private float fixedPresentationTime = 2.25f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public IEnumerator MakeObjAppear(GameObject spawnedObj)
    {
        GameObject targetObj = null;
        spawnedObj.transform.position = car.transform.position + car.transform.forward * 5f;
        spawnedObj.transform.position = new Vector3(spawnedObj.transform.position.x, 5f, spawnedObj.transform.position.z);
        car.GetComponent<Rigidbody>().isKinematic = true;
        objText.enabled = true;
        string formattedName = spawnedObj.name.Split('(')[0];

        objText.text = formattedName;

        float jitteredTime = UnityEngine.Random.Range(0.1f, 0.8f);
        float presentationTime = fixedPresentationTime + jitteredTime;

        yield return new WaitForSeconds(presentationTime);
        /*
        while (!Input.GetKeyDown(KeyCode.W))
        {
            yield return 0;

        }
        */
        loopCount++;
        spawnedObj.GetComponent<VisibilityToggler>().TurnVisible(false);
        //Destroy(spawnedObj);
        objText.text = "";
        objText.enabled = false; 
        car.GetComponent<Rigidbody>().isKinematic = false;
        yield return null;

    }

    // Update is called once per frame
    void Update()
    {
      
        
    }
}
