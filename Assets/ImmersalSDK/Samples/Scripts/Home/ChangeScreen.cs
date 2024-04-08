using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeScreen : MonoBehaviour
{
    public GameObject panelA;
    public GameObject panelB;
    public TextMeshProUGUI errorText;

    public void AddButtonClicked()
    {
        panelA.SetActive(false);
        panelB.SetActive(true);
    }

    public void BackButtonClicked()
    {
        Destroy(TakePhotos.instance.mapImage);
        
        errorText.text = "";
        panelB.SetActive(false);
        panelA.SetActive(true);
    }
}
