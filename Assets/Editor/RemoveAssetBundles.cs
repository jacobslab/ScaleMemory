using System.Collections;
using UnityEditor;
using UnityEngine;

public class RemoveAssetBundles : MonoBehaviour
{
    [MenuItem("Assets/Remove Bundle Name")]
    static void RemoveAssetBundleNameExample()
    {

        AssetDatabase.RemoveAssetBundleName("treasureitems", true);
    }
}