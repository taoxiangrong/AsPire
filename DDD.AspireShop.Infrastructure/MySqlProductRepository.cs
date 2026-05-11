using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Domain.Products;
using MySqlConnector;

namespace DDD.AspireShop.Infrastructure;

internal sealed class MySqlProductRepository(MySqlDataSource dataSource) : IProductRepository
{
    public async Task<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken = default)
    {
        var products = new List<Product>();
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, name, description, price, currency, stock, is_active
            FROM products
            ORDER BY name;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(ReadProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, name, description, price, currency, stock, is_active
            FROM products
            WHERE id = @id;
            """;
        command.Parameters.AddWithValue("@id", id.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadProduct(reader) : null;
    }

    public async Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO products (id, name, description, price, currency, stock, is_active)
            VALUES (@id, @name, @description, @price, @currency, @stock, @is_active)
            ON DUPLICATE KEY UPDATE
                name = VALUES(name),
                description = VALUES(description),
                price = VALUES(price),
                currency = VALUES(currency),
                stock = VALUES(stock),
                is_active = VALUES(is_active);
            """;
        command.Parameters.AddWithValue("@id", product.Id.ToString());
        command.Parameters.AddWithValue("@name", product.Name);
        command.Parameters.AddWithValue("@description", product.Description);
        command.Parameters.AddWithValue("@price", product.Price.Amount);
        command.Parameters.AddWithValue("@currency", product.Price.Currency);
        command.Parameters.AddWithValue("@stock", product.Stock);
        command.Parameters.AddWithValue("@is_active", product.IsActive);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Product ReadProduct(MySqlDataReader reader) =>
        Product.Rehydrate(
            reader.GetGuid("id"),
            reader.GetString("name"),
            reader.GetString("description"),
            new Money(reader.GetDecimal("price"), reader.GetString("currency")),
            reader.GetInt32("stock"),
            reader.GetBoolean("is_active"));
}
