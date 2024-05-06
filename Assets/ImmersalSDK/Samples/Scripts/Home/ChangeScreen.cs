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
        // panelA.transform.localScale = new Vector3(0,0,0);
        // panelB.transform.localScale = new Vector3(1,1,1);
    }

    public void BackButtonClicked()
    {
        Destroy(TakePhotos.instance.mapImage);
        
        errorText.text = "";
        panelA.SetActive(true);
        panelB.SetActive(false);
        // panelA.transform.localScale = new Vector3(1,1,1);
        // panelB.transform.localScale = new Vector3(0,0,0);
    }
}
