using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherPair
{
    public Weather encodingWeather;
    public Weather retrievalWeather;

    public WeatherPair(Weather.WeatherType encoding, Weather.WeatherType retrieval)
    {
        encodingWeather = new Weather(encoding);
        retrievalWeather = new Weather(retrieval);
    }

}
