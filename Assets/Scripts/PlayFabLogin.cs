using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabLogin : MonoBehaviour
{
    string playFabId;

    private void Start()
    {
        LoginToPlayFab();
    }

    private void LoginToPlayFab()
    {
#if UNITY_ANDROID
        var request = new LoginWithAndroidDeviceIDRequest
            { AndroidDeviceId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true, };
        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnPfLogin, OnPfFail);
#endif
#if UNITY_IOS
        var request = new LoginWithIOSDeviceIDRequest
            { DeviceId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true, };
        PlayFabClientAPI.LoginWithIOSDeviceID(request, OnPfLogin, OnPfFail);

#endif
    }

    private void OnPfFail(PlayFabError obj)
    {
        Debug.LogError("PlayFab: " + obj.GenerateErrorReport());
    }

    private void OnPfLogin(LoginResult obj)
    {
        Debug.Log("PlayFab: login successful");
        playFabId = obj.PlayFabId;
        CreateNotificationController();
    }

    private void CreateNotificationController()
    {
        INotificationService notificationService;

#if UNITY_ANDROID
        notificationService = new AndroidNotificationService();
#endif
#if UNITY_IOS
        notificationService = new IOSNotificationService();
#endif

        NotificationController notificationController = new NotificationController(notificationService, playFabId);
        notificationController.Initialize();
    }
}