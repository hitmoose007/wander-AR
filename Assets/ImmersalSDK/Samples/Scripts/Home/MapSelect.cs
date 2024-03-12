using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelect : MonoBehaviour
{
    public int mapId;

    // Start is called before the first frame update
    // void Update()
    // {
    //     // Check if there is at least one touch currently
    // }

    public void OnTouchDetected()
    {
        // Add your logic here for what happens when the object is touched
        Debug.Log("Square was touched.");
        //debug map id

        // Set the map id in the PlayerPrefs
        StaticData.MapIdContentPlacement = mapId;
        Debug.Log("da real Map id: " + mapId);
        StaticData.LoadScene(StaticData.GameScene.ContentPlacementScene);
        // Load the scene
    }
}
