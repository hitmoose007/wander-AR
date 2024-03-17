using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Transactions;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class TakePhotos : MonoBehaviour
{
    public static TakePhotos instance;
    public GameObject mapImage;
    public GameObject itemPrefab;
    public Transform itemHolder;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void TakePhoto()
    {
        StartCoroutine(TakeAPhoto());
    }

    IEnumerator TakeAPhoto()
    {
        yield return new WaitForEndOfFrame();

        Camera camera = Camera.main;
        int width = Screen.width;
        int height = Screen.height;

        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;

        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(width, height);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        camera.targetTexture = null;

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;

        mapImage = Instantiate(itemPrefab, itemHolder);
        RawImage ri = mapImage.transform.GetComponent<RawImage>();
        ri.texture = image;
        StaticData.MapperSceneMapImage = image;
    }
}
