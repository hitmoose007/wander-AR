/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Immersal.REST;
using UnityEngine.UI;

namespace Immersal.Samples.Mapping
{
    public class LoginManager : MonoBehaviour
    {
        public GameObject loginPanel;
        public TMP_InputField emailField;
        public TMP_InputField passwordField;
        public TMP_InputField serverField;
        public TextMeshProUGUI loginErrorText;
        public float fadeOutTime = 1f;
        public event LoginEvent OnLogin = null;
        public event LoginEvent OnLogout = null;
        public delegate void LoginEvent();

        public string email = null;
        public string password = null;
        private IEnumerator m_FadeAlpha;
        private CanvasGroup m_CanvasGroup;
        private ImmersalSDK m_Sdk;

        private bool m_autoLoginEnabled;

        [SerializeField]
        private Toggle m_rememberMeToggle;

        [SerializeField]
        private GameObject m_loginControlContainer;

        [SerializeField]
        private TMP_Text m_autoLoggingIndicatorText;

        private const string IpApiUrl = "https://ipinfo.io?token=437904fba78a27";

        private static LoginManager instance = null;

        public static LoginManager Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null && !Application.isPlaying)
                {
                    instance = UnityEngine.Object.FindObjectOfType<LoginManager>();
                }
#endif
                if (instance == null)
                {
                    Debug.LogError(
                        "No LoginManager instance found. Ensure one exists in the scene."
                    );
                }
                return instance;
            }
        }

        void Awake()
        {
            if (!PlayerPrefs.HasKey("sdkversion"))
            {
                PlayerPrefs.DeleteKey("server");
            }
            else
            {
                if (!PlayerPrefs.GetString("sdkversion").Equals(ImmersalSDK.sdkVersion))
                {
                    PlayerPrefs.DeleteKey("server");
                }
            }

            PlayerPrefs.SetString("sdkversion", ImmersalSDK.sdkVersion);

            if (instance == null)
            {
                instance = this;
            }
            if (instance != this)
            {
                Debug.LogError("There must be only one LoginManager object in a scene.");
                UnityEngine.Object.DestroyImmediate(this);
                return;
            }
        }

        void Start()
        {
            m_Sdk = ImmersalSDK.Instance;
            m_CanvasGroup = loginPanel.GetComponent<CanvasGroup>();

            // Invoke("FillFields", 0.1f);
             CompleteLogin();
        // Login();
        }

        

        private void CompleteLogin()
        {
            loginErrorText.gameObject.SetActive(false);
            m_Sdk.ValidateUser();

            FadeOut();

            OnLogin?.Invoke();
        }

        private void FadeOut()
        {
            if (m_FadeAlpha != null)
            {
                StopCoroutine(m_FadeAlpha);
            }
            m_FadeAlpha = FadeAlpha();
            StartCoroutine(m_FadeAlpha);
        }

        IEnumerator FadeAlpha()
        {
            m_CanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.1f);
            while (m_CanvasGroup.alpha > 0)
            {
                m_CanvasGroup.alpha -= Time.deltaTime / fadeOutTime;
                yield return null;
            }
            loginPanel.SetActive(false);
            yield return null;
        }

    }
}
