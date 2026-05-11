using DDD.AspireShop.Application.Catalog;

namespace DDD.AspireShop.Web;

public sealed class ShoppingCartService
{
    private readonly Dictionary<Guid, CartLine> _lines = [];

    public event Action? Changed;

    public IReadOnlyCollection<CartLine> Lines => _lines.Values.OrderBy(line => line.ProductName).ToArray();

    public int TotalQuantity => _lines.Values.Sum(line => line.Quantity);

    public decimal TotalAmount => _lines.Values.Sum(line => line.LineTotal);

    public void Add(ProductDto product, int quantity = 1)
    {
        if (quantity <= 0)
        {
            return;
        }

        if (_lines.TryGetValue(product.Id, out var line))
        {
            _lines[product.Id] = line with { Quantity = line.Quantity + quantity };
        }
        else
        {
            _lines[product.Id] = new CartLine(product.Id, product.Name, product.Price, product.Currency, quantity);
        }

        Changed?.Invoke();
    }

    public void ChangeQuantity(Guid productId, int quantity)
    {
        if (!_lines.ContainsKey(productId))
        {
            return;
        }

        if (quantity <= 0)
        {
            _lines.Remove(productId);
        }
        else
        {
            _lines[productId] = _lines[productId] with { Quantity = quantity };
        }

        Changed?.Invoke();
    }

    public void Remove(Guid productId)
    {
        _lines.Remove(productId);
        Changed?.Invoke();
    }

    public void Clear()
    {
        _lines.Clear();
        Changed?.Invoke();
    }
}

public sealed record CartLine(Guid ProductId, string ProductName, decimal UnitPrice, string Currency, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}
