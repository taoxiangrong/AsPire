using DDD.AspireShop.Domain.Common;

namespace DDD.AspireShop.Domain.Products;

public sealed class Product
{
    private Product(Guid id, string name, string description, Money price, int stock)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        IsActive = true;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Money Price { get; private set; }

    public int Stock { get; private set; }

    public bool IsActive { get; private set; }

    public static Product Create(string name, string description, Money price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("商品名称不能为空。");
        }

        if (stock < 0)
        {
            throw new DomainException("库存不能小于 0。");
        }

        return new Product(Guid.NewGuid(), name.Trim(), description.Trim(), price, stock);
    }

    public static Product Rehydrate(Guid id, string name, string description, Money price, int stock, bool isActive)
    {
        var product = new Product(id, name, description, price, stock)
        {
            IsActive = isActive
        };

        return product;
    }

    public void Allocate(int quantity)
    {
        if (!IsActive)
        {
            throw new DomainException("已下架商品不能下单。");
        }

        if (quantity <= 0)
        {
            throw new DomainException("购买数量必须大于 0。");
        }

        if (Stock < quantity)
        {
            throw new DomainException($"{Name} 库存不足。");
        }

        Stock -= quantity;
    }

    public void Replenish(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("补货数量必须大于 0。");
        }

        Stock += quantity;
    }

    public void ChangePrice(Money price) => Price = price;

    public void Deactivate() => IsActive = false;
}
