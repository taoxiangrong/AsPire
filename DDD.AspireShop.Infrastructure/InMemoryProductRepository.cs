using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Domain.Products;

namespace DDD.AspireShop.Infrastructure;

internal sealed class InMemoryProductRepository : IProductRepository
{
    private readonly Lock _lock = new();
    private readonly Dictionary<Guid, Product> _products;

    public InMemoryProductRepository()
    {
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

        _products = products.ToDictionary(product => product.Id);
    }

    public Task<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyCollection<Product>>(_products.Values.ToArray());
        }
    }

    public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }
    }

    public Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _products[product.Id] = product;
            return Task.CompletedTask;
        }
    }
}
