using System.Threading.Tasks;
using Firebase.Messaging;
using UnityEngine;

public class NotificationController
{
    private readonly INotificationService _notificationService;
    private readonly string _playFabId;
    private string _pushToken;

    public NotificationController(INotificationService notificationService, string playFabId)
    {
        _notificationService = notificationService;
        _playFabId = playFabId;
    }

    public async Task Initialize()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        var permissionResult = await _notificationService.AskForPermission();
        Debug.Log("Permission result: " + permissionResult);
        if (permissionResult == PermissionRequestResult.Denied) return;
        _notificationService.Initialize();
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        _pushToken = token.Token;
        _notificationService.RegisterForPush(_playFabId, _pushToken);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("PlayFab: Received a new message from: " + e.Message.From);
        var messageText = "";
        if (e.Message.Data != null)
        {
            foreach (var pair in e.Message.Data)
            {
                messageText += pair.Key + ": " + pair.Value + "\n";
            }
        }

        if (e.Message.Notification != null)
        {
            messageText += e.Message.Notification.Title + ": " + e.Message.Notification.Body;
        }

        Debug.Log("PlayFab: Received a message: " + messageText);
    }
}