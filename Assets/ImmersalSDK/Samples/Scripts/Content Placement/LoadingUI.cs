using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadingUI : MonoBehaviour
{
    public Button contentButton;

    // Start is called before the first frame update
    void Start()
    {
        contentButton.Select();
    }
}
