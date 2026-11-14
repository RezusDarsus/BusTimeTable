using System.Net.Http;

namespace GeorgiaTransitIntelligence.Mapping;

internal sealed class StreetMapTiles : IDisposable
{
    private const int CacheLimit = 144;
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(8) };
    private readonly string cacheFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GeorgiaTransitIntelligence", "MapTiles");
    private readonly Dictionary<Tile, Bitmap> images = new();
    private readonly Dictionary<Tile, DateTime> retryAfter = new();
    private readonly System.Windows.Forms.Timer fetchTimer = new() { Interval = 220 };
    private readonly Action redraw;
    private readonly string? yandexKey;
    internal string? Failure { get; private set; }
    internal bool KeyRejected { get; private set; }
    private Tile[] visible = [];
    private CancellationTokenSource? fetchCancellation;

    internal readonly record struct Tile(int Zoom, int X, int Y);

    internal StreetMapTiles(Action redraw, string? yandexKey = null)
    {
        this.redraw = redraw;
        this.yandexKey = yandexKey;
        fetchTimer.Tick += FetchVisible;
    }

    internal bool Draw(Graphics graphics, Rectangle area, double lat, double lon, double zoom)
    {
        int level = (int)Math.Floor(zoom);
        double tileSize = 256 * Math.Pow(2, zoom - level);
        int tileCount = 1 << level;
        float left = (float)(LongitudeX(lon) * tileCount - area.Width / (2 * tileSize));
        float top = (float)(LatitudeY(lat) * tileCount - area.Height / (2 * tileSize));

        var tiles = new List<Tile>();
        bool hasTiles = false;
        for (int y = Math.Max(0, (int)Math.Floor(top)); y < tileCount && y < top + area.Height / tileSize; y++)
        {
            for (int x = Math.Max(0, (int)Math.Floor(left)); x < tileCount && x < left + area.Width / tileSize; x++)
            {
                var tile = new Tile(level, x, y);
                tiles.Add(tile);
                if (!images.TryGetValue(tile, out Bitmap? image)) continue;
                int px = (int)Math.Round((x - left) * tileSize);
                int py = (int)Math.Round((y - top) * tileSize);
                int right = (int)Math.Round((x + 1 - left) * tileSize);
                int bottom = (int)Math.Round((y + 1 - top) * tileSize);
                graphics.DrawImage(image, new Rectangle(px, py, right - px, bottom - py));
                hasTiles = true;
            }
        }

        if (!visible.SequenceEqual(tiles))
        {
            visible = tiles.ToArray();
            fetchCancellation?.Cancel();
            fetchTimer.Stop();
            fetchTimer.Start();
        }
        return hasTiles;
    }

    internal void Hide()
    {
        fetchTimer.Stop();
        fetchCancellation?.Cancel();
        visible = [];
    }

    private async void FetchVisible(object? sender, EventArgs e)
    {
        fetchTimer.Stop();
        fetchCancellation?.Cancel();
        using var cancellation = new CancellationTokenSource();
        fetchCancellation = cancellation;

        try
        {
            foreach (var tile in visible)
            {
                if (cancellation.IsCancellationRequested || Failure is not null) return;
                if (images.ContainsKey(tile) ||
                    (retryAfter.TryGetValue(tile, out DateTime retry) && retry > DateTime.UtcNow)) continue;

                Bitmap? bitmap = await LoadAsync(tile, cancellation.Token);
                if (cancellation.IsCancellationRequested)
                {
                    bitmap?.Dispose();
                    return;
                }

                if (bitmap is null)
                {
                    retryAfter[tile] = DateTime.UtcNow.AddMinutes(1);
                    redraw();
                    continue;
                }
                retryAfter.Remove(tile);
                images[tile] = bitmap;
                if (images.Count > CacheLimit)
                {
                    foreach (Tile unused in images.Keys.Where(key => !visible.Contains(key)).Take(images.Count - CacheLimit).ToArray())
                    {
                        images[unused].Dispose();
                        images.Remove(unused);
                    }
                }
                redraw();
            }
        }
        finally
        {
            if (fetchCancellation == cancellation) fetchCancellation = null;
        }
    }

    private async Task<Bitmap?> LoadAsync(Tile tile, CancellationToken cancellation)
    {
        try
        {
            if (yandexKey is not null)
            {
                using var response = await Client.GetAsync(CreateYandexTileUri(tile, yandexKey), cancellation);
                cancellation.ThrowIfCancellationRequested();
                if (!response.IsSuccessStatusCode)
                {
                    KeyRejected = response.StatusCode is System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized;
                    Failure = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized =>
                            "Yandex rejected the key. Switch maps and reselect Yandex to enter a Tiles API key.",
                        System.Net.HttpStatusCode.TooManyRequests => "Yandex rate limit reached. Switch maps and try again later.",
                        _ => $"Yandex tiles unavailable (HTTP {(int)response.StatusCode})."
                    };
                    return null;
                }

                // Yandex tiles are kept in memory for the active view only.
                using var imageStream = await response.Content.ReadAsStreamAsync(cancellation);
                using var tileImage = Image.FromStream(imageStream);
                return new Bitmap(tileImage);
            }

            Directory.CreateDirectory(cacheFolder);
            string path = Path.Combine(cacheFolder, $"{tile.Zoom}-{tile.X}-{tile.Y}.png");
            byte[] bytes;
            if (File.Exists(path) && File.GetLastWriteTimeUtc(path) > DateTime.UtcNow)
            {
                bytes = await File.ReadAllBytesAsync(path, cancellation);
            }
            else
            {
                using var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://tile.openstreetmap.org/{tile.Zoom}/{tile.X}/{tile.Y}.png");
                request.Headers.UserAgent.ParseAdd("GeorgiaTransitIntelligence/1.0");
                using var response = await Client.SendAsync(request, cancellation);
                if (!response.IsSuccessStatusCode) return null;

                bytes = await response.Content.ReadAsByteArrayAsync(cancellation);
                if (response.Headers.CacheControl?.NoStore != true && response.Headers.CacheControl?.NoCache != true)
                {
                    var now = DateTimeOffset.UtcNow;
                    TimeSpan? maxAge = response.Headers.CacheControl?.MaxAge;
                    var expires = maxAge.HasValue
                        ? now + maxAge.Value - (response.Headers.Age ?? TimeSpan.Zero)
                        : response.Content.Headers.Expires ?? now.AddDays(7);
                    await File.WriteAllBytesAsync(path, bytes, cancellation);
                    File.SetLastWriteTimeUtc(path, expires.UtcDateTime);
                }
            }
            using var stream = new MemoryStream(bytes);
            using var decoded = Image.FromStream(stream);
            return new Bitmap(decoded);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or IOException
            or UnauthorizedAccessException or ArgumentException)
        {
            if (yandexKey is not null && !cancellation.IsCancellationRequested)
                Failure = "Yandex tiles could not load. Check the connection and switch maps to retry.";
            return null;
        }
    }

    private static Uri CreateYandexTileUri(Tile tile, string key) => new(
        $"https://tiles.api-maps.yandex.ru/v1/tiles/?x={tile.X}&y={tile.Y}&z={tile.Zoom}" +
        $"&lang=en_US&l=map&projection=web_mercator&maptype=transit&apikey={Uri.EscapeDataString(key)}");

    internal static double LongitudeX(double longitude) => (longitude + 180) / 360;
    internal static double LatitudeY(double latitude) =>
        (1 - Math.Asinh(Math.Tan(latitude * Math.PI / 180)) / Math.PI) / 2;
    internal static double YLatitude(double y) => Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y))) * 180 / Math.PI;

    public void Dispose()
    {
        Hide();
        fetchTimer.Dispose();
        foreach (var image in images.Values) image.Dispose();
        images.Clear();
    }
}
