#nullable enable
namespace MeetingItemsApp.Common.Abstractions;

public interface IUserSessionProvider
{
    string? GetUserId();
    string? GetUserName();
    string? GetUserEmail();
    bool IsAuthenticated();
}
