using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdminModels
{
    [JsonConverter(typeof(JsonStringEnumConverter<RuoloUtente>))]
    public enum RuoloUtente
    {
        [JsonStringEnumMemberName("miciomane")]
        Miciomane,
        [JsonStringEnumMemberName("admin")]
        Admin,
        [JsonStringEnumMemberName("player")]
        Player,
        [JsonStringEnumMemberName("user")]
        User
    }

    public sealed record PermessiUtente
    {
        [JsonPropertyName("leggereManga")]
        public bool leggereManga { get; init; }

        [JsonPropertyName("aggiungereManga")]
        public bool aggiungereManga { get; init; }

        [JsonPropertyName("ascoltareCanzoni")]
        public bool ascoltareCanzoni { get; init; }

        [JsonPropertyName("aggiungereCanzoni")]
        public bool aggiungereCanzoni { get; init; }

        [JsonPropertyName("accessoGiochiMiciomani")]
        public bool accessoGiochiMiciomani { get; init; }

        [JsonPropertyName("visualizzaClassifica")]
        public bool visualizzaClassifica { get; init; }

        [JsonPropertyName("visualizzaRuoliUtente")]
        public bool visualizzaRuoliUtente { get; init; }

        [JsonPropertyName("modificaRuoliUtente")]
        public bool modificaRuoliUtente { get; init; }

        [JsonPropertyName("modificaCronoJob")]
        public bool modificaCronoJob { get; init; }

        public static PermessiUtente ForRole(RuoloUtente ruolo) => ruolo switch
        {
            RuoloUtente.User => new PermessiUtente
            {
                leggereManga = true,
                ascoltareCanzoni = true,
                accessoGiochiMiciomani = true
            },
            RuoloUtente.Player => new PermessiUtente
            {
                leggereManga = true,
                ascoltareCanzoni = true,
                accessoGiochiMiciomani = true,
                visualizzaClassifica = true,
                visualizzaRuoliUtente = true
            },
            RuoloUtente.Admin => new PermessiUtente
            {
                leggereManga = true,
                aggiungereManga = true,
                ascoltareCanzoni = true,
                aggiungereCanzoni = true,
                accessoGiochiMiciomani = true,
                visualizzaClassifica = true,
                visualizzaRuoliUtente = true
            },
            RuoloUtente.Miciomane => new PermessiUtente
            {
                leggereManga = true,
                aggiungereManga = true,
                ascoltareCanzoni = true,
                aggiungereCanzoni = true,
                accessoGiochiMiciomani = true,
                visualizzaClassifica = true,
                visualizzaRuoliUtente = true,
                modificaRuoliUtente = true,
                modificaCronoJob = true
            },
            _ => new PermessiUtente()
        };

        public static PermessiUtente FromDatabaseValue(string valore)
        {
            using JsonDocument document = JsonDocument.Parse(valore);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return JsonSerializer.Deserialize<PermessiUtente>(
                    valore,
                    (JsonSerializerOptions?)null
                ) ?? new PermessiUtente();

            HashSet<string> permessi = document.RootElement
                .EnumerateArray()
                .Where(elemento => elemento.ValueKind == JsonValueKind.String)
                .Select(elemento => elemento.GetString() ?? "")
                .ToHashSet(StringComparer.Ordinal);

            return new PermessiUtente
            {
                leggereManga = permessi.Contains("leggereManga"),
                aggiungereManga = permessi.Contains("aggiungereManga"),
                ascoltareCanzoni = permessi.Contains("ascoltareCanzoni"),
                aggiungereCanzoni = permessi.Contains("aggiungereCanzoni"),
                accessoGiochiMiciomani = permessi.Contains("accessoGiochiMiciomani"),
                visualizzaClassifica = permessi.Contains("visualizzaClassifica"),
                visualizzaRuoliUtente = permessi.Contains("visualizzaRuoliUtente"),
                modificaRuoliUtente = permessi.Contains("modificaRuoliUtente"),
                modificaCronoJob = permessi.Contains("modificaCronoJob")
            };
        }
    }

    public class Admin
    {
        public string idUtente { get; set; } = "";
        public RuoloUtente ruolo { get; set; } = RuoloUtente.User;
        public PermessiUtente permessi { get; set; } = new();
    }

    public static class RuoloUtenteExtensions
    {
        public static string ToDatabaseValue(this RuoloUtente ruolo) => ruolo switch
        {
            RuoloUtente.Miciomane => "miciomane",
            RuoloUtente.Admin => "admin",
            RuoloUtente.Player => "player",
            _ => "user"
        };

        public static RuoloUtente FromDatabaseValue(string ruolo) => ruolo switch
        {
            "miciomane" => RuoloUtente.Miciomane,
            "admin" => RuoloUtente.Admin,
            "player" => RuoloUtente.Player,
            "user" => RuoloUtente.User,
            _ => throw new ArgumentException($"Ruolo utente non valido: {ruolo}", nameof(ruolo))
        };
    }
}
