using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trial {

    public int trialNumber;
    public Weather encodingWeather;
    public Weather retrievalWeather;
    public Experiment.TaskStage retrievalType;


    public Trial(int trialNum, Weather encoding, Weather retrieval,int retrievalTypeIndex)
    {
        trialNumber = trialNum;
        encodingWeather = encoding;
        retrievalWeather = retrieval;
        SetRetrievalType(retrievalTypeIndex);
    }

    public void SetRetrievalType(int retrievalTypeIndex)
    {
        retrievalType = ((retrievalTypeIndex == 0) ? Experiment.TaskStage.VerbalRetrieval : Experiment.TaskStage.SpatialRetrieval);
    }
}
