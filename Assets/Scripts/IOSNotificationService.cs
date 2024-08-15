using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;

public class IOSNotificationService : INotificationService
{
    public void Initialize()
    {
        Debug.Log("iOS: Initializing notification service");
    }

    public async Task<PermissionRequestResult> AskForPermission()
    {
        Debug.Log("iOS: Asking for permission");
#if UNITY_IOS
 var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
        using var request = new AuthorizationRequest(authorizationOption, true);
        while (!request.IsFinished)
        {
            await Task.Yield();
        }

        return request.Granted ? PermissionRequestResult.Allowed : PermissionRequestResult.Denied;
#endif
        return PermissionRequestResult.Denied;
    }

    public void RegisterForPush(string playFabId, string pushToken)
    {
        Debug.Log("iOS: Registering for push");
        if (pushToken != null)
        {
            RegisterForIOSPushNotificationRequest request = new RegisterForIOSPushNotificationRequest();
            request.DeviceToken = pushToken;
            PlayFabClientAPI.RegisterForIOSPushNotification(request, OnPlayFabReg, OnPlayFabError);
        }
        else
        {
            Debug.Log("Push Token was null!");
        }
    }

    private void OnPlayFabReg(RegisterForIOSPushNotificationResult obj)
    {
        Debug.Log("PlayFab: iOS push registration successful");
    }

    private void OnPlayFabError(PlayFabError obj)
    {
        Debug.LogError("PlayFab: " + obj.GenerateErrorReport());
    }
}