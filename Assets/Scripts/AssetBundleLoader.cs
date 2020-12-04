using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using System.Diagnostics;

public class AssetBundleLoader : MonoBehaviour
{
    private string baseAssetBundleFolder = "https://lightnarcissus.com/SCIFI_U01/AssetBundles/";
    //EXPERIMENT IS A SINGLETON
    private static AssetBundleLoader _instance;

    public GameObject currentActiveObj = null;
    public GameObject treasureChestPrefab = null;

    public AssetBundle treasureBundle;
    public AssetBundle envBundle;
    public AssetBundle audioBundle;

    public AudioClip beepLow;
    public AudioClip beepHigh;
    public AudioClip magicWand;


    public AudioClip correct;
    public AudioClip wrong;

    public AudioSource source;

    string targetFileFormat = ".wav";

    public LoadScenePrefabs scenePrefabLoader;

    public static AssetBundleLoader Instance
    {
        get
        {
            return _instance;
        }
    }


    void Awake()
    {
        if (_instance != null)
        {
            UnityEngine.Debug.Log("Instance already exists!");
            return;
        }
        _instance = this;
    }
        // Start is called before the first frame update
        void Start()
    {
        StartCoroutine("LoadAudioClip");
    }

    IEnumerator LoadAudioClip()
    {

        //beep high
        string FullPath = "https://spaceheist.s3.us-east-2.amazonaws.com/SPACEHEIST_WA_WEB/AssetBundles/beephigh" + targetFileFormat;
        WWW URL = new WWW(FullPath);
        yield return URL;
        beepHigh = URL.GetAudioClip(false, true, AudioType.WAV);


        //beep low
        FullPath = "https://spaceheist.s3.us-east-2.amazonaws.com/SPACEHEIST_WA_WEB/AssetBundles/beeplow" + targetFileFormat;
        URL = new WWW(FullPath);
        yield return URL;
        beepLow = URL.GetAudioClip(false, true, AudioType.WAV);


        FullPath = "https://spaceheist.s3.us-east-2.amazonaws.com/SPACEHEIST_WA_WEB/AssetBundles/MagicWand" + targetFileFormat;
        URL = new WWW(FullPath);
        yield return URL;
        magicWand = URL.GetAudioClip(false, true, AudioType.WAV);



        FullPath = "https://spaceheist.s3.us-east-2.amazonaws.com/SPACEHEIST_WA_WEB/AssetBundles/correct" + targetFileFormat;
         URL = new WWW(FullPath);
        yield return URL;
        correct = URL.GetAudioClip(false, true, AudioType.WAV);



        FullPath = "https://spaceheist.s3.us-east-2.amazonaws.com/SPACEHEIST_WA_WEB/AssetBundles/wrong" + targetFileFormat;
        URL = new WWW(FullPath);
        yield return URL;
        wrong = URL.GetAudioClip(false, true, AudioType.WAV);
    }

    public IEnumerator LoadAudio()
    {
        UnityEngine.Debug.Log("loading audio bundle");
        yield return StartCoroutine(scenePrefabLoader.LoadBundleViaWeb("audio"));


        beepHigh = audioBundle.LoadAsset<AudioClip>("beephigh");
        beepLow = audioBundle.LoadAsset<AudioClip>("beeplow");
        magicWand = audioBundle.LoadAsset<AudioClip>("Magic Wand Noise");

        yield return null;
    }
    public IEnumerator InstantiateEnvironment()
    {
        yield return StartCoroutine(scenePrefabLoader.LoadBundleViaWeb("westerntown"));

        /*
        //load westerntown
        string assetBundleName = "westerntown";
        //#if UNITY_WEBGL && !UNITY_EDITOR

        string uri = "file:///" + Application.dataPath + "/AssetBundles/" + assetBundleName;

        Debug.Log("uri is " + uri.ToString());
              string alt_uri = Application.dataPath + "/AssetBundles/" + assetBundleName;
        Debug.Log("alt uri is " + alt_uri.ToString());
        //#else
        //      string uri = "file:///" + Application.dataPath + "/AssetBundles/" + assetBundleName;
        //#endif

        UnityEngine.Networking.UnityWebRequest request
            = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
        yield return request.Send();
              */

       // envBundle = DownloadHandlerAssetBundle.GetContent(request);
        GameObject cyberCityPrefab = envBundle.LoadAsset<GameObject>("CyberCity");
        GameObject cyberCity = Instantiate(cyberCityPrefab, Experiment.Instance.cyberCitySpawnPos.position,Experiment.Instance.cyberCitySpawnPos.rotation) as GameObject;
  
        yield return null;


    }

    public IEnumerator SpawnTreasureChest()
    {
        string assetBundleName = "treasureitems";
        yield return StartCoroutine(scenePrefabLoader.LoadBundleViaWeb(assetBundleName));

        /*
        string file_path = Application.dataPath + "/bundle_path.txt";
        Debug.Log("filepath is " + file_path);
        StreamReader inp_stm = new StreamReader(file_path);
        string text = "";
        while (!inp_stm.EndOfStream)
        {
            text = inp_stm.ReadLine();
            // Do Something with the input. 
        }

        Debug.Log("text is " + text);

        inp_stm.Close();

        string uri = text + assetBundleName;

        Debug.Log("uri is " + uri.ToString());
        string alt_uri = Application.dataPath + "/AssetBundles/" + assetBundleName;
        Debug.Log("alt uri is " + alt_uri.ToString());
        
        //#if UNITY_WEBGL && !UNITY_EDITOR
      //  string uri = Application.dataPath + "/AssetBundles/" + assetBundleName;
//#else
       // string uri = "file:///" + Application.dataPath + "/AssetBundles/" + assetBundleName;
//#endif
        UnityEngine.Networking.UnityWebRequest request
            = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
        yield return request.Send();
        
        treasureBundle = DownloadHandlerAssetBundle.GetContent(request);
        */
        treasureChestPrefab = treasureBundle.LoadAsset<GameObject>("TreasureChest");
        yield return null;
    }

    public IEnumerator SpawnTreasureObject(string objName,Vector3 spawnPos)
    {
        UnityEngine.Debug.Log("obj name is " + objName);
        UnityEngine.Debug.Log("treasure bundle " + treasureBundle.ToString());
        string prefabName = objName;
        string ok = "cactus";
        GameObject treasureItem = treasureBundle.LoadAsset<GameObject>(prefabName);
        UnityEngine.Debug.Log("treasure item " + treasureItem.ToString());
        currentActiveObj = Instantiate(treasureItem,spawnPos, treasureItem.transform.rotation) as GameObject;
        UsefulFunctions.FaceObject(currentActiveObj, Experiment.Instance.player, false);
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
