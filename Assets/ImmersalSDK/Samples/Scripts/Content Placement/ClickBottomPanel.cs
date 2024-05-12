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
            
        }

        else if (name == "AddText Button" || name == "AddTarget Button")
        {

        }

        else if (name == "EditText Button" || name == "Settings Button")
        {
            
        }
    }
}
