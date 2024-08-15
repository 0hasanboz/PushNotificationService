using System.Threading.Tasks;
using Firebase.Messaging;
using PlayFab;
using PlayFab.ClientModels;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
using UnityEngine;

public class AndroidNotificationService : INotificationService
{
    public void Initialize()
    {
        Debug.Log("Android: Initializing notification service");
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        CreateGlobalNotificationChannel();
    }

    public async Task<PermissionRequestResult> AskForPermission()
    {
        Debug.Log("Android: Asking for permission");
#if UNITY_ANDROID
 var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
        {
            await Task.Yield();
        }

        return request.Status == PermissionStatus.Allowed
            ? PermissionRequestResult.Allowed
            : PermissionRequestResult.Denied;
#endif
        return PermissionRequestResult.Denied;
    }

    private void RegisterForPush(string deviceToken)
    {
        Debug.Log("Android: Registering for push");
        if (string.IsNullOrEmpty(deviceToken))
            return;

        var request = new AndroidDevicePushNotificationRegistrationRequest
        {
            DeviceToken = deviceToken,
            SendPushNotificationConfirmation = true,
            ConfirmationMessage = "Push notifications registered successfully"
        };
        PlayFabClientAPI.AndroidDevicePushNotificationRegistration(request, OnPfAndroidReg, OnPfFail);
    }

    private void OnPfAndroidReg(AndroidDevicePushNotificationRegistrationResult result)
    {
        Debug.Log("PlayFab: Android push registration successful");
    }

    private void OnPfFail(PlayFabError error)
    {
        Debug.LogError("PlayFab: " + error.GenerateErrorReport());
    }
    
    private void OnTokenReceived(object sender, TokenReceivedEventArgs eventArg)
    {
        RegisterForPush(eventArg.Token);
    }

    private void CreateGlobalNotificationChannel()
    {
        Debug.Log("Android: Creating global notification channel");
#if UNITY_ANDROID
         var group = new AndroidNotificationChannelGroup()
                {
                    Id = "global_group",
                    Name = "Global Notification Group"
                };
                AndroidNotificationCenter.RegisterNotificationChannelGroup(group);
        
                var channel = new AndroidNotificationChannel()
                {
                    Id = "global_channel",
                    Name = "Global Channel",
                    Importance = Importance.High,
                    Description = "Global Notification Channel",
                    Group = group.Id
                };
                AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
       
    }
}