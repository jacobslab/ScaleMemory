using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public Material redMat;
    public Material yellowMat;
    public Material greenMat;

    public Material offMat;


    public enum TrafficLights
    {
        Red,
        Yellow,
        Green
    };

    private TrafficLights currentTrafficLight;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void ChangeTo(TrafficLights trafficLight)
    {
        Experiment.Instance.trialLogTrack.LogTrafficLightColor(trafficLight);
        switch(trafficLight)
        {
            case TrafficLights.Red:
          
              //  UnityEngine.Debug.Log("setting to RED");
                gameObject.GetComponent<MeshRenderer>().materials[4].EnableKeyword("_EMISSION");
                gameObject.GetComponent<MeshRenderer>().materials[3].DisableKeyword("_EMISSION");
                gameObject.GetComponent<MeshRenderer>().materials[2].DisableKeyword("_EMISSION");
                break;

            case TrafficLights.Yellow:
               // UnityEngine.Debug.Log("setting to YELLOW");
                gameObject.GetComponent<MeshRenderer>().materials[4].DisableKeyword("_EMISSION");
                gameObject.GetComponent<MeshRenderer>().materials[3].EnableKeyword("_EMISSION");
                gameObject.GetComponent<MeshRenderer>().materials[2].DisableKeyword("_EMISSION");
                break;

            case TrafficLights.Green:
               // UnityEngine.Debug.Log("setting to GREEN");
                gameObject.GetComponent<MeshRenderer>().materials[4].DisableKeyword("_EMISSION");
                gameObject.GetComponent<MeshRenderer>().materials[3].DisableKeyword("_EMISSION");
                gameObject.GetComponent<MeshRenderer>().materials[2].EnableKeyword("_EMISSION");
                break;

        }
    }

    public IEnumerator StartCountdownToGreen()
    {
        ChangeTo(TrafficLights.Red);
        yield return new WaitForSeconds(1f);
        ChangeTo(TrafficLights.Yellow);
        yield return new WaitForSeconds(1f);
        ChangeTo(TrafficLights.Green);
        yield return new WaitForSeconds(0.3f);
        yield return null;
    }

    public void MakeVisible(bool isVisible)
    {
        gameObject.GetComponent<MeshRenderer>().enabled = isVisible;
        Experiment.Instance.trialLogTrack.LogTrafficLightVisibility(isVisible);
    }

    public IEnumerator ShowRed()
    {
        ChangeTo(TrafficLights.Red);
        yield return null;
    }
}
