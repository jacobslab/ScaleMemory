using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IntPair
{
    public int firstInt;
    public int secondInt;
    public IntPair(int A, int B)
    {
        firstInt = A;
        secondInt = B;
    }
}

public class LoopTest : MonoBehaviour
{
    public List<int> sequence = new List<int>();
    public Dictionary<int, List<int>> loopDict = new Dictionary<int, List<int>>();
    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine("RunTest");
        }
    }

    IEnumerator RunTest()
    {
        for (int i = 0; i < 20; i++)
        {
            sequence.Add(i);
        }
        TestFunc();
        yield return null;
    }

    public void TestFunc()
    {
        int loopsPerBlock = 4;

        List<IntPair> intPairList = new List<IntPair>();

        List<int> currList = new List<int>(); //temp list we will store items of each loop in
        int listIndex = 0; //to keep track of the loop number in the dictionary
        for (int i = 0; i < sequence.Count; i++)
        {
            currList.Add(sequence[i]);
            if ((i + 1) % 5 == 0)
            {
                loopDict.Add(listIndex, UsefulFunctions.DuplicateList(currList));
                currList.Clear(); //clear the list for the next loop
                listIndex++; //increment the counter
            }
        }

        List<int> loopNumList = UsefulFunctions.ReturnListOfOrderedInts(4);
        //pick 2 pairs in same loop
        //this will either be in the first loop or last loop of the block
        int loopNum = ((Random.value > 0.5f) ? 0 : loopsPerBlock - 1);

        loopNumList.RemoveAt(loopNum); //remove the loop from consideration for other conditions
        List<int> chosenLoop = new List<int>();
        //since there are only two possibilities, we will manually select a random pair between them
        if (loopDict.TryGetValue(loopNum, out chosenLoop))
        {
            UnityEngine.Debug.Log("considering in loop num " + loopNum.ToString());
            List<int> indicesToRemove = new List<int>();
            if (Random.value > 0.5f)
            {
                intPairList.Add(new IntPair(chosenLoop[0], chosenLoop[3])); //1 and 4
                UnityEngine.Debug.Log("choosing " + chosenLoop[0].ToString() + " and " + chosenLoop[3].ToString());
                if (loopNum == 0) //if selected loop was the first loop of the block
                {
                    indicesToRemove.Add(0);
                    RemoveIndices(1, indicesToRemove); //only remove the first index of the next loop
                }
                else //or if it was the last loop of the block
                {

                    indicesToRemove.Add(3);
                    indicesToRemove.Add(4);
                    RemoveIndices(loopsPerBlock - 2, indicesToRemove); //remove last two indices of the penultimate loop
                }
            }
            else
            {
                intPairList.Add(new IntPair(chosenLoop[1], chosenLoop[4])); //2 and 5
                UnityEngine.Debug.Log("choosing " + chosenLoop[1].ToString() + " and " + chosenLoop[4].ToString());
                if (loopNum == 0)
                {

                    indicesToRemove.Add(0);
                    indicesToRemove.Add(1);
                    RemoveIndices(1, indicesToRemove);
                }
                else
                {
                    indicesToRemove.Add(4);
                    RemoveIndices(loopsPerBlock - 2, indicesToRemove);
                }

            }
        }
        else
        {
            UnityEngine.Debug.Log("no key exists " + loopNum.ToString());
        }


        //different loop,same weather



        for (int i = 0; i < 4; i++)
        {
            List<int> resultList = new List<int>();
            if (loopDict.TryGetValue(i, out resultList))
            {
                for (int j = 0; j < resultList.Count; j++)
                {
                    UnityEngine.Debug.Log("loop " + i.ToString() + " val " + resultList[j].ToString());
                }
            }

        }
        loopDict.Clear();


    }

    void RemoveIndices(int targetLoopNum, List<int> indicesToRemove)
    {
        UnityEngine.Debug.Log("about to remove " + indicesToRemove.Count.ToString() + " in loop num " + targetLoopNum.ToString());
        List<int> targetList = new List<int>();
        if (loopDict.TryGetValue(targetLoopNum, out targetList))
        {
            for (int i = 0; i < indicesToRemove.Count; i++)
            {
                targetList.RemoveAt(indicesToRemove[0]);
            }
            loopDict.Remove(targetLoopNum);
            loopDict.Add(targetLoopNum, targetList); 
        }
    }
}
