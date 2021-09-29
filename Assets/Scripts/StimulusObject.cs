using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class StimulusObject : MonoBehaviour
{
    private GameObject collidingPart;
    public string stimuliDisplayName;
    public Texture stimuliDisplayTexture;
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
        return stimuliDisplayName;
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
