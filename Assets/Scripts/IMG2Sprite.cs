using UnityEngine;
using System.Collections;
using System.IO;


public class IMG2Sprite : MonoBehaviour
{


    // This script loads a PNG or JPEG image from disk and returns it as a Sprite
    // Drop it on any GameObject/Camera in your scene (singleton implementation)
    //
    // Usage from any other script:
    // MySprite = IMG2Sprite.instance.LoadNewSprite(FilePath, [PixelsPerUnit (optional)])

    public Sprite newSprite;
    public Rect rect;
    /*private static IMG2Sprite _instance;

    public static IMG2Sprite instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.

            if (_instance == null)
                _instance = GameObject.FindObjectOfType<IMG2Sprite>();
            return _instance;
        }
    }*/
    public void Start()
    {
        rect = new Rect(0f, 0f, 0f, 0f);
    }

    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {
        //Sprite NewSprite = new Sprite();
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Texture2D SpriteTexture = LoadTexture(FilePath);
        //Rect rect = new Rect(new Vector2(0f, 0f), new Vector2(SpriteTexture.width, SpriteTexture.height));
        rect.position = new Vector2(0f, 0f);
        rect.width = SpriteTexture.width;
        rect.height = SpriteTexture.height;


        newSprite = Sprite.Create(SpriteTexture, rect, new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect);

        return newSprite;
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;
        Debug.Log("LoadTexture: FilePath: " + FilePath);
        if (File.Exists(FilePath))
        {
            Debug.Log("LoadTexture: FilePath: Exists: " + FilePath);
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
}