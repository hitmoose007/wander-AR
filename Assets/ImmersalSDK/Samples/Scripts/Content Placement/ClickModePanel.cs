using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.XR.CoreUtils;
using UnityEditor.Experimental.GraphView;
public class ClickModePanel : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public GameObject contentBackground;
    public GameObject navigationBackground;
    public GameObject contentButtonsPanel;
    public GameObject navigationButtonsPanel;
    public Button contentButton;
    public Button navigationButton;
    
    public void OnSelect (BaseEventData eventData)
    {
        Debug.Log( GetType() + "-" + name + "-OnSelect();");
        OnButtonClick();
    }

    public void OnDeselect (BaseEventData eventData)
    {
        Debug.Log( GetType() + "-" + name + "-OnDeselect();");
    }

    public void OnButtonClick()
    {
        if (name == "Content Place Button")
        {
            contentBackground.SetActive(true);
            navigationBackground.SetActive(false);

            contentButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(255,255,255,255);
            navigationButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(0,0,0,255);
            
            /* Have to change to be invisible, not inactive */
            contentButtonsPanel.SetActive(true);
            navigationButtonsPanel.SetActive(false);

            contentButtonsPanel.transform.GetChild(0).gameObject.SetActive(false);
            contentButtonsPanel.transform.GetChild(1).gameObject.SetActive(false);
            contentButtonsPanel.transform.GetChild(2).gameObject.SetActive(false);
        }

        else if (name == "Navigation Button")
        {
            navigationBackground.SetActive(true);
            contentBackground.SetActive(false);

            navigationButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(255,255,255,255);
            contentButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(0,0,0,255);

            /* Have to change to be invisible, not inactive */
            navigationButtonsPanel.SetActive(true);
            contentButtonsPanel.SetActive(false);

            navigationButtonsPanel.transform.GetChild(0).gameObject.SetActive(false);
            navigationButtonsPanel.transform.GetChild(1).gameObject.SetActive(false);
            navigationButtonsPanel.transform.GetChild(2).gameObject.SetActive(false);
        }
    }
}
