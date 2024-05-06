using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.XR.CoreUtils;

public class ClickBottomPanel : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public void OnSelect (BaseEventData eventData)
    {
        Debug.Log( GetType() + "-" + name + "-OnSelect();");

        
    }

    public void OnDeselect (BaseEventData eventData)
    {
        Debug.Log( GetType() + "-" + name + "-OnDeselect();");
    }

    public void TaskOnClick()
    {
        if (name == "UploadImage Button" || name == "AddWaypoint Button")
        {
            // transform.parent.transform.GetChild(0).gameObject.SetActive(true);
            // transform.parent.transform.GetChild(1).gameObject.SetActive(false);
            // transform.parent.transform.GetChild(2).gameObject.SetActive(false);

            // Changing selected button color on click
            // var colors = gameObject.GetComponent<Button>().colors;
            // colors.normalColor = new Color32(255,255,255,255);
            // gameObject.GetComponent<Button>().colors = colors;
            
            // // Changing other buttons' colors on un-click
            // GameObject button2 = transform.parent.transform.GetChild(4).gameObject;
            // GameObject button3 = transform.parent.transform.GetChild(5).gameObject;

            // var colors2 = button2.GetComponent<Button>().colors;
            // colors2.normalColor = new Color32(156,156,156,255);
            // button2.GetComponent<Button>().colors = colors2;
            
            // colors2 = button3.GetComponent<Button>().colors;
            // colors2.normalColor = new Color32(156,156,156,255);
            // button3.GetComponent<Button>().colors = colors2;
        }

        else if (name == "AddText Button" || name == "AddTarget Button")
        {
            // transform.parent.transform.GetChild(0).gameObject.SetActive(false);
            // transform.parent.transform.GetChild(1).gameObject.SetActive(true);
            // transform.parent.transform.GetChild(2).gameObject.SetActive(false);

            // Changing selected button color on click
            // var colors = gameObject.GetComponent<Button>().colors;
            // colors.normalColor = new Color32(255,255,255,255);
            // gameObject.GetComponent<Button>().colors = colors;
            
            // Changing other buttons' colors on un-click
            // GameObject button2 = transform.parent.transform.GetChild(3).gameObject;
            // GameObject button3 = transform.parent.transform.GetChild(5).gameObject;

            // var colors2 = button2.GetComponent<Button>().colors;
            // colors2.normalColor = new Color32(156,156,156,255);
            // button2.GetComponent<Button>().colors = colors2;
            
            // colors2 = button3.GetComponent<Button>().colors;
            // colors2.normalColor = new Color32(156,156,156,255);
            // button3.GetComponent<Button>().colors = colors2;
        }

        else if (name == "EditText Button" || name == "Settings Button")
        {
            // transform.parent.transform.GetChild(0).gameObject.SetActive(false);
            // transform.parent.transform.GetChild(1).gameObject.SetActive(false);
            // transform.parent.transform.GetChild(2).gameObject.SetActive(true);

            // Changing selected button color on click
            // var colors = gameObject.GetComponent<Button>().colors;
            // colors.normalColor = new Color32(255,255,255,255);
            // gameObject.GetComponent<Button>().colors = colors;
            
            // Changing other buttons' colors on un-click
            // GameObject button2 = transform.parent.transform.GetChild(3).gameObject;
            // GameObject button3 = transform.parent.transform.GetChild(4).gameObject;

            // var colors2 = button2.GetComponent<Button>().colors;
            // colors2.normalColor = new Color32(156,156,156,255);
            // button2.GetComponent<Button>().colors = colors2;
            
            // colors2 = button3.GetComponent<Button>().colors;
            // colors2.normalColor = new Color32(156,156,156,255);
            // button3.GetComponent<Button>().colors = colors2;
        }
    }
}
