namespace EmoMeter.Web.Requests
{
    public record GetUserEventsRequest(
        int Page,
        int PageSize,
        DateTime? Start = null,
        DateTime? End = null);
}
