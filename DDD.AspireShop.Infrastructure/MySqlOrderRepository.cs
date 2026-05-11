using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Domain.Orders;
using MySqlConnector;

namespace DDD.AspireShop.Infrastructure;

internal sealed class MySqlOrderRepository(MySqlDataSource dataSource) : IOrderRepository
{
    public async Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken = default)
    {
        var orders = new List<Order>();
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                o.id,
                o.customer_name,
                o.created_at,
                o.status,
                l.product_id,
                l.product_name,
                l.unit_price,
                l.quantity
            FROM orders o
            LEFT JOIN order_lines l ON l.order_id = o.id
            ORDER BY o.created_at DESC, l.id ASC;
            """;

        var rows = new Dictionary<Guid, OrderReadModel>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var orderId = reader.GetGuid("id");
            if (!rows.TryGetValue(orderId, out var order))
            {
                order = new OrderReadModel(
                    orderId,
                    reader.GetString("customer_name"),
                    new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime("created_at"), DateTimeKind.Utc)),
                    (OrderStatus)reader.GetInt32("status"));
                rows.Add(orderId, order);
            }

            if (!await reader.IsDBNullAsync(reader.GetOrdinal("product_id"), cancellationToken))
            {
                order.Lines.Add(new OrderLine(
                    reader.GetGuid("product_id"),
                    reader.GetString("product_name"),
                    reader.GetDecimal("unit_price"),
                    reader.GetInt32("quantity")));
            }
        }

        foreach (var row in rows.Values)
        {
            orders.Add(Order.Rehydrate(row.Id, row.CustomerName, row.CreatedAt, row.Status, row.Lines));
        }

        return orders;
    }

    public async Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO orders (id, customer_name, created_at, status, total_amount)
                VALUES (@id, @customer_name, @created_at, @status, @total_amount)
                ON DUPLICATE KEY UPDATE
                    customer_name = VALUES(customer_name),
                    created_at = VALUES(created_at),
                    status = VALUES(status),
                    total_amount = VALUES(total_amount);
                """;
            command.Parameters.AddWithValue("@id", order.Id.ToString());
            command.Parameters.AddWithValue("@customer_name", order.CustomerName);
            command.Parameters.AddWithValue("@created_at", order.CreatedAt.UtcDateTime);
            command.Parameters.AddWithValue("@status", (int)order.Status);
            command.Parameters.AddWithValue("@total_amount", order.TotalAmount);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM order_lines WHERE order_id = @order_id;";
            deleteCommand.Parameters.AddWithValue("@order_id", order.Id.ToString());
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var line in order.Lines)
        {
            await using var lineCommand = connection.CreateCommand();
            lineCommand.Transaction = transaction;
            lineCommand.CommandText = """
                INSERT INTO order_lines (order_id, product_id, product_name, unit_price, quantity, line_total)
                VALUES (@order_id, @product_id, @product_name, @unit_price, @quantity, @line_total);
                """;
            lineCommand.Parameters.AddWithValue("@order_id", order.Id.ToString());
            lineCommand.Parameters.AddWithValue("@product_id", line.ProductId.ToString());
            lineCommand.Parameters.AddWithValue("@product_name", line.ProductName);
            lineCommand.Parameters.AddWithValue("@unit_price", line.UnitPrice);
            lineCommand.Parameters.AddWithValue("@quantity", line.Quantity);
            lineCommand.Parameters.AddWithValue("@line_total", line.LineTotal);
            await lineCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private sealed record OrderReadModel(Guid Id, string CustomerName, DateTimeOffset CreatedAt, OrderStatus Status)
    {
        public List<OrderLine> Lines { get; } = [];
    }
}
