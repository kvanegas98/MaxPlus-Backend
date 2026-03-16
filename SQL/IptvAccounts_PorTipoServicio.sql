-- ============================================================
-- SP: Obtener cuentas IPTV disponibles por tipo de servicio
-- Uso: selector de cuenta al aprobar una orden en frontend
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_PorTipoServicio]
    @TipoServicioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id,
        a.AccessUser,
        a.AccessPassword,
        a.AccessEmail,
        a.PlatformUrl,
        a.PinCode,
        a.TipoServicioId,
        ts.Nombre AS ServiceName,
        a.MaxSlots,
        a.PurchasePrice,
        a.ExpirationDate,
        CASE
            WHEN a.ExpirationDate IS NOT NULL
            THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(a.ExpirationDate AS DATE))
            ELSE NULL
        END AS DaysRemaining,
        a.Notes,
        a.IsActive,
        a.CreatedAt,
        a.UpdatedAt,
        (
            SELECT COUNT(*)
            FROM [dbo].[CustomerSubscriptions] cs
            WHERE cs.IptvAccountId = a.Id
              AND cs.Status NOT IN ('Cancelled', 'Renewed')
        ) AS UsedSlots
    FROM [dbo].[IptvAccounts] a
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = a.TipoServicioId
    WHERE a.IsActive = 1
      AND a.TipoServicioId = @TipoServicioId
      -- Solo cuentas con slots disponibles
      AND (
            SELECT COUNT(*)
            FROM [dbo].[CustomerSubscriptions] cs
            WHERE cs.IptvAccountId = a.Id
              AND cs.Status NOT IN ('Cancelled', 'Renewed')
          ) < a.MaxSlots
    ORDER BY a.ExpirationDate ASC;
END
GO
