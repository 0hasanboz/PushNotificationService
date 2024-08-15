using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using Unity.Notifications.Android;
using UnityEngine;

public class AndroidNotificationService : INotificationService
{
    public void Initialize()
    {
        Debug.Log("Android: Initializing notification service");
        CreateGlobalNotificationChannel();
    }

    public async Task<PermissionRequestResult> AskForPermission()
    {
        Debug.Log("Android: Asking for permission");
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
        {
            await Task.Yield();
        }

        return request.Status == PermissionStatus.Allowed
            ? PermissionRequestResult.Allowed
            : PermissionRequestResult.Denied;
    }

    public void RegisterForPush(string playFabId, string pushToken)
    {
        Debug.Log("Android: Registering for push");
        if (string.IsNullOrEmpty(pushToken) || string.IsNullOrEmpty(playFabId))
            return;

        var request = new AndroidDevicePushNotificationRegistrationRequest
        {
            DeviceToken = pushToken,
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

    private void CreateGlobalNotificationChannel()
    {
        Debug.Log("Android: Creating global notification channel");
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
    }
}