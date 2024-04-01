/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using UnityEngine.Events;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using Immersal.AR;
using Immersal.REST;
using UnityEngine.XR.ARSubsystems;
using AOT;

namespace Immersal
{
    public class ImmersalSDK : MonoBehaviour
    {
        public static string sdkVersion = "1.20.0";
        public static bool isHWAR = false;
        private static readonly string[] ServerList = new[]
        {
            "https://api.immersal.com",
            "https://immersal.hexagon.com.cn"
        };

        public enum CameraResolution
        {
            Default,
            HD,
            FullHD,
            Max
        }; // With Huawei AR Engine SDK, only Default (640x480) and Max (1440x1080) are supported.

        public enum APIServer
        {
            DefaultServer,
            ChinaServer
        };

        private static ImmersalSDK instance = null;

        [SerializeField]
        public APIServer defaultServer = APIServer.DefaultServer;

        [Tooltip("SDK developer token")]
        public string developerToken;

        [SerializeField]
        [Tooltip("Application target frame rate")]
        private int m_TargetFrameRate = 60;

        [SerializeField]
        [Tooltip("Android resolution")]
        private CameraResolution m_AndroidResolution = CameraResolution.FullHD;

        [SerializeField]
        [Tooltip("iOS resolution")]
        private CameraResolution m_iOSResolution = CameraResolution.Default;

        [Tooltip("Downsample image to HD resolution")]
        [SerializeField]
        private bool m_Downsample = true;

        public UnityEvent onPoseLost = null;
        public UnityEvent onPoseFound = null;

        public int secondsToDecayPose = 10;

        public LocalizerBase Localizer { get; private set; }
        public int TrackingQuality { get; private set; }

        private ARCameraManager m_CameraManager;
        private ARSession m_ARSession;
        private bool m_bCamConfigDone = false;
        private string m_LocalizationServer;
        private int m_PreviousResults = 0;
        private int m_CurrentResults = 0;
        private int q = 0;
        private float m_LatestPoseUpdated = 0f;
        private bool m_HasPose = false;
        private XRCameraConfiguration? m_InitialConfig;

        public static HttpClient client;

        public int targetFrameRate
        {
            get { return m_TargetFrameRate; }
            set
            {
                m_TargetFrameRate = value;
                SetFrameRate();
            }
        }

        public string defaultServerURL
        {
            get { return ServerList[(int)defaultServer]; }
        }

        public CameraResolution androidResolution
        {
            get { return m_AndroidResolution; }
            set
            {
                m_AndroidResolution = value;
                ConfigureCamera();
            }
        }

        public CameraResolution iOSResolution
        {
            get { return m_iOSResolution; }
            set
            {
                m_iOSResolution = value;
                ConfigureCamera();
            }
        }

        public bool downsample
        {
            get { return m_Downsample; }
            set
            {
                m_Downsample = value;
                SetDownsample();
            }
        }

        public string localizationServer
        {
            get
            {
                if (m_LocalizationServer != null)
                {
                    return m_LocalizationServer;
                }
                return defaultServerURL;
            }
            set { m_LocalizationServer = value; }
        }

        public ARCameraManager cameraManager
        {
            get
            {
                if (m_CameraManager == null)
                {
                    m_CameraManager = UnityEngine.Object.FindObjectOfType<ARCameraManager>();
                }
                return m_CameraManager;
            }
        }

        public ARSession arSession
        {
            get
            {
                if (m_ARSession == null)
                {
                    m_ARSession = UnityEngine.Object.FindObjectOfType<ARSession>();
                }
                return m_ARSession;
            }
        }

        public static ImmersalSDK Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null && !Application.isPlaying)
                {
                    instance = UnityEngine.Object.FindObjectOfType<ImmersalSDK>();
                }
#endif
                if (instance == null)
                {
                    Debug.LogError(
                        "No ImmersalSDK instance found. Ensure one exists in the scene."
                    );
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            if (instance != this)
            {
                Debug.LogError("There must be only one ImmersalSDK object in a scene.");
                UnityEngine.Object.DestroyImmediate(this);
                return;
            }

            if (StaticData.developerToken != null || StaticData.developerToken.Length > 0)
            {
                //for login maybe if needed
                developerToken = StaticData.developerToken;
            }
            else
            {
                developerToken = StaticData.MainAccountDeveloperToken;
            }
            LogCallback callback_delegate = new LogCallback(Log);
            IntPtr intptr_delegate = Marshal.GetFunctionPointerForDelegate(callback_delegate);
            Native.PP_RegisterLogCallback(intptr_delegate);

            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.ExpectContinue = false;

            if (developerToken != null && developerToken.Length > 0)
            {
                PlayerPrefs.SetString("token", developerToken);

                ValidateUser();
            }
        }

