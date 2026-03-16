-- ============================================================
-- sp_IptvAccounts_PorTipoServicio
-- Lista las cuentas IPTV activas filtradas por TipoServicioId,
-- con slots usados calculados (suscripciones Active vigentes).
-- Ordenadas: primero las que tienen más slots disponibles,
-- luego por fecha de vencimiento más próxima.
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.sp_IptvAccounts_PorTipoServicio
    @TipoServicioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id,
        a.AccessUser,
        a.AccessPassword,
        a.PlatformUrl,
        a.PinCode,
        a.TipoServicioId,
        st.Nombre                                       AS ServiceName,
        a.MaxSlots,
        a.PurchasePrice,
        a.ExpirationDate,
        DATEDIFF(DAY, GETUTCDATE(), a.ExpirationDate)   AS DaysRemaining,
        a.Notes,
        a.IsActive,
        a.CreatedAt,
        a.UpdatedAt,
        COUNT(s.Id)                                     AS UsedSlots
    FROM dbo.IptvAccounts a
    LEFT JOIN dbo.TiposServicio st
           ON st.Id = a.TipoServicioId
    LEFT JOIN dbo.CustomerSubscriptions s
           ON s.IptvAccountId = a.Id
          AND s.Status = 'Active'
    WHERE a.IsActive = 1
      AND a.TipoServicioId = @TipoServicioId
    GROUP BY
        a.Id, a.AccessUser, a.AccessPassword, a.PlatformUrl,
        a.PinCode, a.TipoServicioId, st.Nombre, a.MaxSlots,
        a.PurchasePrice, a.ExpirationDate, a.Notes,
        a.IsActive, a.CreatedAt, a.UpdatedAt
    ORDER BY
        (a.MaxSlots - COUNT(s.Id)) DESC,   -- más slots disponibles primero
        a.ExpirationDate ASC;              -- vence más tarde al final
END;
GO
