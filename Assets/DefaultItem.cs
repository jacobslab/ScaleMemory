using UnityEngine;
using System.Collections;
using System.Diagnostics;

//Default item, aka a treasure chest.
public class DefaultItem : MonoBehaviour
{
	Experiment exp { get { return Experiment.Instance; } }

	//opening variables
	public Transform pivotA;
	public Transform pivotB;
	public Transform top;
	public Transform specialObjectSpawnPoint;

	public GameObject renderParent; //we will use this to turn on and off the rendered objects

	float angleToOpen = 150.0f; //degrees


	public ParticleSystem DefaultParticles;
	public ParticleSystem SpecialParticles;

	public AudioSource defaultCollisionSound;
	public AudioSource specialCollisionSound;

    public AudioClip magicWand;

	bool isExecutingPlayerCollision = false;
	//bool shouldDie = false;

	public TextMesh specialObjectText;

	void Awake()
	{
		GetComponent<VisibilityToggler>().TurnVisible(true);
		//InitTreasureState();
	}

	//void InitTreasureState()
	//{
	//	switch (exp.trialController.NumDefaultObjectsCollected)
	//	{
	//		case 0:
	//			TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_1, true);
	//			break;
	//		case 1:
	//			TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_2, true);
	//			break;
	//		case 2:
	//			TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_3, true);
	//			break;
	//		case 3:
	//			TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_4, true);
	//			break;
	//	}
	//}

