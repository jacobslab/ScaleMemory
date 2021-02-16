using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class testscript : MonoBehaviour
{
  //  public MTurk mTurkController;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
          //  mTurkController.Response();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
          //  mTurkController.GetAssignmentFunc();
        }
    }
}
