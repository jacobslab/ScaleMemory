using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//THIS CLASS VALIDATES IF THE TASK FULFILLS TaskCriteria

public class TaskChecker 
{
    List<TaskCriteria<int>> _intTaskCriteria;
    List<TaskCriteria<float>> _floatTaskCriteria;
    List<TaskCriteria<string>> _stringTaskCriteria;
}
