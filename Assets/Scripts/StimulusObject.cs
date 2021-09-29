using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class StimulusObject : MonoBehaviour
{
    private GameObject collidingPart;
    public string stimuliDisplayName;
    public Texture stimuliDisplayTexture;
    public AudioSource specialCollisionSound;

    public TextMesh specialObjectText;
	public Transform specialObjectSpawnPoint;

    public AudioClip magicWand;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayJuice(bool isSpecial)
    {
        //play particle effect here

        UnityEngine.Debug.Log("playing audio of treasure chest");
        specialCollisionSound.PlayOneShot(magicWand);
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

    public void SetSpecialObjectText(string text)
    {
        specialObjectText.text = text;
        Experiment.Instance.trialLogTrack.LogTreasureLabel(specialObjectText.text);
    }
}
