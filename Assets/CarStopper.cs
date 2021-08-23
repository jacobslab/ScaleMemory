using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarStopper : MonoBehaviour
{
    private bool activated = false;
    private bool retActivated = false;
    public GameObject stimulusObject;
    public GameObject positionIndicator;


    // Start is called before the first frame update
    void Start()
    {       
    }


    // Update is called once per frame
    void Update()
    {
        if (Experiment.Instance.verbalRetrieval)
        {
            /*
            float distToPlayer = Vector3.Distance(transform.position, Experiment.Instance.player.transform.position);
            if (distToPlayer < 10f && !retActivated)
            {
                retActivated = true;
                UnityEngine.Debug.Log("ret activated for " + gameObject.name);
                StartCoroutine("PerformVerbalRetrieval");
            }
            */

        }
        else
        {
            /*
            float distToPlayer = Vector3.Distance(transform.position, Experiment.Instance.player.transform.position);
            UnityEngine.Debug.Log("dist to player " + gameObject.name + " : " + distToPlayer.ToString());
            if (distToPlayer < 15f && !activated)
            {
                activated = true;
                Experiment.Instance.SetCarMovement(false);
                StartCoroutine("ShowObject");
            }
            */
        }
    }

    void ShowIndicator(bool shouldShow)
    {

        positionIndicator.GetComponent<MeshRenderer>().enabled=shouldShow;
    }

    IEnumerator PerformVerbalRetrieval()
    {
        ShowIndicator(true);
       yield return StartCoroutine(Experiment.Instance.StartVerbalRetrieval(stimulusObject));
        ShowIndicator(false);
        yield return null;
    }

    IEnumerator ShowObject()
    {


        ShowIndicator(true);
        UnityEngine.Debug.Log("stopping car temporarily to show object");
        yield return StartCoroutine(Experiment.Instance.PresentStimuli(stimulusObject));
        ShowIndicator(false);

        //activated = false;
        yield return null;
    }

    IEnumerator BeginItemCuedRetrieval()
    {
        yield return StartCoroutine(Experiment.Instance.ShowItemCuedReactivation(stimulusObject));
        yield return null;
    }


    IEnumerator BeginLocationCuedRetrieval()
    {
        ShowIndicator(true);
        yield return StartCoroutine(Experiment.Instance.ShowLocationCuedReactivation(stimulusObject));
        ShowIndicator(false);
        yield return null;
    }


    private void OnTriggerEnter(Collider col)
    {
        UnityEngine.Debug.Log(gameObject.name + " collided with " + col.gameObject.name);
        if (col.gameObject.tag == "StimulusCollisions")
        {
            UnityEngine.Debug.Log("collided with player");
            stimulusObject.GetComponent<StimulusObject>().ToggleCollisions(false); //disable collisions until the next lap
            activated = true;
            Experiment.Instance.SetCarMovement(false);
            //if it is retrieval, then cue appropriately
         
            if (Experiment.Instance.currentStage == Experiment.TaskStage.VerbalRetrieval)
            {
                UnityEngine.Debug.Log("beginning location cued due to collision");
                StartCoroutine("BeginLocationCuedRetrieval");

            }
            //else, if it's encoding, present stimulus
            else if(Experiment.Instance.currentStage == Experiment.TaskStage.Encoding)
            {
                StartCoroutine("ShowObject");
            }
        }
    }

}