        public async void ValidateUser()
        {
            if (
                Application.platform == RuntimePlatform.IPhonePlayer
                || Application.platform == RuntimePlatform.Android
            )
            {
                int r = Core.ValidateUser(developerToken);
                Debug.LogFormat("{0} License", r >= 1 ? "Enterprise" : "Free");
            }
            else
            {
                JobStatusAsync j = new JobStatusAsync();
                j.OnResult += (SDKStatusResult result) =>
                {
                    Debug.LogFormat("{0} License", result.level >= 1 ? "Enterprise" : "Free");
                };
                await j.RunJobAsync();
            }
        }

        void Start()
        {
            SetFrameRate();
#if !UNITY_EDITOR
            SetDownsample();
#endif
            onPoseLost?.Invoke();
        }

        [MonoPInvokeCallback(typeof(LogCallback))]
        public static void Log(IntPtr ansiString)
        {
            string msg = Marshal.PtrToStringAnsi(ansiString);
            Debug.LogFormat("Plugin: {0}", msg);
        }

        private void SetFrameRate()
        {
            Application.targetFrameRate = targetFrameRate;
        }

        private void SetDownsample()
        {
            if (downsample)
            {
                Core.SetInteger("LocalizationMaxPixels", 960 * 720);
            }
            else
            {
                Core.SetInteger("LocalizationMaxPixels", 0);
            }
        }

        private void Update()
        {
            if (Localizer != null)
            {
                LocalizerStats stats = Localizer.stats;
                if (stats.localizationAttemptCount > 0)
                {
                    q = CurrentResults(stats.localizationSuccessCount);

                    if (!m_HasPose && q > 1)
                    {
                        m_HasPose = true;
                        onPoseFound?.Invoke();
                    }

                    if (m_HasPose && (q < 1 || !Localizer.isTracking))
                    {
                        m_HasPose = false;
                        Localizer.Reset();
                        m_PreviousResults = 0;
                        m_CurrentResults = 0;
                        onPoseLost?.Invoke();
                    }

                    TrackingQuality = q;
                }
            }

            if (!isHWAR)
            {
                if (!m_bCamConfigDone && cameraManager != null)
                    ConfigureCamera();
            }
        }

        private void ConfigureCamera()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            var cameraSubsystem = cameraManager.subsystem;
            if (cameraSubsystem == null || !cameraSubsystem.running)
                return;
            var configurations = cameraSubsystem.GetConfigurations(Allocator.Temp);
            if (!configurations.IsCreated || (configurations.Length <= 0))
                return;
            int bestError = int.MaxValue;
            var currentConfig = cameraSubsystem.currentConfiguration;
            int dw = (int)currentConfig?.width;
            int dh = (int)currentConfig?.height;
            if (dw == 0 && dh == 0)
                return;
#if UNITY_ANDROID
            CameraResolution reso = androidResolution;
#else
			CameraResolution reso = iOSResolution;
#endif

            if (!m_bCamConfigDone)
            {
                m_InitialConfig = currentConfig;
            }

            switch (reso)
            {
                case CameraResolution.Default:
                    dw = (int)currentConfig?.width;
                    dh = (int)currentConfig?.height;
                    break;
                case CameraResolution.HD:
                    dw = 1280;
                    dh = 720;
                    break;
                case CameraResolution.FullHD:
                    dw = 1920;
                    dh = 1080;
                    break;
                case CameraResolution.Max:
                    dw = 80000;
                    dh = 80000;
                    break;
            }

            foreach (var config in configurations)
            {
                int perror = config.width * config.height - dw * dh;
                if (Math.Abs(perror) < bestError)
                {
                    bestError = Math.Abs(perror);
                    currentConfig = config;
                }
            }

            if (reso != CameraResolution.Default)
            {
                Debug.LogFormat(
                    "resolution = {0}x{1}",
                    (int)currentConfig?.width,
                    (int)currentConfig?.height
                );
                cameraSubsystem.currentConfiguration = currentConfig;
            }
            else
            {
                cameraSubsystem.currentConfiguration = m_InitialConfig;
            }
#endif
            m_bCamConfigDone = true;
        }

        int CurrentResults(int localizationResults)
        {
            int diffResults = localizationResults - m_PreviousResults;
            m_PreviousResults = localizationResults;
            if (diffResults > 0)
            {
                m_LatestPoseUpdated = Time.time;
                m_CurrentResults += diffResults;
                if (m_CurrentResults > 3)
                {
                    m_CurrentResults = 3;
                }
            }
            else if (Time.time - m_LatestPoseUpdated > secondsToDecayPose)
            {
                m_LatestPoseUpdated = Time.time;
                if (m_CurrentResults > 0)
                {
                    m_CurrentResults--;
                }
            }

            return m_CurrentResults;
        }

        public void RegisterLocalizer(LocalizerBase localizer)
        {
            Localizer = localizer;
            Localizer.OnReset += OnLocalizerReset;
        }

        public void UnRegisterLocalizer()
        {
            Localizer.OnReset -= OnLocalizerReset;
            Localizer = null;
        }

        private void OnLocalizerReset()
        {
            m_CurrentResults = m_PreviousResults = 0;
            m_HasPose = false;
        }
    }
}
