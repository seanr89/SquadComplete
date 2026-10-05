namespace squad_domain.Models;

/// <summary>
/// The broad pitch line a player occupies. This is the shared vocabulary between ingestion
/// (<c>squad-func</c>, which stores whatever raw position the data source supplies), display
/// (<c>squad-api</c>) and the frontend, whose <c>Position</c> type in <c>types.ts</c> must stay in
/// sync with <see cref="PositionGroupExtensions.ToCode"/>.
/// </summary>
public enum PositionGroup
{
    /// <summary>Missing or unrecognised position.</summary>
    Unknown,
    Goalkeeper,
    Defender,
    Midfielder,
    Forward
}

public static class PositionGroupExtensions
{
    /// <summary>Wire code for <see cref="PositionGroup.Unknown"/>.</summary>
    public const string UnknownCode = "UNK";

    /// <summary>
    /// Classifies a raw position string from a data source (e.g. <c>"Goalkeeper"</c>, <c>"CB"</c>, <c>"@P5"</c>).
    /// Matching is case-insensitive; unrecognised or empty values map to <see cref="PositionGroup.Unknown"/>.
    /// </summary>
    public static PositionGroup Parse(string? position)
    {
        if (string.IsNullOrEmpty(position)) return PositionGroup.Unknown;

        var pos = position.ToUpperInvariant();
        if (pos.Contains("GOALKEEPER") || pos is "G" or "GK" or "@P5") return PositionGroup.Goalkeeper;
        if (pos.Contains("DEFENDER") || pos is "D" or "DEF" or "LB" or "RB" or "CB") return PositionGroup.Defender;
        if (pos.Contains("MIDFIELDER") || pos is "M" or "MID" or "CM" or "DM" or "AM") return PositionGroup.Midfielder;
        if (pos.Contains("FORWARD") || pos is "F" or "FWD" or "ST" or "LW" or "RW") return PositionGroup.Forward;

        return PositionGroup.Unknown;
    }

    /// <summary>The short code used in API responses and by the frontend (<c>GK</c>, <c>DEF</c>, <c>MID</c>, <c>FWD</c>, <c>UNK</c>).</summary>
    public static string ToCode(this PositionGroup group) => group switch
    {
        PositionGroup.Goalkeeper => "GK",
        PositionGroup.Defender => "DEF",
        PositionGroup.Midfielder => "MID",
        PositionGroup.Forward => "FWD",
        _ => UnknownCode
    };
}
