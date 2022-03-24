using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//simple generic class
public class TaskParam<T>
{

    public T value { get; set; }

    public TaskParam(T val)
    {
        value = val;
    }
}
