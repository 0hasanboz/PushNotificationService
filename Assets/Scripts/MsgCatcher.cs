using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Unity.Notifications.Android;
using UnityEngine;

public class MsgCatcher : MonoBehaviour
{
    private const string k_androidChannel = "DEFAULT_CHANNEL_DW";

    public string pushToken;
    public string playFabId;
    public string lastMsg;

    public TMP_Text tokenText, msgText;

    private FirebaseApp app;

    private void OnPfFail(PlayFabError error)
    {
        Debug.Log("PlayFab: api error: " + error.GenerateErrorReport());
    }

    public async void Start()
    {
        LoginToPlayFab();
        CreateAndroidChannel();

        await Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;
                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        await RequestAuthorization();
    }

    public async Task RequestAuthorization()
    {
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
        {
            await Task.Yield();
        }

        Debug.Log("RequestAuthorization: " + request.Status);
        if (request.Status == PermissionStatus.Allowed)
        {
        }
    }

    private void LoginToPlayFab()
    {
#if UNITY_ANDROID
        var request = new LoginWithAndroidDeviceIDRequest
            { AndroidDeviceId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true, };
        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnPfLogin, OnPfFail);
#endif
    }

    private void OnPfLogin(LoginResult result)
    {
        Debug.Log("PlayFab: login successful");
        playFabId = result.PlayFabId;
        RegisterForPush();
    }

    private void RegisterForPush()
    {
        if (string.IsNullOrEmpty(pushToken) || string.IsNullOrEmpty(playFabId))
            return;

#if UNITY_ANDROID
        var request = new AndroidDevicePushNotificationRegistrationRequest
        {
            DeviceToken = pushToken,
            SendPushNotificationConfirmation = true,
            ConfirmationMessage = "Push notifications registered successfully"
        };
        PlayFabClientAPI.AndroidDevicePushNotificationRegistration(request, OnPfAndroidReg, OnPfFail);
#endif
    }

    private void OnPfAndroidReg(AndroidDevicePushNotificationRegistrationResult result)
    {
        Debug.Log("PlayFab: Push Registration Successful");
    }

    private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        Debug.Log("PlayFab: Received Registration Token: " + token.Token);
        pushToken = token.Token;
        tokenText.text = pushToken;
        RegisterForPush();
    }

    private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        Debug.Log("PlayFab: Received a new message from: " + e.Message.From);
        lastMsg = "";
        if (e.Message.Data != null)
        {
            lastMsg += "DATA: " + e.Message.Data.First() + "\n";
            Debug.Log("PlayFab: Received a message with data:");
            foreach (var pair in e.Message.Data)
                Debug.Log("PlayFab data element: " + pair.Key + "," + pair.Value);
        }

        if (e.Message.Notification != null)
        {
            Debug.Log("PlayFab: Received a notification:");
            lastMsg += "TITLE: " + e.Message.Notification.Title + "\n";
            lastMsg += "BODY: " + e.Message.Notification.Body + "\n";
        }

        msgText.text = lastMsg;
    }

    private void CreateAndroidChannel()
    {
        var group = new AndroidNotificationChannelGroup()
        {
            Id = "Main",
            Name = "Main notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannelGroup(group);
        var channel = new AndroidNotificationChannel()
        {
            Id = k_androidChannel,
            Name = "Default Channel",
            Importance = Importance.High,
            Description = "Generic notifications",
            Group = "Main", // must be same as Id of previously registered group
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }
}