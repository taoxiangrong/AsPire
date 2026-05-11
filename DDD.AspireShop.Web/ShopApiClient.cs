using System.Net.Http.Json;
using System.Text.Json;
using DDD.AspireShop.Application.Catalog;
using DDD.AspireShop.Application.FlashSales;
using DDD.AspireShop.Application.Orders;

namespace DDD.AspireShop.Web;

public sealed class ShopApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<ProductDto[]>("/api/products", cancellationToken) ?? [];

    public async Task<IReadOnlyCollection<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<OrderDto[]>("/api/orders", cancellationToken) ?? [];

    public async Task<IReadOnlyCollection<FlashSaleItemDto>> GetFlashSaleItemsAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<FlashSaleItemDto[]>("/api/flash-sales", cancellationToken) ?? [];

    public async Task<OrderDto> SubmitOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/orders", request, cancellationToken);
        return await ReadOrderResponseAsync(response, cancellationToken);
    }

    public async Task<OrderDto> SubmitFlashSaleOrderAsync(SubmitFlashSaleOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/flash-sales/orders", request, cancellationToken);
        return await ReadOrderResponseAsync(response, cancellationToken);
    }

    private static async Task<OrderDto> ReadOrderResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken) ??
                throw new InvalidOperationException("API 未返回订单。");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = TryReadError(content) ?? "提交订单失败。";
        throw new InvalidOperationException(message);
    }

    private static string? TryReadError(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            return document.RootElement.TryGetProperty("error", out var error)
                ? error.GetString()
                : null;
        }
        catch (JsonException)
        {
            return content;
        }
    }
}
