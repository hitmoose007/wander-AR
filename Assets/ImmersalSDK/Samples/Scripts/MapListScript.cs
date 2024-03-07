using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MapListScript : MonoBehaviour
{
    [Serializable]
    public struct Map
    {
        public string name;
    }

    [SerializeField] Map[] allMaps;

    // Start is called before the first frame update
    void Start()
    {
        GameObject mapItem = transform.GetChild(0).gameObject;
        GameObject g;

        int N = allMaps.Length;
        
        for(int i = 0; i < N; i++)
        {
            g = Instantiate(mapItem, transform);
            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allMaps[i].name;
        }

        Destroy(mapItem);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
