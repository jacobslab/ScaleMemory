using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{

    Experiment exp { get { return Experiment.Instance; } }
    private float prevLapTime = 0f;

    //retrieval
    public CanvasGroup retrievalTextPanel;
    public CanvasGroup targetTextPanel;
    public Text retrievalItemName;

    //prolific specific
    public CanvasGroup prolificInfoPanel;
    public CanvasGroup consentPanel;
    public CanvasGroup failProlificPanel;

    //presentation text
    public Text presentationItemText;

    //loading screen
    public CanvasGroup loadingScreen;
    public bl_ProgressBar loadingBar;

    //subject info entry panel
    public CanvasGroup subjectInfoPanel;
    public InputField subjectInputField;

    //next trial screen
    public CanvasGroup nextTrialPanel;

    //track screening
    public CanvasGroup trackScreeningPanel;
    public CanvasGroup familiarizationOverheadInstructions;
    public Text familiarizationOverheadInstructionText;

    //practice panel
    public CanvasGroup practiceInstructionPanel;
    public Text preEncodingInstructions;
    public Text preSpatialRetrieval;
    public Text preWeatherCondition;
    public Text secondEncodingInstructions;


    //encoding panel
    public CanvasGroup encodingPanel;

    //post practice panel
    public CanvasGroup postPracticePanel;

    //mic access panel
    public CanvasGroup micAccessPanel;

    //retrieval panel
    public CanvasGroup retrievalPanel;
    public CanvasGroup verbalRetrievalPanel;

    //follow up test panel
    public CanvasGroup followUpTestPanel;

    public Text spatialInstructionA;
    public Text spatialInstructionB;

    public Text verbalInstructionA;
    public Text verbalInstructionB;

    public Text itemOneName;
    public Text itemTwoName;
    public CanvasGroup spatialRetrievalFeedbackPanel;


    //controls instruction images
    public CanvasGroup spacebarContinue;
    public CanvasGroup spacebarPlaceItem;
    public CanvasGroup pageControls;
    public CanvasGroup driveControls;
    public CanvasGroup selectionControls;

    //reactivation panel
    public CanvasGroup locationReactivationPanel;
    public CanvasGroup locationRetrievalInstructionPanel;
    public CanvasGroup itemReactivationPanel;
    public Text itemReactivationText;
    public CanvasGroup itemReactivationDetails;
    public CanvasGroup itemRetrievalInstructionPanel;
    public Text itemRetrievalInstructionText;
    public RawImage microphoneIconImage;

    string itemRetrievalInstructionBase = "drive to location of ";

    //end of block tests
    public CanvasGroup endOfBlockInstructionPanel;
    public CanvasGroup temporalOrderTestPanel;
    public CanvasGroup temporalDistanceTestPanel;
    public CanvasGroup contextRecollectionTestPanel;


    public Text temporalOrderItemA;
    public Text temporalOrderItemB;

    public Text temporalDistanceItemA;
    public Text temporalDistanceItemB;


    public Text contextRecollectionItem;

    //intro panel
    public CanvasGroup taskIntroPanel;

    //fixation panel
    public CanvasGroup fixationPanel;
    public CanvasGroup fixationCross;

    //stimulus display panel
    public CanvasGroup stimDisplayPanel;
    public RawImage stimItemImage;
    public Text stimNameText;
    //blue marker indicator
    public CanvasGroup markerCirclePanel;

    //elemem connection
    public CanvasGroup elememConnectionPanel;
    public CanvasGroup connectionSuccessPanel;
    public Text connectionText;

    //intermission
    public CanvasGroup intermissionInstructionPanel;

    //ip entry
    public CanvasGroup ipEntryPanel;
    public InputField ipAddrInput;

    //lap time
    public CanvasGroup lapTimePanel;
    public Text currentLapTimeText;
    public Text bestLapTimeText;
    public Text timeSplitText;

    //arrow
    public CanvasGroup leftTurnArrow;
    public CanvasGroup rightTurnArrow;

    //crash notification
    public CanvasGroup crashNotification;

    public CanvasGroup verbalInstruction;

    //black screen
    public CanvasGroup blackScreen;

    //end session
    public CanvasGroup endSessionPanel;


    // info text
//    public TextMeshPro infoText;

    public enum OptionSelection
    {
        Left,
        Right
    }

    public Image selectionImage;

    public List<GameObject> itemCuedSelectionCanvasElements = new List<GameObject>();
    public List<GameObject> locationCuedSelectionCanvasElements = new List<GameObject>();
    public List<GameObject> temporalDistanceCanvasElements = new List<GameObject>();


    private List<Vector3> itemCuedSelectionPositions = new List<Vector3>();
    private List<Vector3> locationCuedSelectionPositions = new List<Vector3>();
    private List<Vector3> temporalDistancePositions = new List<Vector3>();

    private List<Vector3> activeSelectionPositions;
    private int currSelection = 0;
    private int maxOptions = 0;

    public delegate IEnumerator OnUIPageChange();
    public static event OnUIPageChange uiPageChange;

    private int currUIPageID = 0;
    private int maxPage = 0;

    private string currInstPage = "";


    private int maxVerbalPages = 2;
    private int maxSpatialPages = 4;

    public bool showInstructions = false;

    

    // Use this for initialization
    void Start()
    {
        targetTextPanel.alpha = 0f;
        ToggleSelection(false);
        presentationItemText.enabled = false;
        selectionImage.enabled = false;

        blackScreen.alpha = 0f;


        activeSelectionPositions = new List<Vector3>();
     //   retrievalTextPanel.alpha = 0f;

        for (int i = 0; i < itemCuedSelectionCanvasElements.Count; i++)
        {
            itemCuedSelectionPositions.Add(itemCuedSelectionCanvasElements[i].GetComponent<RectTransform>().anchoredPosition);
        }
        for (int i = 0; i < locationCuedSelectionCanvasElements.Count; i++)
        {
            locationCuedSelectionPositions.Add(locationCuedSelectionCanvasElements[i].GetComponent<RectTransform>().anchoredPosition);
        }

        for (int i = 0; i < temporalDistanceCanvasElements.Count; i++)
        {
            temporalDistancePositions.Add(temporalDistanceCanvasElements[i].GetComponent<RectTransform>().anchoredPosition);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator UpdateUIPage()
    {
        UnityEngine.Debug.Log("updating UI page to " + currUIPageID.ToString());
        if(Experiment.Instance.currentStage == Experiment.TaskStage.VerbalRetrieval)
        {
            yield return StartCoroutine(exp.instructionsManager.UpdateVerbalInstructions());
        }
        else if(Experiment.Instance.currentStage == Experiment.TaskStage.SpatialRetrieval)
        {
            yield return StartCoroutine(exp.instructionsManager.UpdateSpatialInstructions());
        }

        //uiPageChange();
    }


    public void UpdateLoadingProgress(float loadPercent)
    {
        loadingBar.Value = loadPercent;
    }

    public void SetElememInstructions(string newText)
    {
        connectionText.text = newText;
    }

    //these are used to ask subject to check with the testing supervisor
    public IEnumerator ShowIntermissionInstructions()
    {
        intermissionInstructionPanel.alpha = 1f;
        exp.trialLogTrack.LogIntermission(true);
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        intermissionInstructionPanel.alpha = 0f;
        exp.trialLogTrack.LogIntermission(false);
        yield return null;
    }

    public IEnumerator SetActiveInstructionPage(string instructionPage)
    {
        //reset the page ID
        bool isSpatial = false;
        currUIPageID = 0;
        currInstPage = instructionPage;
        switch(instructionPage)
        {
            case "Verbal":
             //   uiPageChange += Experiment.Instance.UpdateVerbalInstructions;
                maxPage = maxVerbalPages;
                UnityEngine.Debug.Log("set verbal instruction as active");
                break;
            case "Spatial":
                isSpatial = true;
               // uiPageChange += Experiment.Instance.UpdateSpatialInstructions;
                maxPage = maxSpatialPages;
                UnityEngine.Debug.Log("set spatial instruction as active");
                break;
            default:
                UnityEngine.Debug.Log("on the default case");
                break;
        }
        showInstructions = true;
        if(!isSpatial)
            pageControls.alpha = 1f;
        //then force update it so it shows up with the first page
        yield return StartCoroutine(UpdateUIPage());

        yield return null;
    }

    public void SetFamiliarizationInstructions(Weather.WeatherType currWeather)
    {
        familiarizationOverheadInstructions.alpha = 1f;
        string baseInst = "Take a moment to drive around and learn the city layout and surroundings while it is " + currWeather.ToString();
        familiarizationOverheadInstructionText.text = baseInst;
        //switch (currWeather)
        //{
        //    case Weather.WeatherType.Sunny:
        //        familiarizationOverheadInstructionText.text = "Take a moment to drive around and learn the city layout and surroundings while it is Sunny";
        //        break;
        //    case Weather.WeatherType.Rainy:
        //        familiarizationOverheadInstructionText.text = "Take a moment to drive around and learn the city layout and surroundings while it is Rainy";
        //        break;
        //    case Weather.WeatherType.Night:
        //        familiarizationOverheadInstructionText.text = "Take a moment to drive around and learn the city layout and surroundings while it is Night";
        //        break;

        //}

    }

    public void FinishInstructionSequence(string instructionPage)
    {
        switch (instructionPage)
        {
            case "Verbal":
                uiPageChange -= exp.instructionsManager.UpdateVerbalInstructions;
                break;
            case "Spatial":
                uiPageChange -= exp.instructionsManager.UpdateSpatialInstructions;
                break;
        }
        currUIPageID = 0;

        pageControls.alpha = 0f;
        UnityEngine.Debug.Log("finishing the instruction sequence");
        showInstructions = false;
    }

    public int GetCurrentUIPage()
    {
        return currUIPageID;
    }

    public IEnumerator SetItemRetrievalInstructions(string objName)
    {
        itemRetrievalInstructionPanel.alpha = 1f;
        itemRetrievalInstructionText.text = itemRetrievalInstructionBase + objName;
        yield return null;
    }

    public int GetSelectionIndex()
    {
        return currSelection;
    }

    public IEnumerator SetupSelectionOptions(string selectionType)
    {
        //reset
        activeSelectionPositions.Clear();

        activeSelectionPositions = new List<Vector3>();

        switch (selectionType)
        {
            case "Item":
            for (int i = 0; i < itemCuedSelectionPositions.Count; i++)
            {
                activeSelectionPositions.Add(itemCuedSelectionPositions[i]);
            }
            maxOptions = itemCuedSelectionPositions.Count;
            break;

            case "Location":
            for (int i = 0; i < locationCuedSelectionPositions.Count; i++)
            {
                activeSelectionPositions.Add(locationCuedSelectionPositions[i]);
            }
            maxOptions = locationCuedSelectionPositions.Count;
            break;
            case "TemporalOrder":
                for (int i = 0; i < locationCuedSelectionPositions.Count; i++)
                {

                    Vector3 currPos = locationCuedSelectionPositions[i] - new Vector3(200f * Mathf.Sign(locationCuedSelectionPositions[i].x),-300f,0f);
                    activeSelectionPositions.Add(currPos);
                }
                maxOptions = locationCuedSelectionPositions.Count;

                break;
            case "TemporalDistance":
                for (int i = 0; i < temporalDistancePositions.Count; i++)
                {
                    activeSelectionPositions.Add(temporalDistancePositions[i]);
                }
                maxOptions = temporalDistancePositions.Count;
                break;
            case "ContextRecollection":
                for (int i = 0; i < itemCuedSelectionPositions.Count; i++)
                {
                    activeSelectionPositions.Add(itemCuedSelectionPositions[i]);
                }
                maxOptions = itemCuedSelectionPositions.Count;
                break;

        }   

        UnityEngine.Debug.Log("setting selection options with max options at " + maxOptions.ToString());
        currSelection = 0;

        yield return null;
    }

    public void ToggleSelection(bool isEnabled)
    {

        selectionImage.enabled = isEnabled;
        if (isEnabled)
        {
            currSelection = 0; //set to default start
            ResetSelection();
        }
        }

    public IEnumerator SetLocationRetrievalInstructions()
    {
        locationRetrievalInstructionPanel.alpha = 1f;
        microphoneIconImage.color = Color.white;

        yield return null;
    }
    public void PerformSelection(OptionSelection newSelection)
    {
        if (newSelection == OptionSelection.Left)
        {
            currSelection--;
        }
        else
        {
            currSelection++;
        }

        //wrap it around
        if (currSelection >= maxOptions)
        {
            currSelection = 0;
        }
        else if (currSelection < 0)
        {
            if (maxOptions != 0)
                currSelection = maxOptions - 1;
            else
                currSelection = 0;
        }
        UnityEngine.Debug.Log("curr selection is " + currSelection.ToString());
        selectionImage.GetComponent<RectTransform>().anchoredPosition = activeSelectionPositions[currSelection] - new Vector3(0f, 50f, 0f);


    }

    public void PerformUIPageChange(OptionSelection newOption)
    {
        if (newOption == OptionSelection.Left)
        {
            UnityEngine.Debug.Log("moving left");
            currUIPageID--;
        }
        else
        {
            UnityEngine.Debug.Log("moving right");
            currUIPageID++;
        }
        //if exceeded beyond max page, then finish the sequence
        if (currUIPageID >= maxPage)
        {
            StartCoroutine(UpdateUIPage());
            FinishInstructionSequence(currInstPage);

        }
        else if (currUIPageID < 0)
        {
            StartCoroutine(UpdateUIPage());
            currUIPageID = 0;
        }
        else
        {
            UnityEngine.Debug.Log("updating UI page to  " + currUIPageID.ToString());

            //after changing the page, call the delegate associated with it so we can actually update the relevant instruction page
            StartCoroutine(UpdateUIPage());
        }

    }

    void ResetSelection()
    {
        UnityEngine.Debug.Log("reset selection");
        currSelection = 0;
        selectionImage.GetComponent<RectTransform>().anchoredPosition = activeSelectionPositions[currSelection] - new Vector3(0f, 50f, 0f);
    }

    public void ResetRetrievalInstructions()
    {
        itemRetrievalInstructionPanel.alpha = 0f;
        locationRetrievalInstructionPanel.alpha = 0f;
    }


}
