using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorMessage
{

    private string display_msg = "";
    public enum SeverityLevel
    {
        Minor,
        Critical
    };

    public SeverityLevel errorSeverity;


    public ErrorMessage(string _msg, SeverityLevel _severityLevel )
    {
        display_msg = _msg;
        errorSeverity = _severityLevel;
    }


    public string GetErrorMessage()
    {
        return display_msg;
    }

}
