using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsManager : MonoBehaviour
{
    public UIController uiController;
    Experiment exp { get { return Experiment.Instance; } }
    public IEnumerator ShowEncodingInstructions()
    {
        uiController.encodingPanel.alpha = 1f;
        uiController.spacebarContinue.alpha = 1f;
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        uiController.spacebarContinue.alpha = 0f;
        uiController.encodingPanel.alpha = 0f;
        yield return null;
    }

    public IEnumerator UpdateVerbalInstructions()
    {
        yield return StartCoroutine(ShowVerbalRetrievalInstructions(uiController.GetCurrentUIPage()));
        yield return null;
    }

    public IEnumerator ShowVerbalRetrievalInstructions(int pageID)
    {
        UnityEngine.Debug.Log("setting spatial instruction to page : " + pageID.ToString());
        switch (pageID)
        {
            //page one
            case 0:
                uiController.verbalInstructionA.enabled = true;
                uiController.verbalInstructionB.enabled = false;
                uiController.verbalRetrievalPanel.alpha = 1f;
                break;
            //    yield return StartCoroutine(WaitForActionButton());

            //page two
            case 1:
                uiController.verbalInstructionA.enabled = false;
                uiController.verbalInstructionB.enabled = true;
                break;
            // yield return StartCoroutine(WaitForActionButton());
            case 2:
                uiController.verbalRetrievalPanel.alpha = 0f;
                break;

                //    
        }

        yield return null;
    }

    public IEnumerator UpdateSpatialInstructions()
    {
        UnityEngine.Debug.Log("updating spatial instructions");
        yield return StartCoroutine(ShowRetrievalInstructions(uiController.GetCurrentUIPage()));
        yield return null;
    }

    public IEnumerator ShowRetrievalInstructions(int pageID)
    {
        UnityEngine.Debug.Log("setting spatial instruction to page : " + pageID.ToString());
        switch (pageID)
        {
            case 0:
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.preSpatialRetrieval.enabled = true;
                break;
            //  yield return StartCoroutine(WaitForActionButton());
            case 1:
                uiController.preSpatialRetrieval.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                uiController.preSpatialRetrieval.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;

                string itemName = exp.objController.ReturnStimuliDisplayText();
                uiController.itemReactivationText.text = itemName;
                uiController.itemReactivationPanel.alpha = 1f;
                break;
            // yield return new WaitForSeconds(2f);
            case 2:
                uiController.spatialInstructionA.enabled = true;
                uiController.spatialInstructionB.enabled = false;
                uiController.retrievalPanel.alpha = 1f;
                break;
            //yield return StartCoroutine(WaitForActionButton());
            case 3:
                uiController.itemReactivationPanel.alpha = 0f;
                uiController.spatialInstructionA.enabled = false;
                uiController.spatialInstructionB.enabled = true;
                break;
            // yield return StartCoroutine(WaitForActionButton());
            case 4:
                uiController.itemReactivationPanel.alpha = 0f;
                uiController.retrievalPanel.alpha = 0f;
                break;

        }


        yield return null;
    }

    public IEnumerator ShowPracticeInstructions(string instType)
    {
        switch (instType)
        {
            case "PreEncoding":
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.preEncodingInstructions.enabled = true;
                uiController.spacebarContinue.alpha = 1f;
                yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
                uiController.spacebarContinue.alpha = 0f;
                uiController.preEncodingInstructions.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
            case "SecondEncoding":
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.secondEncodingInstructions.enabled = true;
                uiController.spacebarContinue.alpha = 1f;
                yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
                uiController.spacebarContinue.alpha = 0f;
                uiController.secondEncodingInstructions.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
            case "PreWeather":
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.preWeatherCondition.enabled = true;
                uiController.spacebarContinue.alpha = 1f;
                yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
                uiController.spacebarContinue.alpha = 0f;
                uiController.preWeatherCondition.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
        }
        yield return null;
    }


    void ChangeUIPage(bool isForwards)
    {
        //we are moving one page forwards
        if (isForwards)
        {

        }
        //else we are moving one page backwards
        else
        {

        }
    }

}