	// Use this for initialization
	void Start()
	{
		if (specialObjectText != null)
		{
			specialObjectText.text = "";
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player" && (tag == "DefaultObject" || tag == "DefaultSpecialObject") && !isExecutingPlayerCollision)
		{
			//if (!ExperimentSettings.isOculus)
			//{
				//GetComponent<TreasureChestLogTrack>().LogPlayerChestCollision();

				isExecutingPlayerCollision = true;
				StartCoroutine(RunCollision());

			//}
			//else
			//{
			//	StartCoroutine("WaitForObjectLookAt");
			//}
		}
	}

	//IEnumerator WaitForObjectLookAt()
	//{
	//	Debug.Log("default item wait for object look at");
	//	yield return StartCoroutine(Experiment.Instance.trialController.WaitForPlayerToLookAt(gameObject));
	//	//open the object
	//	StartCoroutine(Open(Experiment.Instance.player.gameObject));

	//	//if it was a special spot and this is the default object...
	//	//...we should spawn the special object!
	//	if (tag == "DefaultSpecialObject")
	//	{

	//		yield return StartCoroutine(SpawnSpecialObject(specialObjectSpawnPoint.position));

	//	}
	//	else
	//	{
	//		yield return StartCoroutine(RunDefaultCollision());
	//	}
	//}

	public IEnumerator RunCollision()
	{

        //yield return StartCoroutine(Experiment.Instance.trialController.WaitForPlayerRotationToTreasure(gameObject));

        //open the object
        UnityEngine.Debug.Log("opening chest");
		StartCoroutine(Open(Experiment.Instance.player));

		//if it was a special spot and this is the default object...
		//...we should spawn the special object!
		if (tag == "DefaultSpecialObject")
		{

            UnityEngine.Debug.Log("spawning image plane");
            yield return StartCoroutine(SpawnSpecialObject(specialObjectSpawnPoint.position));

		}
		else
        {
            UnityEngine.Debug.Log("running default collision");
            yield return StartCoroutine(RunDefaultCollision());
		}



	}

    public IEnumerator SpawnObject()
    {

		yield return StartCoroutine(SpawnSpecialObject(specialObjectSpawnPoint.position));
		yield return null;
	}

	IEnumerator RunDefaultCollision()
	{
		//shouldDie = true;

		//PlayJuice(false);

		yield return StartCoroutine(Experiment.Instance.WaitForTreasurePause(null));

		/*while(SpecialParticles.isPlaying && DefaultParticles.isPlaying){
			yield return 0;
		}*/

		Destroy(gameObject); //once audio & particles have finished playing, destroy the item!
	}

	void PlayJuice(bool isSpecial)
	{
		//	if (Config_CoinTask.isJuice)
		//	{
		//		if (isSpecial)
		//		{
		//			JuiceController.PlayParticles(SpecialParticles);

		UnityEngine.Debug.Log("playing audio of treasure chest");
		specialCollisionSound.PlayOneShot(magicWand);
		//AudioController.PlayAudio(specialCollisionSound);
		//		}
		//		else
		//		{
		//			JuiceController.PlayParticles(DefaultParticles);
		//			AudioController.PlayAudio(defaultCollisionSound);
		//		}
		//	}
		}

		IEnumerator SpawnSpecialObject(Vector3 specialSpawnPos)
	{



        GameObject specialObject = Instantiate(Experiment.Instance.imagePlanePrefab, specialSpawnPos, Quaternion.identity) as GameObject;

        //GameObject specialObject = Experiment.Instance.SpawnSpecialObject(specialSpawnPos);
        Texture stimImage = Experiment.Instance.objController.ReturnStimuliToPresent();
        string stimDisplayText = Experiment.Instance.objController.ReturnStimuliDisplayText();
        GetComponent<StimulusObject>().stimuliDisplayName = stimDisplayText;
        specialObject.GetComponent<SpawnableImage>().SetImage(stimImage);

        //attach image plane to treasure chest's parent which is ItemColliderBox
        specialObject.transform.parent = transform.parent;

        //reset rotation to match the parent's rotation
        specialObject.transform.localRotation = Quaternion.identity;

	//	string name = specialObject.GetComponent<SpawnableObject>().GetDisplayName();
		//set special object text
		SetSpecialObjectText(stimDisplayText);

	//	Experiment.Instance.trialController.AddNameToList(specialObject, stimDisplayText);

		PlayJuice(true);

		//tell the trial controller to wait for the animation
		yield return StartCoroutine(Experiment.Instance.WaitForTreasurePause(specialObject));

		//should destroy the chest after the special object time
		//Destroy(gameObject);
	}

	public void SetSpecialObjectText(string text)
	{
		specialObjectText.text = text;
		Experiment.Instance.trialLogTrack.LogTreasureLabel(specialObjectText.text);
	}

	//open. most likely a treasure chest. could also be something like a giftbox.
	public IEnumerator Open(GameObject opener)
	{

		//if (GetIsSpecial())
		//{
		//	TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_OPEN_SPECIAL, true);
		//}
		//else
		//{
		//	TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_OPEN_EMPTY, true);
		//}

		float distOpenerToPivotA = (pivotA.position - opener.transform.position).magnitude;
		float distOpenerToPivotB = (pivotB.position - opener.transform.position).magnitude;

		Vector3 pivotPos = transform.position;
		string closePivotName = ""; //actually want to use the closer pivot as our opener reference for Logging
		if (distOpenerToPivotA > distOpenerToPivotB)
		{ //use the further away pivot
			pivotPos = pivotA.position;
			closePivotName = pivotB.name;
		}
		else
		{
			pivotPos = pivotB.position;
			closePivotName = pivotA.name;
			angleToOpen = -angleToOpen;
		}

		//GetComponent<TreasureChestLogTrack>().LogOpening(closePivotName, GetIsSpecial());

		//Quaternion origRotation = top.rotation;


		float angleChange = 8.0f;
		float directionMult = 1.0f;

		if (angleToOpen < 0)
		{
			directionMult = -1.0f;
		}

		float timeElapsed = 0f;
		float minTime = 0.25f;

		//if (Config.isJuice)
		//{
			//animate if juice!
			while (directionMult * angleToOpen > 0 || timeElapsed < minTime)
			{

				if (!Experiment.isPaused)
				{
					timeElapsed += Time.deltaTime;
				if (directionMult * angleToOpen > 0)
				{
					top.RotateAround(pivotPos, -directionMult * transform.right, angleChange);
					angleToOpen -= directionMult * angleChange;
				}
				}
				yield return 0;
			}
		//}
		//else
		//{
		//	top.RotateAround(pivotPos, transform.right, -angleToOpen);
		//}

		yield return 0;
	}

	bool GetIsSpecial()
	{
		if (gameObject.tag == "DefaultSpecialObject")
		{
			return true;
		}
		return false;
	}

	void OnDestroy()
	{
		UnityEngine.Debug.Log("destroying chest");
		//EndTreasureState();
	}

	//void EndTreasureState()
	//{
	//	if (GetIsSpecial())
	//	{
	//		TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_OPEN_SPECIAL, false);
	//	}
	//	else
	//	{
	//		TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_OPEN_EMPTY, false);
	//	}
	//	switch (exp.trialController.NumDefaultObjectsCollected)
	//	{
	//		case 1:
	//			TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_1, false);
	//			break;
	//		case 2:
	//			TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_2, false);
	//			break;
	//		case 3:
	//			TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_3, false);
	//			break;
	//		case 4:
	//			TCPServer.Instance.SetState(TCP_Config.DefineStates.TREASURE_4, false);
	//			break;
	//	}
	//}
}
