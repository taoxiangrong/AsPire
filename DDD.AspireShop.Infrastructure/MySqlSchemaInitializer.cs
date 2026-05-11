using DDD.AspireShop.Domain.Products;
using Microsoft.Extensions.Hosting;
using MySqlConnector;

namespace DDD.AspireShop.Infrastructure;

internal sealed class MySqlSchemaInitializer(MySqlDataSource dataSource) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS products (
                id CHAR(36) NOT NULL PRIMARY KEY,
                name VARCHAR(200) NOT NULL,
                description VARCHAR(1000) NOT NULL,
                price DECIMAL(18,2) NOT NULL,
                currency VARCHAR(8) NOT NULL,
                stock INT NOT NULL,
                is_active BOOLEAN NOT NULL
            );
            """, cancellationToken);

        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS orders (
                id CHAR(36) NOT NULL PRIMARY KEY,
                customer_name VARCHAR(200) NOT NULL,
                created_at DATETIME(6) NOT NULL,
                status INT NOT NULL,
                total_amount DECIMAL(18,2) NOT NULL
            );
            """, cancellationToken);

        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS order_lines (
                id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                order_id CHAR(36) NOT NULL,
                product_id CHAR(36) NOT NULL,
                product_name VARCHAR(200) NOT NULL,
                unit_price DECIMAL(18,2) NOT NULL,
                quantity INT NOT NULL,
                line_total DECIMAL(18,2) NOT NULL,
                INDEX ix_order_lines_order_id (order_id),
                CONSTRAINT fk_order_lines_orders FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
            );
            """, cancellationToken);

        await SeedProductsAsync(connection, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task SeedProductsAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM products;";
        var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));
        if (count > 0)
        {
            return;
        }

        var products = new[]
        {
            Product.Create("领域驱动设计红书", "聚合、仓储、限界上下文的经典入门读物。", new Money(89), 12),
            Product.Create("Aspire 微服务套件", "用于本地开发、服务发现和可观测性的 .NET Aspire 示例商品。", new Money(199), 8),
            Product.Create("战术建模工作坊", "事件风暴、命令、实体和值对象的团队建模课程。", new Money(1299), 4),
            Product.Create("云原生观测仪表盘", "面向运营团队的指标、日志、链路追踪三件套。", new Money(399), 18),
            Product.Create("限量架构师键盘", "热插拔轴体、低延迟连接和商城专属键帽。", new Money(699), 6),
            Product.Create("开发者咖啡礼盒", "冷萃、挂耳和会议续命组合装。", new Money(159), 30),
            Product.Create("微服务部署训练营", "从本地 Aspire 到容器发布的实战课程。", new Money(1899), 5),
            Product.Create("会员专属运维背包", "多仓隔层、防泼水面料和轻量通勤设计。", new Money(459), 10)
        };

        foreach (var product in products)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO products (id, name, description, price, currency, stock, is_active)
                VALUES (@id, @name, @description, @price, @currency, @stock, @is_active);
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
    }

    private static async Task ExecuteAsync(MySqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
