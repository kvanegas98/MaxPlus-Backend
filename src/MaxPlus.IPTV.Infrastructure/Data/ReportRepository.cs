using System.Data;
using MaxPlus.IPTV.Core.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class ReportRepository : IReportRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ReportRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ReportSummaryData> GetSummaryAsync(string filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Filtro", filter);

        using var multi = await connection.QueryMultipleAsync(
            "sp_Reportes_Resumen",
            parameters,
            commandType: CommandType.StoredProcedure);

        var kpi         = await multi.ReadFirstOrDefaultAsync<dynamic>();
        var byType      = (await multi.ReadAsync<dynamic>()).ToList();
        var demosRow    = await multi.ReadFirstOrDefaultAsync<dynamic>();
        var subsRow     = await multi.ReadFirstOrDefaultAsync<dynamic>();

        var orderTypeCounts = byType.Select(r => new OrderTypeCount(
            TipoOrden:    (string)r.TipoOrden,
            TotalOrdenes: (int)r.TotalOrdenes
        )).ToList();

        return new ReportSummaryData(
            VentasTotales:        (decimal)(kpi?.VentasTotales   ?? 0m),
            TicketPromedio:       (decimal)(kpi?.TicketPromedio   ?? 0m),
            TotalOrdenes:         (int)(kpi?.TotalOrdenes         ?? 0),
            ProductoMasVendido:   (string?)kpi?.ProductoMasVendido,
            ByOrderType:          orderTypeCounts,
            TotalDemosHoy:        (int)(demosRow?.TotalDemosHoy       ?? 0),
            SuscripcionesActivas: (int)(subsRow?.SuscripcionesActivas ?? 0)
        );
    }

    public async Task<IEnumerable<HourlySaleRow>> GetSalesByHourAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<dynamic>(
            "sp_Reportes_VentasPorHora",
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => new HourlySaleRow(
            Hour:        (int)r.Hora,
            TotalOrders: (int)r.TotalOrdenes,
            TotalSales:  (decimal)r.TotalVentas
        )).ToList();
    }

    public async Task<IEnumerable<TopProductRow>> GetTopProductsAsync(string filter, int top = 10)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Filtro", filter);
        parameters.Add("@Top",    top);

        var rows = await connection.QueryAsync<dynamic>(
            "sp_Reportes_TopProductos",
            parameters,
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => new TopProductRow(
            Name:         (string)r.Nombre,
            UnitsSold:    (int)r.UnidadesVendidas,
            TotalRevenue: (decimal)r.TotalRecaudado
        )).ToList();
    }

    public async Task<IEnumerable<OrderHistoryRow>> GetOrderHistoryAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10, string? customerName = null, string? orderNumber = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@FechaInicio",  startDate);
        parameters.Add("@FechaFin",     endDate);
        parameters.Add("@PageNumber",   pageNumber);
        parameters.Add("@PageSize",     pageSize);
        parameters.Add("@Cliente",      customerName);
        parameters.Add("@NumeroOrden",  orderNumber);

        using var multi = await connection.QueryMultipleAsync(
            "sp_Reportes_HistorialOrdenes",
            parameters,
            commandType: CommandType.StoredProcedure);

        var invoices = (await multi.ReadAsync<dynamic>()).ToList();
        var allDetails = (await multi.ReadAsync<InvoiceDetail>()).ToList();

        return invoices.Select(r => new OrderHistoryRow(
            InvoiceId:            (Guid)r.Id,
            NumeroOrden:          (string)r.NumeroOrden,
            SaleDate:             (DateTime)r.FechaVenta,
            OrderType:            (string)r.TipoOrden,
            PaymentMethod:        (string?)r.MetodoPago,
            TotalAmount:          (decimal)r.TotalAmount,
            Status:               (string)r.Estado,
            CustomerName:         (string?)r.NombreCliente ?? "Cliente General",
            RegisteredClientName: (string?)r.ClienteNombre,
            TotalProducts:        (int)r.TotalProductos,
            Details:              allDetails.Where(d => d.InvoiceId == (Guid)r.Id)
        )).ToList();
    }

    public async Task VoidInvoiceAsync(Guid invoiceId, Guid userId, string? reason)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@FacturaId", invoiceId);
        parameters.Add("@UsuarioId", userId);
        parameters.Add("@Motivo",    reason);

        await connection.ExecuteAsync(
            "sp_Facturas_Anular",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
