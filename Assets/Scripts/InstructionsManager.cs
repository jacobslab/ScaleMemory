using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class InstructionsManager : MonoBehaviour
{
    public UIController uiController;
    Experiment exp { get { return Experiment.Instance; } }
    //IMG2Sprite img2sprite { get { return IMG2Sprite.instance; } }
    public IEnumerator ShowEncodingInstructions()
    {
        //Running only for Practice
        if (exp.isdevmode)
        {
            uiController.encodingPanel.alpha = 1f;
            yield return StartCoroutine(exp.WaitForJitterAction());
            uiController.encodingPanel.alpha = 0f;
        }
        else if (exp.beginPracticeSelect == 1)
        {
            //uiController.ECOGencodingPanel.alpha = 1f;
            string path = Application.dataPath + "/Resources_IGNORE/D1/01.png";
            string[] Images = Directory.GetFiles(Application.dataPath + "/Resources_IGNORE/D1", "*.png", SearchOption.AllDirectories);
            foreach (string path_n in Images) {
                Debug.Log("InstManager: Path_n: " + path_n);
                Sprite image_new = exp.img2sprite.LoadNewSprite(path_n);
                uiController.instructionRendererImage.sprite = image_new;
                uiController.instructionRenderer.alpha = 1f;

                yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

                uiController.instructionRenderer.alpha = 0f;
            }
            Debug.Log("InstManager: Path: " + path);


        }
        else if (exp.beginPracticeSelect == 0)
        {
            uiController.encodingPanel.alpha = 1f;
            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
            uiController.encodingPanel.alpha = 0f;
        }

        yield return null;
    }

    public IEnumerator ShowPreEncodingInstructions()
    {
        //Running only for Practice
        string[] Images = Directory.GetFiles(Application.dataPath + "/Resources_IGNORE/D2", "*.png", SearchOption.AllDirectories);
        foreach (string path_n in Images)
            {
                //Debug.Log("InstManager: Path_n: " + path_n);
                Sprite image_new = exp.img2sprite.LoadNewSprite(path_n);
                uiController.instructionRendererImage.sprite = image_new;
                uiController.instructionRenderer.alpha = 1f;

                yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

                uiController.instructionRenderer.alpha = 0f;
            }

        yield return null;
    }

    public IEnumerator ShowSpatialInstructions()
    {
        //Running only for Practice
        string[] Images = Directory.GetFiles(Application.dataPath + "/Resources_IGNORE/D3", "*.png", SearchOption.AllDirectories);
        foreach (string path_n in Images)
        {
            //Debug.Log("InstManager: Path_n: " + path_n);
            Sprite image_new = exp.img2sprite.LoadNewSprite(path_n);
            uiController.instructionRendererImage.sprite = image_new;
            uiController.instructionRenderer.alpha = 1f;

            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

            uiController.instructionRenderer.alpha = 0f;
        }

        yield return null;
    }

    public IEnumerator ShowRemFamSpatialInstructions()
    {
        //Running only for Practice
        string[] Images = Directory.GetFiles(Application.dataPath + "/Resources_IGNORE/D4", "*.png", SearchOption.AllDirectories);
        foreach (string path_n in Images)
        {
            //Debug.Log("InstManager: Path_n: " + path_n);
            Sprite image_new = exp.img2sprite.LoadNewSprite(path_n);
            uiController.instructionRendererImage.sprite = image_new;
            uiController.instructionRenderer.alpha = 1f;

            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

            uiController.instructionRenderer.alpha = 0f;
        }

        yield return null;
    }

    public IEnumerator ShowVerbalInstructions()
    {
        //Running only for Practice
        string[] Images = Directory.GetFiles(Application.dataPath + "/Resources_IGNORE/D5", "*.png", SearchOption.AllDirectories);
        foreach (string path_n in Images)
        {
            //Debug.Log("InstManager: Path_n: " + path_n);
            Sprite image_new = exp.img2sprite.LoadNewSprite(path_n);
            uiController.instructionRendererImage.sprite = image_new;
            uiController.instructionRenderer.alpha = 1f;

            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

            uiController.instructionRenderer.alpha = 0f;
        }

        yield return null;
    }

    public IEnumerator ShowVerbalVoiceInstructions()
    {
        //Running only for Practice
        string[] Images = Directory.GetFiles(Application.dataPath + "/Resources_IGNORE/D6", "*.png", SearchOption.AllDirectories);
        foreach (string path_n in Images)
        {
            //Debug.Log("InstManager: Path_n: " + path_n);
            Sprite image_new = exp.img2sprite.LoadNewSprite(path_n);
            uiController.instructionRendererImage.sprite = image_new;
            uiController.instructionRenderer.alpha = 1f;

            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

            uiController.instructionRenderer.alpha = 0f;
        }

        yield return null;
    }

    public IEnumerator ShowThirdTrialInstructions()
    {
        //Running only for Practice
        string[] Images = Directory.GetFiles(Application.dataPath + "/Resources_IGNORE/D7", "*.png", SearchOption.AllDirectories);
        foreach (string path_n in Images)
        {
            //Debug.Log("InstManager: Path_n: " + path_n);
            Sprite image_new = exp.img2sprite.LoadNewSprite(path_n);
            uiController.instructionRendererImage.sprite = image_new;
            uiController.instructionRenderer.alpha = 1f;

            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

            uiController.instructionRenderer.alpha = 0f;
        }

        yield return null;
    }

    public IEnumerator ShowDistractorInstructions()
    {
        //Running only for Practice
        string[] Images = Directory.GetFiles(Application.dataPath + "/Resources_IGNORE/D8", "*.png", SearchOption.AllDirectories);
        foreach (string path_n in Images)
        {
            //Debug.Log("InstManager: Path_n: " + path_n);
            Sprite image_new = exp.img2sprite.LoadNewSprite(path_n);
            uiController.instructionRendererImage.sprite = image_new;
            uiController.instructionRenderer.alpha = 1f;

            yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

            uiController.instructionRenderer.alpha = 0f;
        }

        yield return null;
    }

    public IEnumerator ShowEncodingInstructions1()
    {
        //Running only for Practice

        if (exp.beginPracticeSelect == 1)
        {
            uiController.ECOGencodingPanel1.alpha = 1f;
        }
        else if (exp.beginPracticeSelect == 0)
        {
            uiController.encodingPanel1.alpha = 1f;
        }

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
        if (exp.beginPracticeSelect == 1)
        {
            uiController.ECOGencodingPanel1.alpha = 0f;
        }
        else if (exp.beginPracticeSelect == 0)
        {
            uiController.encodingPanel1.alpha = 0f;
        }
        yield return null;
    }

    public IEnumerator BeforeLoopTest()
    {
        
        uiController.BeforeLoopTest.alpha = 1f;

        if ((exp.beginScreenSelect != 0) &&
            !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
        {
            uiController.spacebarContinue.alpha = 0f;
        }
        if (!exp.isdevmode)
        {
            yield return StartCoroutine(exp.WaitForJitter(4));
            /*if (exp.beginScreenSelect == 0)
                yield return StartCoroutine(exp.WaitForJitter(4));
            else
                yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
            */
        }
        else
        {
            yield return StartCoroutine(exp.WaitForJitter(4));
        }
        uiController.spacebarContinue.alpha = 0f;
        uiController.BeforeLoopTest.alpha = 0f;

        yield return null;
    }

    public IEnumerator UpdateVerbalInstructions()
    {
        yield return StartCoroutine(ShowVerbalRetrievalInstructions(uiController.GetCurrentUIPage()));
        yield return null;
    }

    public IEnumerator UpdateLoop2PageInstructions()
    {
        yield return StartCoroutine(ShowLoop2PageInstructionsAcc(uiController.GetCurrentUILoop2Page()));
        yield return null;
    }


    public IEnumerator ShowVerbalRetrievalInstructions(int pageID)
    {
        UnityEngine.Debug.Log("setting spatial instruction to page : " + pageID.ToString());
        if (exp.beginScreenSelect == -1)
        {
            if ((exp.beginScreenSelect != 0) &&
                !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
            {
                uiController.spacebarContinue.alpha = 0f;
                uiController.selectionControls.alpha = 1f;
                uiController.selectControlsText.text = "Previous/Next Page";
            }
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
                    uiController.spacebarContinue.alpha = 0f;
                    uiController.selectionControls.alpha = 0f;
                    uiController.selectControlsText.text = "Left/Right";
                    uiController.verbalRetrievalPanel.alpha = 0f;
                    break;

                    //    
            }
        }

        yield return null;
    }

    public IEnumerator ShowLoop2PageInstructionsAcc(int pageID)
    {
        UnityEngine.Debug.Log("setting Loop2page instruction to page : " + pageID.ToString());
        if (exp.beginScreenSelect == -1)
        {
            if ((exp.beginScreenSelect != 0) &&
                !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
            {
                uiController.spacebarContinue.alpha = 0f;
                uiController.selectionControls.alpha = 1f;
                uiController.selectControlsText.text = "Previous/Next Page";
            }
            else {
                uiController.selectionControls.alpha = 1f;
                uiController.selectControlsText.enabled = false;
                uiController.selectControlsImage.enabled = false;
            }
            switch (pageID)
            {
                //page one
                case 0:
                    uiController.Loop2Image1.alpha = 1f;
                    uiController.Loop2Image2.alpha = 0f;
                    break;
                case 1:
                    uiController.Loop2Image1.alpha = 0f;
                    uiController.Loop2Image2.alpha = 1f;
                    uiController.Loop2Image3.alpha = 0f;
                    break;
                case 2:
                    uiController.Loop2Image2.alpha = 0f;
                    uiController.Loop2Image3.alpha = 1f;
                    uiController.Loop2Image4.alpha = 0f;
                    break;
                case 3:
                    uiController.Loop2Image3.alpha = 0f;
                    uiController.Loop2Image4.alpha = 1f;
                    uiController.Loop2Image5.alpha = 0f;
                    break;
                case 4:
                    uiController.Loop2Image4.alpha = 0f;
                    uiController.Loop2Image5.alpha = 1f;
                    uiController.Loop2Image6.alpha = 0f;
                    break;
                case 5:
                    uiController.Loop2Image5.alpha = 0f;
                    uiController.Loop2Image6.alpha = 1f;
                    uiController.Loop2Image7.alpha = 0f;
                    break;
                case 6:
                    uiController.Loop2Image6.alpha = 0f;
                    uiController.Loop2Image7.alpha = 1f;
                    uiController.Loop2Image8.alpha = 0f;
                    break;
                case 7:
                    uiController.Loop2Image7.alpha = 0f;
                    uiController.Loop2Image8.alpha = 1f;
                    uiController.Loop2Image9.alpha = 0f;
                    break;
                case 8:
                    uiController.Loop2Image8.alpha = 0f;
                    uiController.Loop2Image9.alpha = 1f;
                    uiController.Loop2Image10.alpha = 0f;
                    break;
                case 9:
                    uiController.Loop2Image9.alpha = 0f;
                    uiController.Loop2Image10.alpha = 1f;
                    uiController.Loop2Image11.alpha = 0f;
                    break;
                case 10:
                    uiController.Loop2Image10.alpha = 0f;
                    uiController.Loop2Image11.alpha = 1f;
                    uiController.Loop2Image12.alpha = 0f;
                    break;
                case 11:
                    uiController.Loop2Image11.alpha = 0f;
                    uiController.Loop2Image12.alpha = 1f;
                    break;
                case 12:
                    uiController.spacebarContinue.alpha = 0f;
                    uiController.selectionControls.alpha = 0f;
                    uiController.selectControlsText.enabled = true;
                    uiController.selectControlsImage.enabled = true;
                    uiController.selectControlsText.text = "Left/Right";
                    uiController.Loop2Image11.alpha = 0f;
                    uiController.Loop2Image12.alpha = 0f;
                    break;
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
        if (exp.beginScreenSelect == -1 && exp.beginScreenSelect == 0)
        {
            switch (pageID)
            {
                case 0:
                    if ((exp.beginScreenSelect != 0) &&
                        !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                    {
                        uiController.spacebarContinue.alpha = 0f;
                        uiController.selectionControls.alpha = 1f;
                        uiController.selectControlsText.text = "Previous/Next Page";
                    }
                    uiController.practiceInstructionPanel.alpha = 1f;
                    uiController.preSpatialRetrieval.enabled = true;
                    uiController.retrievalPanel.alpha = 0f;
                    uiController.itemReactivationPanel.alpha = 0f;
                    uiController.spatialInstructionA.enabled = false;
                    uiController.spatialInstructionB.enabled = false;
                    break;
                case 1:
                    if ((exp.beginScreenSelect != 0) &&
                        !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                    {
                        uiController.spacebarContinue.alpha = 0f;
                        uiController.selectionControls.alpha = 1f;
                    }

                    uiController.preSpatialRetrieval.enabled = false;
                    uiController.practiceInstructionPanel.alpha = 0f;

                    uiController.spatialInstructionA.enabled = true;
                    uiController.spatialInstructionB.enabled = false;
                    uiController.retrievalPanel.alpha = 1f;

                    string itemName = exp.objController.ReturnStimuliDisplayText();
                    uiController.itemReactivationText.text = itemName;
                    uiController.itemReactivationPanel.alpha = 1f;
                    break;
                //yield return StartCoroutine(WaitForActionButton());
                case 2:
                    if ((exp.beginScreenSelect != 0) &&
                        !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                    {
                        uiController.spacebarContinue.alpha = 0f;
                        uiController.selectionControls.alpha = 1f;
                    }
                    uiController.itemReactivationPanel.alpha = 0f;
                    uiController.spatialInstructionA.enabled = false;
                    uiController.spatialInstructionB.enabled = true;
                    break;
                // yield return StartCoroutine(WaitForActionButton());
                case 3:
                    if ((exp.beginScreenSelect != 0) &&
                        !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                    {
                        uiController.spacebarContinue.alpha = 0f;
                        uiController.selectionControls.alpha = 0f;
                        uiController.selectControlsText.text = "Left/Right";
                    }
                    uiController.itemReactivationPanel.alpha = 0f;
                    uiController.retrievalPanel.alpha = 0f;
                    uiController.spatialInstructionA.enabled = false;
                    uiController.spatialInstructionB.enabled = false;
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
