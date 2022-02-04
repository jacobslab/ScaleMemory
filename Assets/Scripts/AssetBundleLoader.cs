using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using System.Diagnostics;

public class AssetBundleLoader : MonoBehaviour
{
    //private string baseAssetBundleFolder = "https://lightnarcissus.com/SCIFI_U01/AssetBundles/";

    public static string baseBundlePath = "https://spaceheist.s3.us-east-2.amazonaws.com/SPACEHEIST_WA_WEB/AssetBundles/WebGL/";

    //public static string baseBundlePath = "https://lightnarcissus.com/AssetBundles/";
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

    public bool isWeb = true;

    public VideoLayerManager vidLayerManager;
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
    }


    public IEnumerator LoadCamTransform()
    {
        AssetBundle myLoadedAssetBundle;
        string bundleName = "misc";
        string uri = "";
        if (isWeb)
        {
            uri = Path.Combine(AssetBundleLoader.baseBundlePath, bundleName);

            UnityEngine.Debug.Log("URI " + uri);
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
            UnityEngine.Debug.Log("sending request");
            yield return StartCoroutine(WebReq(request));
            UnityEngine.Debug.Log("received data from the request");
            myLoadedAssetBundle = DownloadHandlerAssetBundle.GetContent(request);
            string obj = "";
        }
        else
        {
            uri = Path.Combine(Application.dataPath + "/AssetBundles", bundleName);
            UnityEngine.Debug.Log("URI " + uri);
            myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles/", bundleName));
            if (myLoadedAssetBundle == null)
            {
                UnityEngine.Debug.Log("Failed to load AssetBundle!");
                yield return null;
            }
        }
        if (myLoadedAssetBundle == null)
        {
            UnityEngine.Debug.Log("Failed to load AssetBundle!");
            yield return null;
        }


        //string[]names = myLoadedAssetBundle.GetAllAssetNames();
        //UnityEngine.Debug.Log("name length " + names.Length.ToString());
        var loadedCamTransform = myLoadedAssetBundle.LoadAsset<TextAsset>("cam_transform");

        Experiment.Instance.camTransformTextAsset = loadedCamTransform;


        yield return null;
    }



    public IEnumerator LoadItemLayer(string bundleName)
    {

        AssetBundle myLoadedAssetBundle;
        string uri = "";
        if (isWeb)
        {
            uri = Path.Combine(AssetBundleLoader.baseBundlePath, bundleName);

            UnityEngine.Debug.Log("URI " + uri);
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
            UnityEngine.Debug.Log("sending request");
            yield return StartCoroutine(WebReq(request));
            UnityEngine.Debug.Log("received data from the request");
            myLoadedAssetBundle = DownloadHandlerAssetBundle.GetContent(request);
            string obj = "";
        }
        else
        {
            uri = Path.Combine(Application.dataPath + "/AssetBundles", bundleName);
            UnityEngine.Debug.Log("URI " + uri);
            myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles/", bundleName));
            if (myLoadedAssetBundle == null)
            {
                UnityEngine.Debug.Log("Failed to load AssetBundle!");
                yield return null;
            }
        }



        var loadedTexture = myLoadedAssetBundle.LoadAsset<Texture2D>("sunny_card-001.png");
        UnityEngine.Debug.Log("loaded image card " + loadedTexture.name);
        yield return StartCoroutine(vidLayerManager.itemLayer.AssociateImageWithWeather(loadedTexture,"Sunny"));


        loadedTexture = myLoadedAssetBundle.LoadAsset<Texture2D>("rain_card-001.png");
        UnityEngine.Debug.Log("loaded image card " + loadedTexture.name);
        yield return StartCoroutine(vidLayerManager.itemLayer.AssociateImageWithWeather(loadedTexture, "Rainy"));

        loadedTexture = myLoadedAssetBundle.LoadAsset<Texture2D>("night_card-001.png");
        UnityEngine.Debug.Log("loaded image card " + loadedTexture.name);
        yield return StartCoroutine(vidLayerManager.itemLayer.AssociateImageWithWeather(loadedTexture, "Night"));

        yield return null;
    }


    IEnumerator<UnityWebRequestAsyncOperation> WebReq(UnityWebRequest request)
    {
        yield return request.SendWebRequest();
    }


    public IEnumerator LoadStimuliImages()
    {
        AssetBundle myLoadedAssetBundle;
        string uri = "";
        string bundleName = "stimuli";

        if (isWeb)
        {
            uri = Path.Combine(AssetBundleLoader.baseBundlePath, bundleName);
            UnityEngine.Debug.Log("URI " + uri);
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
            UnityEngine.Debug.Log("sending request");
            yield return StartCoroutine(WebReq(request));
            UnityEngine.Debug.Log("received data from the request");
            myLoadedAssetBundle = DownloadHandlerAssetBundle.GetContent(request);
            string obj = "";
        }
        else
        {
            uri = Path.Combine(Application.dataPath + "/AssetBundles", bundleName);
            UnityEngine.Debug.Log("URI " + uri);
            myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles/", bundleName));
            if (myLoadedAssetBundle == null)
            {
                UnityEngine.Debug.Log("Failed to load AssetBundle!");
                yield return null;
            }
        }

        if (myLoadedAssetBundle == null)
        {
            UnityEngine.Debug.Log("Failed to load AssetBundle!");
            yield return null;
        }
        Object[] images = myLoadedAssetBundle.LoadAllAssets(typeof(Texture));
        yield return StartCoroutine(Experiment.Instance.objController.FillPermanentImageList(images));

        yield return null;
    }


    public IEnumerator LoadTexturesFromBundle(string bundleName)
    {
        //var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(AssetBundleLoader.baseBundlePath, bundleName));
        AssetBundle myLoadedAssetBundle;

        UnityEngine.Debug.Log("about to load " + bundleName);
        string uri = "";

        if (isWeb)
        {
            uri = Path.Combine(AssetBundleLoader.baseBundlePath, bundleName);
            UnityEngine.Debug.Log("URI " + uri);
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
            UnityEngine.Debug.Log("sending request");
            yield return StartCoroutine(WebReq(request));
            UnityEngine.Debug.Log("received data from the request");
            myLoadedAssetBundle = DownloadHandlerAssetBundle.GetContent(request);
            string obj = "";
        }
        else
        {
            uri = Path.Combine(Application.dataPath + "/AssetBundles", bundleName); UnityEngine.Debug.Log("URI " + uri);
            myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles/", bundleName));
            if (myLoadedAssetBundle == null)
            {
                UnityEngine.Debug.Log("Failed to load AssetBundle!");
                yield return null;
            }
        }

        


        //var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath +"/AssetBundles/", bundleName));
        if (myLoadedAssetBundle == null)
        {
            UnityEngine.Debug.Log("Failed to load AssetBundle!");
            yield return null;
        }
        string layerName = bundleName;
        //vidLayerManager.GetMainLayerCurrentFrameNumber();

        for (int i = 0; i < 1131; i++)
        {
            string targetName = string.Format(layerName + "-{0:d3}", i + 1) + ".jpg";
            var loadedTexture = myLoadedAssetBundle.LoadAsset<Texture2D>(targetName);
           // UnityEngine.Debug.Log("loading  " + loadedTexture.name);
            vidLayerManager.newTextures.Add(loadedTexture);

        }
        UnityEngine.Debug.Log("finished loading for weather " + bundleName);
        yield return null;
    }


    public IEnumerator LoadAudioClip()
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

    public IEnumerator SpawnTreasureObject(string objName, Vector3 spawnPos)
    {
        UnityEngine.Debug.Log("obj name is " + objName);
        UnityEngine.Debug.Log("treasure bundle " + treasureBundle.ToString());
        string prefabName = objName;
        string ok = "cactus";
        GameObject treasureItem = treasureBundle.LoadAsset<GameObject>(prefabName);
        UnityEngine.Debug.Log("treasure item " + treasureItem.ToString());
        currentActiveObj = Instantiate(treasureItem, spawnPos, treasureItem.transform.rotation) as GameObject;
        UsefulFunctions.FaceObject(currentActiveObj, Experiment.Instance.player, false);
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
