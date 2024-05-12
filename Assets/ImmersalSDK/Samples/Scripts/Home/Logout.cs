using UnityEngine;

public class Logout : MonoBehaviour
{
    // Start is called before the first frame update

    public void LogoutUser()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.SignOut();
        StaticData.LoadScene(StaticData.GameScene.LoginScene);
    }

    // Update is called once per frame
}
