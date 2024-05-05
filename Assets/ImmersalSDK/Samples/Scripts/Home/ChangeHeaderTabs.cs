using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.XR.CoreUtils;

public class ChangeHeaderTabs : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Button privateButton, publicButton;
    public GameObject privatePanel;
    public GameObject publicPanel;

    public void OnSelect (BaseEventData eventData)
    {
        Debug.Log( GetType() + "-" + name + "-OnSelect();");

        if (name == "Private Maps Button")
        {
            privatePanel.SetActive(true);
            publicPanel.SetActive(false);
        }

        else if (name == "Public Maps Button")
        {
            publicPanel.SetActive(true);
            privatePanel.SetActive(false);
        }
    }

    public void OnDeselect (BaseEventData eventData)
    {
        Debug.Log( GetType() + "-" + name + "-OnDeselect();");
    }

    public void TaskOnButtonClick()
    {
        if (name == "Private Maps Button")
        {
            Debug.Log( GetType() + "-" + name + "-OnClick();");

            // Changing button color on un-click
            var colors2 = publicButton.GetComponent<Button>().colors;
            colors2.normalColor = new Color32(38,40,45,255);
            publicButton.GetComponent<Button>().colors = colors2;

            // Changing button text color on un-click
            publicButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(162,172,188,255);
            
            // Changing button color on click
            var colors = privateButton.GetComponent<Button>().colors;
            colors.normalColor = new Color32(0,69,224,255);
            privateButton.GetComponent<Button>().colors = colors;

            // Changing button text color on click
            privateButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(255,255,255,255);

            // Deleting un-clicked button's map list content items to prevent clones
            GameObject content = publicPanel.transform.GetChild(0).gameObject;

            for (var i = content.transform.childCount - 1; i >= 0; i--)
            {   
                if (content.transform.GetChild(i).gameObject != null)
                {
                    Destroy(content.transform.GetChild(i).gameObject);
                }
            }
        }

        else if (name == "Public Maps Button")
        {   
            Debug.Log( GetType() + "-" + name + "-OnClick();");

            // Changing button color on un-click
            var colors2 = privateButton.GetComponent<Button>().colors;
            colors2.normalColor = new Color32(38,40,45,255);
            privateButton.GetComponent<Button>().colors = colors2;
            
            // Changing button text color on un-click
            privateButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(162,172,188,255);

            // Changing button color on click
            var colors = publicButton.GetComponent<Button>().colors;
            colors.normalColor = new Color32(0,69,224,255);
            publicButton.GetComponent<Button>().colors = colors;

            // Changing button text color on click
            publicButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(255,255,255,255);

            // Deleting un-clicked button's map list content items to prevent clones
            GameObject content = privatePanel.transform.GetChild(0).gameObject;
            
            for (var i = content.transform.childCount - 1; i >= 0; i--)
            {
                if (content.transform.GetChild(i).gameObject != null)
                {
                    Destroy(content.transform.GetChild(i).gameObject);
                }
            }
        }

        else {
            Debug.Log( GetType() + "-" + name + "-error;");
        }
    }
}
