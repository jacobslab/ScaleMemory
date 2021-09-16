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


    //encoding panel
    public CanvasGroup encodingPanel;

    //retrieval panel
    public CanvasGroup retrievalPanel;
    public CanvasGroup verbalRetrievalPanel;

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
            currSelection = 0; //set to default start
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

    public void ResetRetrievalInstructions()
    {
        itemRetrievalInstructionPanel.alpha = 0f;
        locationRetrievalInstructionPanel.alpha = 0f;
    }


}
