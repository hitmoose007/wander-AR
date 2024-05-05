using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using Immersal.REST;

public class ContentPlacementUI : MonoBehaviour
{
    [SerializeField]
    private GameObject m_fontStylePanel;

    public void ToggleFontStylePanel()
    {
        m_fontStylePanel.SetActive(false);
    }
}
