using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BlockTestPair
{
    public GameObject firstItem;
    public GameObject secondItem;

    public enum BlockTestType
    {
        TemporalOrder,
        TemporalDistance,
        ContextRecollection

    }


    public BlockTestPair(GameObject itemA, GameObject itemB)
    {
        firstItem = itemA;
        secondItem = itemB;
    }


}
