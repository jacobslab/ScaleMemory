using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{


    private float prevLapTime = 0f;

    //retrieval
    public CanvasGroup retrievalTextPanel;
    public CanvasGroup targetTextPanel;
    public Text retrievalItemName;

    //presentation text
    public Text presentationItemText;

    //subject info entry panel
    public CanvasGroup subjectInfoPanel;
    public InputField subjectInputField;

    //track screening
    public CanvasGroup trackScreeningPanel;

    //practice panel
    public CanvasGroup practiceInstructionPanel;
    public Text preEncodingInstructions;
    public Text preSpatialRetrieval;
    public Text preWeatherCondition;
    public Text secondEncodingInstructions;


    //encoding panel
    public CanvasGroup encodingPanel;

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

    //intro panel
    public CanvasGroup taskIntroPanel;

    //fixation panel
    public CanvasGroup fixationPanel;
    public CanvasGroup fixationCross;

    //blackrock connection
    public CanvasGroup blackrockConnectionPanel;
    public CanvasGroup connectionSuccessPanel;
    public Text connectionText;

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

    private List<Vector3> itemCuedSelectionPositions = new List<Vector3>();
    private List<Vector3> locationCuedSelectionPositions = new List<Vector3>();

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
            yield return StartCoroutine(Experiment.Instance.UpdateVerbalInstructions());
        }
        else if(Experiment.Instance.currentStage == Experiment.TaskStage.SpatialRetrieval)
        {
            yield return StartCoroutine(Experiment.Instance.UpdateSpatialInstructions());
        }

        //uiPageChange();
    }

    public IEnumerator SetActiveInstructionPage(string instructionPage)
    {
        //reset the page ID
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
               // uiPageChange += Experiment.Instance.UpdateSpatialInstructions;
                maxPage = maxSpatialPages;
                UnityEngine.Debug.Log("set spatial instruction as active");
                break;
            default:
                UnityEngine.Debug.Log("on the default case");
                break;
        }
        showInstructions = true;
        //then force update it so it shows up with the first page
        yield return StartCoroutine(UpdateUIPage());

        yield return null;
    }

    public void FinishInstructionSequence(string instructionPage)
    {
        switch (instructionPage)
        {
            case "Verbal":
                uiPageChange -= Experiment.Instance.UpdateVerbalInstructions;
                break;
            case "Spatial":
                uiPageChange -= Experiment.Instance.UpdateSpatialInstructions;
                break;
        }
        currUIPageID = 0;
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

    public IEnumerator SetupSelectionOptions(string retrievalType)
    {
        //reset
        activeSelectionPositions.Clear();

        activeSelectionPositions = new List<Vector3>();
        if (retrievalType == "Item")
        {
            for(int i=0;i<itemCuedSelectionPositions.Count;i++)
            {
                activeSelectionPositions.Add(itemCuedSelectionPositions[i]);
            }
            maxOptions = itemCuedSelectionPositions.Count;

        }
        else if (retrievalType == "Location")
        {
            for (int i = 0; i < locationCuedSelectionPositions.Count; i++)
            {
                activeSelectionPositions.Add(locationCuedSelectionPositions[i]);
            }
            maxOptions = locationCuedSelectionPositions.Count;
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
