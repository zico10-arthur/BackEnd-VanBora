namespace VanBora.Application.Settings;

public class CorsSettings
{
    public const string SectionName = "Cors";

    /// <summary>Origens do frontend (ex.: http://localhost:3000).</summary>
    public string[] AllowedOrigins { get; set; } = ["http://localhost:3000"];
}
