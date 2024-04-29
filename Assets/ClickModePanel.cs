using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.XR.CoreUtils;
public class ClickModePanel : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public GameObject contentBackground;
    public GameObject navigationBackground;

    public GameObject contentButtonsPanel;
    public GameObject navigationButtonsPanel;
    
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

            contentButtonsPanel.SetActive(true);
            navigationButtonsPanel.SetActive(false);
        }

        else if (name == "Navigation Button")
        {
            navigationBackground.SetActive(true);
            contentBackground.SetActive(false);

            navigationButtonsPanel.SetActive(true);
            contentButtonsPanel.SetActive(false);
        }
    }
}
