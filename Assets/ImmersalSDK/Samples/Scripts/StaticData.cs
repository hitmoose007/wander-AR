using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StaticData : MonoBehaviour
{
    // All static variables stored in this script
    // and can be accessed from any other script or scene
    public static string userEmail = "poop@gmail.com";
    public static string MainAccountDeveloperToken =
        "0069cd762bf3cd46acc1dd97c526dd6fece87c81550db3d92b0cc1927a962aaf";
    public static string developerToken =
        "0069cd762bf3cd46acc1dd97c526dd6fece87c81550db3d92b0cc1927a962aaf";
    public static string MapperSceneMapName;

    public static Texture2D MapperSceneMapImage;
    public static bool MapperSceneIsMapPrivate = false;
    public static int MapIdContentPlacement;
    public static bool ContentSceneIsMapPrivate = false;

    public enum GameScene
    {
        ContentPlacementScene, // Scene for placing content
        MapperScene, // Scene for mapping
        HomeScene, // Scene for home
    }

    public static void LoadScene(GameScene gameScene)
    {
        switch (gameScene)
        {
            case GameScene.ContentPlacementScene:
                SceneManager.LoadScene("ContentPlacementSample");
                break;
            case GameScene.MapperScene:
                SceneManager.LoadScene("MappingApp");
                break;
            case GameScene.HomeScene:
                SceneManager.LoadScene("MapDownload");
                break;
            default:
                Debug.LogError("Scene not recognized!");
                break;
        }
    }
}
