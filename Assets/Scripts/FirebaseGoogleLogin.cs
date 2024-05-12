using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.UI;
using Google;
using System.Net.Http;
using UnityEngine.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using Immersal.REST;
using System.IO;

public class FirebaseGoogleLogin : MonoBehaviour
{
    // It's required for google signin and we can find this api key from firebase here
    public string GoogleWebAPI =
        "335058442868-dd3a18s18mcd23d0hpeml2gvsi0ksr6s.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;
    private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    private FirebaseAuth auth;
    private FirebaseUser user;

    private bool newUser;
    public GameObject LoginScreen,
        ProfileScreen;

    void Awake()
    {
        // Configure webAPI key with google
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = GoogleWebAPI,
            RequestIdToken = true
        };
    }

    void ClearDataDirectory()
    {
        string path = Application.persistentDataPath; // Path where data is stored

        if (Directory.Exists(path))
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }

    void Start()
    {
        ClearDataDirectory();
        PlayerPrefs.DeleteAll();

        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    InitFirebase();
                }
                else
                {
                    Debug.LogError(
                        "Could not resolve all Firebase dependencies: " + dependencyStatus
                    );
                }
            });
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public async void GoogleSignInClickAsync()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        GoogleSignInUser signedInUser = await GoogleSignIn.DefaultInstance.SignIn();

        await OnGoogleAuthenticatedFinished(signedInUser);
    }

    async Task OnGoogleAuthenticatedFinished(GoogleSignInUser signInUser)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(
            signInUser.IdToken,
            null
        );
        user = await auth.SignInWithCredentialAsync(credential);
        try
        {
            DocumentReference userRef = db.Collection("user").Document(user.UserId);
            DocumentSnapshot ExistingUserDoc = await userRef.GetSnapshotAsync();
            newUser = !ExistingUserDoc.Exists;

            // Proceed with updating the UI
            LoginScreen.SetActive(false);
            ProfileScreen.SetActive(true);

            if (newUser)
            {
                GoogleSignIn.DefaultInstance.SignOut();
                string randomPassword = StrongPasswordGenerator.Generate(8);
                JobRegisterAsync j = new JobRegisterAsync();
                j.email = user.Email;
                j.password = randomPassword;
                j.username = user.DisplayName;

                j.OnResult += async (result) =>
                {
                    await db.Collection("user")
                        .Document(user.UserId)
                        .SetAsync(
                            new
                            {
                                name = user.DisplayName,
                                email = user.Email,
                                token = result.token,
                                password = randomPassword
                            }
                        ); // Add user to database
                    StaticData.developerToken = result.token;
                    StaticData.userEmail = user.Email;
                };
                await j.RunJobAsync();
            }
            else
            {
                DocumentSnapshot userDoc = await db.Collection("user")
                    .Document(user.UserId)
                    .GetSnapshotAsync();
                StaticData.developerToken = userDoc.GetValue<string>("token");
                StaticData.userEmail = userDoc.GetValue<string>("email");
            }

            GoogleSignIn.DefaultInstance.SignOut();
            StaticData.LoadScene(StaticData.GameScene.HomeScene);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public class StrongPasswordGenerator
    {
        private static readonly char[] UppercaseLetters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] LowercaseLetters =
            "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] Digits = "0123456789".ToCharArray();

        private static readonly char[] AllCharacters = UppercaseLetters
            .Concat(LowercaseLetters)
            .Concat(Digits)
            .ToArray();

        public static string Generate(int length)
        {
            var random = new System.Random();
            var passwordChars = new List<char>();

            // Ensure the password contains at least one character of each type
            passwordChars.Add(UppercaseLetters[random.Next(UppercaseLetters.Length)]);
            passwordChars.Add(LowercaseLetters[random.Next(LowercaseLetters.Length)]);
            passwordChars.Add(Digits[random.Next(Digits.Length)]);

            // Fill the rest of the password length with random characters from all available characters
            for (int i = passwordChars.Count; i < length; i++)
            {
                passwordChars.Add(AllCharacters[random.Next(AllCharacters.Length)]);
            }

            // Shuffle the characters to avoid predictable patterns
            var shuffledPassword = passwordChars.OrderBy(x => random.Next()).ToArray();

            return new string(shuffledPassword);
        }
    }
}
