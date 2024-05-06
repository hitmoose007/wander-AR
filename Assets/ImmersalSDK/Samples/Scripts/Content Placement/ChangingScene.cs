using System.Collections;
using System.Collections.Generic;
using Immersal.REST;
using UnityEngine;

public class ChangingScene : MonoBehaviour
{

    public void OnBackButtonClick()
    {
        StaticData.LoadScene(StaticData.GameScene.HomeScene);   
    }
}

