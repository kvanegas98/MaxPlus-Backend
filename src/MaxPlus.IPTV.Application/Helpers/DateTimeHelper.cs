using System;

namespace MaxPlus.IPTV.Application.Helpers;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo NicaraguaTimeZone;

    static DateTimeHelper()
    {
        // En Windows el ID es 'Central America Standard Time'
        // En Linux (si llegaras a desplegar en Docker/Linux) suele ser 'America/Managua'
        try
        {
            NicaraguaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback para entornos Linux/Docker
            NicaraguaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Managua");
        }
    }

    /// <summary>
    /// Retorna la fecha y hora actual de Nicaragua.
    /// </summary>
    public static DateTime GetNicaraguaTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NicaraguaTimeZone);
    }

    /// <summary>
    /// Convierte una fecha UTC a la hora de Nicaragua.
    /// </summary>
    public static DateTime ToNicaraguaTime(this DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, NicaraguaTimeZone);
    }
}
