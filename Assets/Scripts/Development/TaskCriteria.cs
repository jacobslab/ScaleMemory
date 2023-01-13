using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//task criteria compares TaskParam to a value
public class TaskCriteria<T>
{
    TaskParam<T> _param;
    T _value;
    public TaskCriteria(TaskParam<T> parameter,T value)
    {
        _param = parameter;
        _value = value;
    }

    public int PerformCheck(object obj)
    {
        if (obj == null)
            return 1;
        return 0;
    }
}
