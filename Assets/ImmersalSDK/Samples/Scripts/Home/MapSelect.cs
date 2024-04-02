using System.Collections;
using System.Collections.Generic;
using Immersal.REST;
using UnityEngine;

public class MapSelect : MonoBehaviour
{
    public int mapId;
    public string jobState;

    // Start is called before the first frame update
    // void Update()
    // {
    //     // Check if there is at least one touch currently
    // }

    public void OnTouchDetected()
    {
        // Add your logic here for what happens when the object is touched
        //debug map id

        // Set the map id in the PlayerPrefs
        if (jobState == SDKJobState.Done)
        {
            StaticData.MapIdContentPlacement = mapId;
            StaticData.LoadScene(StaticData.GameScene.ContentPlacementScene);
        }
        // Load the scene
    }
}
