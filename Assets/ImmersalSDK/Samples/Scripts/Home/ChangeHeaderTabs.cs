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
    public GameObject privatePanel;
    public GameObject publicPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelect (BaseEventData eventData)
    {
        Debug.Log( GetType() + "-" + name + "-OnSelect();");

        // Changing color of button text for visibility
        this.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(23,26,31,255);

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

        // Changing color of button text for visibility
        this.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color32(251,229,186,255);
    }

    public void OnButtonClick()
    {
        if (name == "Private Maps Button")
        {
            GameObject content = publicPanel.transform.GetChild(0).gameObject;

            for (var i = content.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }
        }

        else if (name == "Public Maps Button")
        {
            GameObject content = privatePanel.transform.GetChild(0).gameObject;
            
            for (var i = content.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }
        }
    }
}
