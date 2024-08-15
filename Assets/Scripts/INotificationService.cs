using System.Threading.Tasks;

public interface INotificationService
{
    public void Initialize();
    public void RegisterForPush(string playFabId, string pushToken);
    public Task<PermissionRequestResult> AskForPermission();
}

public enum PermissionRequestResult
{
    Allowed,
    Denied
}