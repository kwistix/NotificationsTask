namespace NotificationsLibrary;

public interface INotificationService
{
    IEnumerable<Notification> FilterAndSort(
        IEnumerable<Notification> notifications,
        NotificationFilterOptions options);
}

public class NotificationService : INotificationService
{
    public IEnumerable<Notification> FilterAndSort(
        IEnumerable<Notification> notifications,
        NotificationFilterOptions options)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(options);

        var filtered = ApplyFilters(notifications, options);
        var sorted = ApplySorting(filtered, options);

        return sorted;
    }

    private static IEnumerable<Notification> ApplyFilters(
        IEnumerable<Notification> query,
        NotificationFilterOptions options)
    {
        if (options.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == options.IsRead.Value);
        }

        if (options.Types is { Length: > 0 })
        {
            query = query.Where(n => options.Types.Contains(n.Type));
        }

        if (options.MinPriority.HasValue)
        {
            query = query.Where(n => n.Priority >= options.MinPriority.Value);
        }

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            query = query.Where(n =>
                (n.Title?.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (n.Content?.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return query;
    }

    private static IEnumerable<Notification> ApplySorting(
        IEnumerable<Notification> query,
        NotificationFilterOptions options)
    {
        if (!options.SortBy.HasValue)
        {
            return query;
        }

        return options.SortBy.Value switch
        {
            SortNotificationBy.Date => options.Descending
                ? query.OrderByDescending(n => n.CreatedAt)
                : query.OrderBy(n => n.CreatedAt),

            SortNotificationBy.Priority => options.Descending
                ? query.OrderByDescending(n => n.Priority)
                : query.OrderBy(n => n.Priority),

            SortNotificationBy.Title => options.Descending
                ? query.OrderByDescending(n => n.Title, StringComparer.OrdinalIgnoreCase)
                : query.OrderBy(n => n.Title, StringComparer.OrdinalIgnoreCase),

            _ => query
        };
    }
}
