﻿using System.Collections;
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
        if ((exp.beginScreenSelect != 0) &&
            !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
        {
            uiController.spacebarContinue.alpha = 1f;
        }
        if (!exp.isdevmode)
        {
            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        }
        else
        {
            yield return StartCoroutine(exp.WaitForJitterAction());
        }
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
        if (exp.beginScreenSelect == -1)
        {
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
        if (exp.beginScreenSelect == -1)
        {
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
                if ((exp.beginScreenSelect != 0) &&
                 !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                {
                    uiController.spacebarContinue.alpha = 1f;
                }
                if (!exp.isdevmode)
                {
                    yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
                }
                else
                {
                    yield return StartCoroutine(exp.WaitForJitterAction());
                }

                uiController.spacebarContinue.alpha = 0f;
                uiController.preEncodingInstructions.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
            case "SecondPracticeLoop":
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.secondPracticeLoopInstructions.enabled = true;
                if ((exp.beginScreenSelect != 0) &&
                    !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                {
                    uiController.spacebarContinue.alpha = 1f;
                }
                if (!exp.isdevmode)
                {
                    yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
                }
                else
                {
                    yield return StartCoroutine(exp.WaitForJitterAction());
                }
                uiController.spacebarContinue.alpha = 0f;
                uiController.secondPracticeLoopInstructions.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
            case "PreWeather":
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.preWeatherCondition.enabled = true;
                if ((exp.beginScreenSelect != 0) &&
                    !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                {
                    uiController.spacebarContinue.alpha = 1f;
                }
                if (!exp.isdevmode)
                {
                    yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
                }
                else
                {
                    yield return StartCoroutine(exp.WaitForJitterAction());
                }
                uiController.spacebarContinue.alpha = 0f;
                uiController.preWeatherCondition.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
        }
        yield return null;
    }

    //these are used to ask subject to check with the testing supervisor
    public IEnumerator ShowIntermissionInstructions()
    {
        uiController.intermissionInstructionPanel.alpha = 1f;
        exp.trialLogTrack.LogIntermission(true);
        if (!exp.isdevmode)
        {
            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        }
        else
        {
            yield return StartCoroutine(exp.WaitForJitterAction());
        }
        uiController.intermissionInstructionPanel.alpha = 0f;
        exp.trialLogTrack.LogIntermission(false);
        yield return null;
    }


    public IEnumerator ShowSecondSessionWelcomeInstructions()
    {
        uiController.secondSessionIntroPanel.alpha = 1f;
        exp.trialLogTrack.LogInstructions(true);
        if (!exp.isdevmode)
        {
            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        }
        else
        {
            yield return StartCoroutine(exp.WaitForJitterAction());
        }
        uiController.secondSessionIntroPanel.alpha = 0f;
        exp.trialLogTrack.LogInstructions(false);
    }
    public IEnumerator ShowSecondEncodingInstructions()
    {
        uiController.secondEncodingInstructionPanel.alpha = 1f;
        exp.trialLogTrack.LogInstructions(true);
        if (!exp.isdevmode)
        {
            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        }
        else
        {
            yield return StartCoroutine(exp.WaitForJitterAction());
        }
        uiController.secondEncodingInstructionPanel.alpha = 0f;
        exp.trialLogTrack.LogInstructions(false);
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
