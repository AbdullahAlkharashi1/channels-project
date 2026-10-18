namespace Ticketing.Api.Common;

public sealed record PageResponse<T>(
    IReadOnlyList<T> Content,
    int Page,
    int Size,
    long TotalElements,
    int TotalPages)
{
    public static PageResponse<T> Of(IReadOnlyList<T> content, int page, int size, long total) =>
        new(content, page, size, total, size <= 0 ? 0 : (int)Math.Ceiling(total / (double)size));
}
