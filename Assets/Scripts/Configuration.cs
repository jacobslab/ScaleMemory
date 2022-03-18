﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Configuration : MonoBehaviour {

  
    public static float familiarizationMaxTime { get { return (float)Configuration.GetSetting("familiarizationMaxTime"); } }


    public static string audioFileExtension { get { return (string)Configuration.GetSetting("audioFileExtension"); } }

    public static float distanceThreshold { get { return (float)Configuration.GetSetting("distanceThreshold"); } } //minimum distance before an object is said to be "on the point"
    public static float timeThreshold { get { return (float)Configuration.GetSetting("timeThreshold"); } } //minimum time before or after an action can be said to be "on cue"

    public static float timeBetweenLaps { get { return (float)Configuration.GetSetting("timeBetweenLaps"); } }

     public static int spawnCount { get { return (int)Configuration.GetSetting("spawnCount"); } }

    public static float itemPresentationTime { get { return (int)Configuration.GetSetting("itemPresentationTime"); } }

    public static int heartbeatInterval { get { return (int)Configuration.GetSetting("heartbeatIntervalMS"); } }

    public static int minBufferLures { get { return (int)Configuration.GetSetting("minBufferLures"); } }

    public static int minGapBetweenStimuli { get { return (int)Configuration.GetSetting("minGapBetweenStimuli"); } } //measured in waypoints

    //presentation jitter time
    public static float minJitterTime { get { return (float)Configuration.GetSetting("minJitterTime"); } }
    public static float maxJitterTime { get { return (float)Configuration.GetSetting("maxJitterTime"); } }

    //session count
    public static int totalSessions { get { return (int)Configuration.GetSetting("totalSessions"); } }//how many sessions to split the whole task into


    //frame speed
    public static float minFrameSpeed { get { return (float)Configuration.GetSetting("minFrameSpeed"); } }
    public static float maxFrameSpeed { get { return (float)Configuration.GetSetting("maxFrameSpeed"); } }

    public static float minRetrievalFrameSpeed { get { return (float)Configuration.GetSetting("minRetrievalFrameSpeed"); } }
    public static float maxRetrievalFrameSpeed { get { return (float)Configuration.GetSetting("maxRetrievalFrameSpeed"); } }

    //spawn possibility buffer to start and end of loop; measured in frames
    public static int startBuffer { get { return (int)Configuration.GetSetting("startBuffer"); } }
    public static int endBuffer { get { return (int)Configuration.GetSetting("endBuffer"); } }

    public static int minFramesBetweenStimuli { get { return (int)Configuration.GetSetting("minFramesBetweenStimuli"); } }
    public static int minGapToLure { get { return (int)Configuration.GetSetting("minGapToLure"); } }

    public static int luresPerTrial { get { return (int)Configuration.GetSetting("luresPerTrial"); } }

    //reactivation times
    public static float itemReactivationTime { get { return (float)Configuration.GetSetting("itemReactivationTime"); } }
    public static float locationReactivationTime { get { return (float)Configuration.GetSetting("locationReactivationTime"); } }


    //following two are based on the nomenclature of elemem config files, but non-elemem versions should also follow this naming convention
    public static string ipAddress { get { return (string)Configuration.GetSetting("elememServerIP"); } }
    public static int portNumber { get { return (int)Configuration.GetSetting("elememServerPort"); } }

    public enum StimMode
    {
        NONSTIM,
        STIM
    };

    public static StimMode stimMode = StimMode.NONSTIM;


    public static int ReturnWeatherTypes()
    {
        return Enum.GetNames(typeof(Weather.WeatherType)).Length;
    }
    private const string SYSTEM_CONFIG_NAME = "config.json";

    private static object systemConfig = null;
    private static object experimentConfig = null;


    public static T Get<T>(Func<T> getProp, T defaultValue)
    {
        try
        {
            return getProp.Invoke();
        }
        catch (MissingFieldException)
        {
            return defaultValue;
        }
    }

    // TODO: JPB: (Hokua) Should this function be templated? What are the pros and cons?
    //            Note: It could also be a "dynamic" type, but WebGL doesn't support it (so we can't use dynamic)
    //            Should it be a nullable type and remove the Get<T> function? (hint: Look up the ?? operator)
    private static object GetSetting(string setting)
    {
        UnityEngine.Debug.Log("looking for setting " + setting);
        object value;
        var experimentConfig = (IDictionary<string, object>)GetExperimentConfig();

        UnityEngine.Debug.Log("length of exp config " + experimentConfig.Count.ToString());
        if (experimentConfig.TryGetValue(setting, out value))
        {
            UnityEngine.Debug.Log("found value for " + setting + " : " + value);
            return value;
        }

        var systemConfig = (IDictionary<string, object>)GetSystemConfig();
        UnityEngine.Debug.Log("length of system config " + systemConfig.Count.ToString());
        if (systemConfig.TryGetValue(setting, out value))
        {
            UnityEngine.Debug.Log("found value for " + setting + " : " + value);
            return value;
        }

        throw new MissingFieldException("Missing Config Setting " + setting + ".");
    }

    private static object GetSystemConfig()
    {
        if (systemConfig == null)
        {
            // Setup config file
#if ELEMEM_TEST
            string configPath = System.IO.Path.Combine(
            Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName,
                "Configs");
            UnityEngine.Debug.Log("system config path " + configPath);
            string text = File.ReadAllText(Path.Combine(configPath, SYSTEM_CONFIG_NAME));
            systemConfig = FlexibleConfig.LoadFromText(text);
            UnityEngine.Debug.Log("loaded system text " + text);
#elif !UNITY_WEBGL // System.IO
            string configPath = System.IO.Path.Combine(
                Directory.GetParent(Directory.GetParent(Experiment.Instance.sessionDirectory).FullName).FullName,
                "Configs");
            string text = File.ReadAllText(Path.Combine(configPath, SYSTEM_CONFIG_NAME));

            systemConfig = FlexibleConfig.LoadFromText(text);
#else
                if (onlineSystemConfigText == null)
                    Debug.Log("Missing config from web");
                else
                    systemConfig = FlexibleConfig.LoadFromText(onlineSystemConfigText);
#endif
        }
        return systemConfig;
    }

    private static object GetExperimentConfig()
    {
        if (experimentConfig == null)
        {
            // Setup config file
#if ELEMEM_TEST
            string configPath = System.IO.Path.Combine(
            Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName,
                "Configs");
            UnityEngine.Debug.Log("config path " + configPath);
            string text = File.ReadAllText(Path.Combine(configPath, Experiment.ExpName + ".json"));
            experimentConfig = FlexibleConfig.LoadFromText(text);
            UnityEngine.Debug.Log("loaded experiment text " + text);
#elif !UNITY_WEBGL // System.IO
            string configPath = System.IO.Path.Combine(
                Directory.GetParent(Directory.GetParent(Experiment.Instance.ReturnSubjectDirectory()).FullName).FullName,
                "Configs");
            string text = File.ReadAllText(Path.Combine(configPath, Experiment.ExpName + ".json"));
            experimentConfig = FlexibleConfig.LoadFromText(text);
#else
                if (onlineExperimentConfigText == null)
                    Debug.Log("Missing config from web");
                else
                    experimentConfig = FlexibleConfig.LoadFromText(onlineExperimentConfigText);
#endif
        }
        return experimentConfig;
    }
}

