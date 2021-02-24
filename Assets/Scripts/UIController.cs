using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour {


	private float prevLapTime=0f;
	public CanvasGroup retrievalTextPanel;
	public CanvasGroup targetTextPanel;
	public Text zRetrievalText;
	public Text mRetrievalText;
	public Text itemName;

	//item screening
	public CanvasGroup itemScreeningPanel;

	//track screening
	public CanvasGroup trackScreeningPanel;

	//encoding panel
	public CanvasGroup encodingPanel;

    //mic test group

    public CanvasGroup micInstructionsGroup;
    public CanvasGroup micTestGroup;
    public CanvasGroup micSuccessGroup;
    public Image micStatusImage;

    //retrieval panel
    public CanvasGroup retrievalPanel;
	public CanvasGroup verbalRetrievalPanel;
	public Text itemOneName;
	public Text itemTwoName;
	public CanvasGroup spatialRetrievalFeedbackPanel;
    public CanvasGroup spatialFeedbackContinuePanel;

    public Text debugText;

	//intro panel
	public CanvasGroup taskIntroPanel;

    //consent panel
    public CanvasGroup consentPanel;



    //prolific info
    public CanvasGroup prolificInfoPanel;
    public CanvasGroup failProlificPanel;

    //fixation panel
    public CanvasGroup fixationPanel;
	public CanvasGroup fixationCross;

	//blackrock connection
	public CanvasGroup blackrockConnectionPanel;
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

    //fast drive message
    public CanvasGroup fastDriveMessage;

    public CanvasGroup verbalInstruction;

	//black screen
	public CanvasGroup blackScreen;

	//end session
	public CanvasGroup endSessionPanel;

	// info text
	public TextMeshPro infoText;
	

	// Use this for initialization
	void Start () {
		targetTextPanel.alpha = 0f;
		retrievalTextPanel.alpha = 0f;
	}
	
	// Update is called once per frame
	void Update () {
	}

	

}
