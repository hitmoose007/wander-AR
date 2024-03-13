using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Immersal.Samples;

public class InputHandler : MonoBehaviour
{
    [SerializeField]
    TMP_InputField inputField;

    [SerializeField]
    TextMeshProUGUI errorText;

    public void RetrieveInput()
    {//RESET
        StaticData.MapperSceneMapName = "";
        string input = inputField.text;

        if (input.Length > 0 && StaticData.MapperSceneMapImage != null)
        {
            errorText.text = "";
            /* IMPORTANT */
            StaticData.MapperSceneMapName = input;

            StaticData.LoadScene(StaticData.GameScene.MapperScene);
            //set map image


            // Debug.Log("Input: " + input);
        }
        else if (input.Length <= 0 && StaticData.MapperSceneMapImage != null)
        {
            errorText.text = "Invalid map name!";
        }
        else if (input.Length > 0 && StaticData.MapperSceneMapImage == null)
        {
            errorText.text = "Thumbnail image has not been captured!";
        }
        else
        {
            errorText.text = "Map name & thumbnail image missing!";
        }
    }
}
