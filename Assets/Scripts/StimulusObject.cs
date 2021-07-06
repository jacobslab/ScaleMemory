using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimulusObject : MonoBehaviour
{
    private GameObject collidingPart;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetObjectName()
    {
        return gameObject.name.Split('(')[0];
    }

    public void ToggleCollisions(bool shouldCollide)
    {
        collidingPart.GetComponent<BoxCollider>().enabled = shouldCollide;
    }

    public void LinkColliderObj(GameObject colliderObj)
    {
        collidingPart = colliderObj;
        colliderObj.GetComponent<CarStopper>().stimulusObject = this.gameObject;
    }
}
