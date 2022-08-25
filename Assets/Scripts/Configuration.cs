using Newtonsoft.Json;
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


    public static Experiment exp { get { return Experiment.Instance; } }
    public static float familiarizationMaxTime { get { return float.Parse(Configuration.GetSetting("familiarizationMaxTime")); } }


    public static string audioFileExtension { get { return Configuration.GetSetting("audioFileExtension"); } }

     public static int spawnCount { get { return int.Parse(Configuration.GetSetting("spawnCount")); } }

    public static float itemPresentationTime { get { return float.Parse(Configuration.GetSetting("itemPresentationTime")); } }

    public static int heartbeatInterval { get { return int.Parse(Configuration.GetSetting("heartbeatIntervalMS")); } }

    public static int minBufferLures { get { return int.Parse(Configuration.GetSetting("minBufferLures")); } }


    //presentation jitter time
    public static float minJitterTime { get { return float.Parse(Configuration.GetSetting("minJitterTime")); } }
    public static float maxJitterTime { get { return float.Parse(Configuration.GetSetting("maxJitterTime")); } }

    //session count
    public static int totalSessions { get { return int.Parse(Configuration.GetSetting("totalSessions")); } }//how many sessions to split the whole task into


    //frame speed
    public static float minFrameSpeed { get { return float.Parse(Configuration.GetSetting("minFrameSpeed")); } }
    public static float maxFrameSpeed { get { return float.Parse(Configuration.GetSetting("maxFrameSpeed")); } }

    public static float minRetrievalFrameSpeed { get { return float.Parse(Configuration.GetSetting("minRetrievalFrameSpeed")); } }
    public static float maxRetrievalFrameSpeed { get { return float.Parse(Configuration.GetSetting("maxRetrievalFrameSpeed")); } }

    //spawn possibility buffer to start and end of loop; measured in frames
    public static int startBuffer { get { return int.Parse(Configuration.GetSetting("startBuffer")); } }
    public static int endBuffer { get { return int.Parse(Configuration.GetSetting("endBuffer")); } }

    public static int minFramesBetweenStimuli { get { return int.Parse(Configuration.GetSetting("minFramesBetweenStimuli")); } }
    public static int minGapToLure { get { return int.Parse(Configuration.GetSetting("minGapToLure")); } }

    public static int luresPerTrial { get { return int.Parse(Configuration.GetSetting("luresPerTrial")); } }

    //reactivation times
    public static float itemReactivationTime { get { return float.Parse(Configuration.GetSetting("itemReactivationTime")); } }
    public static float locationReactivationTime { get { return float.Parse(Configuration.GetSetting("locationReactivationTime")); } }


    public static float pauseBtwnEndQuestions { get { return float.Parse(Configuration.GetSetting("pauseBtwnEndQuestions")); } }
    public static float pauseBtwnEachSpatialQuestion { get { return float.Parse(Configuration.GetSetting("pauseBtwnEachSpatialQuestion")); } }
    public static float fastSpeed { get { return float.Parse(Configuration.GetSetting("fastSpeed")); } }
    public static float slowSpeed { get { return float.Parse(Configuration.GetSetting("slowSpeed")); } }

    //following two are based on the nomenclature of elemem config files, but non-elemem versions should also follow this naming convention
    public static string ipAddress { get { return Configuration.GetSetting("elememServerIP"); } }
    public static int portNumber { get { return int.Parse(Configuration.GetSetting("elememServerPort")); } }

    public enum StimMode
    {
        closed,
        open
    };

    public static StimMode stimMode { get { return (StimMode) Enum.Parse(typeof(StimMode),Configuration.GetSetting("stimMode")); } }


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

    // TODO:instead of returning string; return appropriate value based on a new query typeOf argument
    private static string GetSetting(string setting)
    {
        UnityEngine.Debug.Log("looking for setting " + setting);
        object value;
        var experimentConfig = (IDictionary<string, object>)GetExperimentConfig();

        UnityEngine.Debug.Log("length of exp config " + experimentConfig.Count.ToString());
        if (experimentConfig.TryGetValue(setting, out value))
        {
            UnityEngine.Debug.Log("found value for " + setting + " : " + value);
            return value.ToString();
        }

        var systemConfig = (IDictionary<string, object>)GetSystemConfig();
        UnityEngine.Debug.Log("length of system config " + systemConfig.Count.ToString());
        if (systemConfig.TryGetValue(setting, out value))
        {
            UnityEngine.Debug.Log("found value for " + setting + " : " + value);
            return value.ToString();
        }

        throw new MissingFieldException("Missing Config Setting " + setting + ".");
    }

    private static object GetSystemConfig()
    {
        if (systemConfig == null)
        {
#if !UNITY_WEBGL
            // Setup config file
            string configPath = System.IO.Path.Combine(
            Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName,
                "Configs");
            UnityEngine.Debug.Log("system config path " + configPath);
            string text = File.ReadAllText(Path.Combine(configPath, SYSTEM_CONFIG_NAME));
            systemConfig = FlexibleConfig.LoadFromText(text);
            UnityEngine.Debug.Log("loaded system text " + text);

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
#if !UNITY_WEBGL

            UnityEngine.Debug.Log("data path " + Application.dataPath);
            UnityEngine.Debug.Log("one level up " + Directory.GetParent(Application.dataPath).FullName);
            UnityEngine.Debug.Log("another level up " + Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName);
            // Setup config file
            string configPath = System.IO.Path.Combine(
            Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName,
                "Configs");
            UnityEngine.Debug.Log("config path " + configPath);
            string text = "";
            if (exp.beginScreenSelect != 0)
            {
#if BEHAVIORAL
            string text = File.ReadAllText(Path.Combine(configPath, Experiment.ExpName + "_behavioral.json"));
#else
                text = File.ReadAllText(Path.Combine(configPath, Experiment.ExpName + ".json"));
#endif
            }
            else {
                text = File.ReadAllText(Path.Combine(configPath, Experiment.ExpName + "_mri.json"));
            }
            experimentConfig = FlexibleConfig.LoadFromText(text);
            UnityEngine.Debug.Log("loaded experiment text " + text);
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

