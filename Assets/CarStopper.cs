using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarStopper : MonoBehaviour
{
    private bool activated = false;
    private bool retActivated = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Experiment.Instance.verbalRetrieval)
        {
            float distToPlayer = Vector3.Distance(transform.position, Experiment.Instance.player.transform.position);
            if (distToPlayer < 10f && !retActivated)
            {
                retActivated = true;
                UnityEngine.Debug.Log("ret activated for " + gameObject.name);
                StartCoroutine("PerformVerbalRetrieval");
            }

        }
        else
        {
            float distToPlayer = Vector3.Distance(transform.position, Experiment.Instance.player.transform.position);
            if (distToPlayer < 10f && !activated)
            {
                activated = true;
                StartCoroutine("ShowObject");
            }
        }
    }

    IEnumerator PerformVerbalRetrieval()
    {
        yield return StartCoroutine(Experiment.Instance.StartVerbalRetrieval(gameObject));
        yield return null;
    }

    IEnumerator ShowObject()
    {
        if (gameObject.GetComponent<VisibilityToggler>() != null)
            gameObject.GetComponent<VisibilityToggler>().TurnVisible(true);
        yield return StartCoroutine(Experiment.Instance.StopCarTemporarily());

        string objName = gameObject.name.Split('(')[0];

        Experiment.Instance.trialLogTrack.LogItemPresentation(objName);

        if (gameObject.GetComponent<VisibilityToggler>() != null)
            gameObject.GetComponent<VisibilityToggler>().TurnVisible(false);
        //activated = false;
        yield return null;
    }

}
