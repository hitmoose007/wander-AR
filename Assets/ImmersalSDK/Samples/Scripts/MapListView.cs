using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapListView : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform listItemHolder;
    public int numOfListItems;
    List<string> mapName = new List<string>();

    private void Awake()
    {
        for (int i = 1; i <= 10; i++)
        {
            mapName.Add("Map " + i);
        }

        /* mapName.Add("Map 1");
        mapName.Add("Map 2");
        mapName.Add("Map 3");
        mapName.Add("Map 4");
        mapName.Add("Map 5"); */
    }

    void Start()
    {
        for (int i = 0; i < mapName.Count; i++)
        {
            GameObject item = Instantiate(listItemPrefab, listItemHolder);
            item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = mapName[i];
        }
    }

    // void Update()
    // {
        
    // }
}
