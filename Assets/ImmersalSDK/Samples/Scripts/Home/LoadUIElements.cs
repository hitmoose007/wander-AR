using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadUIElements : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject cameraPanel;
    public Button headerTabButton;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        homePanel.SetActive(true);
        cameraPanel.SetActive(false);
        headerTabButton.Select();
    }
}
