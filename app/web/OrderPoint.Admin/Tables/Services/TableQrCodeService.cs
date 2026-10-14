using System.Collections.Concurrent;
using QRCoder;

namespace OrderPoint.Admin.Tables.Services;

// QR codes open the customer app at /t/{code}; CustomerApp:PublicUrl overrides Aspire's address for real phones
internal sealed class TableQrCodeService(IConfiguration configuration)
{
    private const string CustomerServiceName = "order-point-customer";

    private const int PixelsPerModule = 8;

    // Scanners need dark modules on a light background, whatever the theme
    private const string DarkColor = "#000000";

    private const string LightColor = "#ffffff";

    private readonly ConcurrentDictionary<string, string> _svgsByUrl = new();

    // Null when no address is configured, so no QR code can point anywhere
    internal string? GetTableUrl(string tableCode)
    {
        string? baseUrl = GetCustomerAppUrl();

        return baseUrl is null ? null : $"{baseUrl.TrimEnd('/')}/t/{Uri.EscapeDataString(tableCode)}";
    }

    // Scales with its container (viewBox, no fixed size)
    internal string GetSvg(string url)
    {
        return _svgsByUrl.GetOrAdd(url, CreateSvg);
    }

    private string? GetCustomerAppUrl()
    {
        string? publicUrl = configuration["CustomerApp:PublicUrl"];

        if (!string.IsNullOrWhiteSpace(publicUrl))
        {
            return publicUrl;
        }

        return configuration[$"services:{CustomerServiceName}:https:0"]
            ?? configuration[$"services:{CustomerServiceName}:http:0"];
    }

    private static string CreateSvg(string url)
    {
        using QRCodeGenerator generator = new();
        using QRCodeData data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using SvgQRCode qrCode = new(data);

        return qrCode.GetGraphic(
            PixelsPerModule,
            DarkColor,
            LightColor,
            drawQuietZones: true,
            sizingMode: SvgQRCode.SizingMode.ViewBoxAttribute);
    }
}