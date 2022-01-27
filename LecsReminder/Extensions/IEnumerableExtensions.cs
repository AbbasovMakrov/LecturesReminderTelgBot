namespace LecsReminder.Extensions;

public static class IEnumerableExtensions
{
    public static void Each<T>(this IEnumerable<T> items,Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    public static async Task EachAsync<T>(this IEnumerable<T> items, Action<T> action)
    {
        await Task.Run(() =>
        {
            foreach (var item in items)
            {
                action(item);
            }
        });
    }
}