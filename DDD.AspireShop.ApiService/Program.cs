using DDD.AspireShop.ApiService.IntegrationEvents;
using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Application.Catalog;
using DDD.AspireShop.Application.DomainEvents;
using DDD.AspireShop.Domain.Common;
using DDD.AspireShop.Infrastructure;
using Savorboard.CAP.InMemoryMessageQueue;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.AddMySqlDataSource("shopdb");
builder.Services.AddShopInfrastructure();
builder.Services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
builder.Services.AddScoped<IIntegrationEventPublisher, CapIntegrationEventPublisher>();
builder.Services.AddScoped<OrderSubmittedSubscriber>();
builder.Services.AddSingleton<InMemoryConsumedOrderMessageStore>();
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(GetProductsQuery).Assembly));
builder.Services.AddCap(options =>
{
    options.UseInMemoryStorage();
    options.UseInMemoryMessageQueue();
    options.UseDashboard();
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
