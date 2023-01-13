using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Weather
{
    public Weather(WeatherType initializedWeather)
    {
        weatherMode = initializedWeather;
        name = weatherMode.ToString();
    }


    public string name;
    public enum WeatherType
    {
        Sunny,
        Rainy,
        Night
    };
    public WeatherType weatherMode;

}
