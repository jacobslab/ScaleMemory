using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
public class LoadScenePrefabs : MonoBehaviour
{

    private string[] assetBundleNames = new string[4];
    private GameObject experimentObj;
    private List<GameObject> scenePrefabList = new List<GameObject>();


    enum AssetBundleType
    {
        Native,
        WWW
    };

    private AssetBundleType assetBundleType = AssetBundleType.WWW;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        //   StartCoroutine(LoadBundleViaWeb("cube"));
    }

    // Update is called once per frame
    void Update()
    {
    }


  

    public IEnumerator LoadBundleViaWeb(string bundleName)
    {
        Debug.Log("loading from web " + bundleName);
        //    yield return StartCoroutine(LoadPrefabFromWWWAssetBundle("base", -1));
        //  experimentObj = basePrefabObj.GetComponent<BaseObject>().experimentObj;
        yield return StartCoroutine(LoadPrefabFromWWWAssetBundle(bundleName, 0));

        //begin the task
        //  StartCoroutine("BeginTask");
        //turn off the second environment for now
        //  scenePrefabList[1].gameObject.SetActive(false);
        yield return null;
    }

    IEnumerator LoadPrefabFromWWWAssetBundle(string bundleName, int index)
    {
        //Debug.Log("datapath " + Application.dataPath);
        //var path = Path.Combine(Application.dataPath, "AssetBundles/WebGL");
        //  var path = "http://orion.bme.columbia.edu/jacobs/heist_task_data/AssetBundles/WebGL";

        var path = "https://spaceheist.s3.us-east-2.amazonaws.com/SPACEHEIST_WA_WEB/SCIFI_Bundles/";
        //     var path = "https://lightnarcissus.com/U01Test/AssetBundles/";


        //  string targetBundle = "cube";
        string uri = Path.Combine(path, bundleName);
        Debug.Log("the uri is " + uri);
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
        Debug.Log("sending request");
        yield return StartCoroutine(WebReq(request));
        Debug.Log("received data from the request");
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        string obj = "";
        if (bundleName == "westerntown")
        {
            AssetBundleLoader.Instance.envBundle = bundle;
        }
        else if (bundleName == "treasureitems")
        {
            AssetBundleLoader.Instance.treasureBundle = bundle;

        }
        else if (bundleName == "audio")
        {
            AssetBundleLoader.Instance.audioBundle = bundle;

        }
        else
        {
            obj = "Cube";
            //     GameObject prefabObj = bundle.LoadAsset<GameObject>(obj);
            //     GameObject prefab = Instantiate(prefabObj) as GameObject;

        }
        //  GameObject prefabObj = bundle.LoadAsset<GameObject>(obj);
        //  GameObject prefab = Instantiate(prefabObj) as GameObject;
        //if the prefab loaded is the base, then do the initial setup

        /*
        if (bundleName == "base")
        {
            basePrefabObj = prefab;
            experimentObj = basePrefabObj.GetComponent<BaseObject>().experimentObj;
            assetBundleNames[0] = "spacestation";
            assetBundleNames[1] = "office";
        }
        //else, it is loading up individual environment prefabs
        else
        {
            //fill out missing references to cam zone which are now children of "Base" GameObject
            basePrefabObj.GetComponent<BaseObject>().AddCamZoneReferences(prefab);
            scenePrefabList.Add(prefab);

            Debug.Log("assigning " + scenePrefabList[index]);
            experimentObj.GetComponent<Experiment>().shopLift.environments[index] = scenePrefabList[index];
            Debug.Log("in shoplift env " + experimentObj.GetComponent<Experiment>().shopLift.environments[index].gameObject.name);
        }

    */
        //    GameObject cube = bundle.LoadAsset<GameObject>(bundleName);
        yield return null;

    }

    IEnumerator<UnityWebRequestAsyncOperation> WebReq(UnityWebRequest request)
    {
        yield return request.SendWebRequest();
    }

    GameObject LoadPrefabFromNativeAssetBundle(string bundleName)
    {
        GameObject prefab = null;
        Debug.Log("datapath " + Application.dataPath);
        var path = Path.Combine(Application.dataPath, "AssetBundles");
        Debug.Log("path " + path);
#if !UNITY_EDITOR_OSX
        var assetBundlePath = path;
#else
        var assetBundlePath = path;
#endif
        Debug.Log("assetbundlepath " + assetBundlePath);
        var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(path, bundleName));
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return prefab;
        }
        prefab = myLoadedAssetBundle.LoadAsset<GameObject>(bundleName);
        Instantiate(prefab);
        return prefab;
    }
}
