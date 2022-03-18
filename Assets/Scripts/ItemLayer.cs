using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class ItemLayer : MonoBehaviour
{

    public RawImage bgLayer;

    private Texture2D sunny_card;
    private Texture2D rain_card;
    private Texture2D night_card;

    private Texture2D active_card;
    // Start is called before the first frame update
    void Start()
    {

        active_card = sunny_card;
        bgLayer.texture = active_card;
    }

    public IEnumerator AssociateImageWithWeather(Texture2D image, string weather)
    {
        switch(weather)
        {
            case "Sunny":
                sunny_card = image;
                break;
            case "Rainy":
                rain_card = image;
                break;
            case "Night":
                night_card = image;
                break;
        }

        bgLayer.texture = image;
        yield return null;
    }


    public void UpdateImage(Weather.WeatherType newWeather)
    {
        UnityEngine.Debug.Log("updating weather for item layer " + newWeather.ToString());
        switch(newWeather)
        {
            case Weather.WeatherType.Sunny:
                active_card = sunny_card;
                bgLayer.texture = active_card;
                break;
            case Weather.WeatherType.Rainy:
                active_card = rain_card;
                bgLayer.texture = active_card;
                break;
            case Weather.WeatherType.Night:
                active_card = night_card;
                bgLayer.texture = active_card;
                break;
            default:
                active_card = sunny_card;
                bgLayer.texture = active_card;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
