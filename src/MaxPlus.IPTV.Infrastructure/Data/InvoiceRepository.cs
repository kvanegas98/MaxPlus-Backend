using System.Data;
using System.Text.Json;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public InvoiceRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> CreateInvoiceAsync(Invoice invoice)
    {
        using var connection = _connectionFactory.CreateConnection();

        var detailsJson = JsonSerializer.Serialize(invoice.Details.Select(d => new
        {
            TipoServicioId = d.TipoServicioId,
            SubscriptionId = d.SubscriptionId,
            Concepto       = d.Concept,
            Cantidad       = d.Quantity,
            DurationMonths = d.DurationMonths,
            PrecioUnitario = d.UnitPrice,
            Descuento      = d.DiscountAmount,
            SubTotal       = d.SubTotal,
            Nota           = d.Nota
        }));

        var parameters = new DynamicParameters();
        parameters.Add("@NombreCliente",  invoice.CustomerName);
        parameters.Add("@UsuarioId",      invoice.UserId);
        parameters.Add("@TotalAmount",    invoice.TotalAmount);
        parameters.Add("@DetailsJson",    detailsJson, DbType.String);
        parameters.Add("@TipoOrden",      invoice.OrderType);
        parameters.Add("@MetodoPago",     invoice.PaymentMethod);
        parameters.Add("@MontoRecibido",  invoice.AmountReceived);
        parameters.Add("@ClienteId",      invoice.CustomerId);
        parameters.Add("@MontoDescuento", invoice.DiscountAmount);
        parameters.Add("@Nota",           invoice.Nota);
        parameters.Add("@SubscriptionId",   invoice.SubscriptionId);
        parameters.Add("@ServiceOrderId",   invoice.ServiceOrderId);
        parameters.Add("@MetodoPagoId",     invoice.MetodoPagoId);
        parameters.Add("@PaymentReference", invoice.PaymentReference);
        parameters.Add("@FacturaId",      dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_ProcesarVenta",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@FacturaId");
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@FacturaId", invoiceId);

        using var multi = await connection.QueryMultipleAsync(
            "sp_ObtenerFacturaPorId",
            parameters,
            commandType: CommandType.StoredProcedure);

        var invoice = await multi.ReadSingleOrDefaultAsync<Invoice>();

        if (invoice != null)
        {
            var details = (await multi.ReadAsync<InvoiceDetail>()).ToList();
            invoice.Details = details;
        }

        return invoice;
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(Guid customerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ClienteId", customerId);
        return await connection.QueryAsync<Invoice>(
            "sp_Facturas_PorCliente",
            parameters,
            commandType: CommandType.StoredProcedure);
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
