using System.Threading.Tasks;

public interface INotificationService
{
    public void Initialize();
    public Task<PermissionRequestResult> AskForPermission();
}

public enum PermissionRequestResult
{
    Allowed,
    Denied
}