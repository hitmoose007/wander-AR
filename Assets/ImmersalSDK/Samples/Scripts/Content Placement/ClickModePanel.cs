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

    public Button contentButton;
    public Button navigationButton;
    public Button navigationListButton;
    public Button stopNavigationButton;
    
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

            //Show panel
            contentButtonsPanel.transform.localScale = new Vector3(1,1,1);
            // Hide panel
            navigationButtonsPanel.transform.localScale = new Vector3(0,0,0);
            navigationListButton.transform.localScale = new Vector3(0,0,0);
            stopNavigationButton.transform.localScale = new Vector3(0,0,0);

            for (int i = 0; i < 3; i++)
            {
                contentButtonsPanel.transform.GetChild(i).gameObject.SetActive(false);
            }

            for (int i = 3; i < 6; i++)
            {
                GameObject childButton = contentButtonsPanel.transform.GetChild(i).gameObject;

                var colors = childButton.GetComponent<Button>().colors;
                colors.normalColor = new Color32(156,156,156,255);
                childButton.GetComponent<Button>().colors = colors;
            }
        }

        else if (name == "Navigation Button")
        {
            navigationBackground.SetActive(true);
            contentBackground.SetActive(false);

            navigationButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(255,255,255,255);
            contentButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(0,0,0,255);

            //Show panel
            navigationButtonsPanel.transform.localScale = new Vector3(1,1,1);
            navigationListButton.transform.localScale = new Vector3(1,1,1);
            stopNavigationButton.transform.localScale = new Vector3(1,1,1);
            // Hide panel
            contentButtonsPanel.transform.localScale = new Vector3(0,0,0);

            for (int i = 0; i < 3; i++)
            {
                navigationButtonsPanel.transform.GetChild(i).gameObject.SetActive(false);
            }
            
            for (int i = 3; i < 6; i++)
            {
                GameObject childButton = navigationButtonsPanel.transform.GetChild(i).gameObject;

                var colors = childButton.GetComponent<Button>().colors;
                colors.normalColor = new Color32(156,156,156,255);
                childButton.GetComponent<Button>().colors = colors;
            }
        }
    }
}
