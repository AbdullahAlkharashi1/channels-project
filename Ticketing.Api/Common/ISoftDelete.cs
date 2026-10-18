namespace Ticketing.Api.Common;

public interface ISoftDelete
{
    DateTimeOffset? RemovedAt { get; set; }
}
