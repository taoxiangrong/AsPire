# DDD.AspireShop 知识点学习文档

这是一份围绕本项目的学习笔记，目标是帮助你按代码路径理解 .NET Aspire、DDD 分层、MediatR、CAP、MySQL 仓储和 Blazor 前端之间的关系。

## 1. 学习路线

建议按下面顺序阅读代码：

1. `DDD.AspireShop.AppHost/AppHost.cs`：看 Aspire 如何编排 Web 和 API。
2. `DDD.AspireShop.ApiService/Program.cs`：看 API 如何注册服务、数据库、MediatR、CAP。
3. `DDD.AspireShop.Domain/Products/Product.cs` 与 `DDD.AspireShop.Domain/Orders/Order.cs`：看核心业务规则。
4. `DDD.AspireShop.Application/Orders/SubmitOrderCommand.cs`：看一个完整用例如何被编排。
5. `DDD.AspireShop.Infrastructure/MySqlProductRepository.cs` 与 `MySqlOrderRepository.cs`：看接口如何落到数据库。
6. `DDD.AspireShop.Web/ShopApiClient.cs` 与 `Components/Pages/*`：看前端如何调用 API。

## 2. .NET Aspire

本项目使用 Aspire 做本地分布式应用编排。

核心文件：

- `DDD.AspireShop.AppHost/AppHost.cs`
- `DDD.AspireShop.ServiceDefaults/Extensions.cs`
- `DDD.AspireShop.Web/Program.cs`
- `DDD.AspireShop.ApiService/Program.cs`

重点概念：

- `DistributedApplication.CreateBuilder(args)`：创建 Aspire 分布式应用构建器。
- `AddProject<T>("name")`：把一个 .NET 项目加入 Aspire 编排。
- `WithReference(apiService)`：让 Web 可以通过服务发现访问 API。
- `WaitFor(apiService)`：Web 等 API 可用后再启动依赖。
- `AddServiceDefaults()`：统一注册健康检查、服务发现、OpenTelemetry 等基础能力。
- `https+http://apiservice`：Aspire 服务发现地址，优先 HTTPS，必要时回退 HTTP。

在这个项目里，Aspire 主要解决三件事：

- 本地一次启动多个服务。
- Web 通过逻辑服务名访问 API。
- 给服务加健康检查与可观测性基础设施。

## 3. DDD 分层

本项目采用典型 DDD + Clean Architecture 风格。

### Domain

位置：`DDD.AspireShop.Domain`

职责：

- 表达业务概念。
- 保护业务不变量。
- 抛出领域异常。
- 产生领域事件。

代表代码：

- `Product.Allocate(int quantity)`：校验商品是否上架、购买数量、库存是否足够，然后扣减库存。
- `Order.Submit(...)`：校验客户名、订单行，调用商品扣库存，创建订单行，产生 `OrderSubmittedDomainEvent`。
- `Money`：金额值对象，金额和币种一起表达价格。

学习重点：

- 业务规则优先放在领域对象里，而不是 Controller 或数据库层。
- `Rehydrate` 用于从数据库还原对象，避免触发创建时的副作用。
- 领域对象应尽量少依赖外部技术框架。

### Application

位置：`DDD.AspireShop.Application`

职责：

- 编排一次业务用例。
- 定义输入输出 DTO。
- 定义仓储、事件发布等抽象接口。
- 使用 MediatR 实现命令与查询。

代表代码：

- `SubmitOrderCommand`：提交订单命令。
- `GetProductsQuery`：查询商品列表。
- `IProductRepository`、`IOrderRepository`：仓储接口。
- `IDomainEventDispatcher`、`IIntegrationEventPublisher`：事件抽象。

学习重点：

- Application 层不直接写 SQL。
- Application 层可以依赖 Domain，但不依赖 Infrastructure。
- 用例流程通常是：读取数据 -> 调用领域模型 -> 持久化 -> 发布事件 -> 返回 DTO。

### Infrastructure

位置：`DDD.AspireShop.Infrastructure`

职责：

- 实现应用层定义的接口。
- 处理数据库访问、表结构初始化和种子数据。
- 通过依赖注入把实现注册给上层使用。

代表代码：

- `MySqlSchemaInitializer`：启动时创建 `products`、`orders`、`order_lines` 表并初始化商品。
- `MySqlProductRepository`：商品查询与保存。
- `MySqlOrderRepository`：订单查询与保存。
- `DependencyInjection.AddShopInfrastructure()`：注册仓储实现。

学习重点：

- 基础设施层依赖 Application 的接口与 Domain 的模型。
- SQL 与 MySQL 细节被隔离在 Infrastructure。
- 上层通过接口使用仓储，不关心具体存储技术。

### ApiService

位置：`DDD.AspireShop.ApiService`

职责：

- 暴露 HTTP API。
- 注册应用层、基础设施层、MediatR、CAP。
- 将领域异常转换为 HTTP 400。

代表代码：

- `ProductsController`
- `OrdersController`
- `FlashSalesController`
- `CapIntegrationEventPublisher`
- `OrderSubmittedSubscriber`

学习重点：

- Controller 只做薄入口，业务交给 MediatR command/query。
- `DomainException` 被捕获并转换为 `{ error = "..." }`。
- API 层负责连接技术组件，例如 CAP 与 ASP.NET Core。

### Web

位置：`DDD.AspireShop.Web`

职责：

- 提供 Blazor Server UI。
- 保存用户购物车状态。
- 通过 `ShopApiClient` 调用 API。

代表代码：

- `ShopApiClient`
- `ShoppingCartService`
- `Components/Pages/Products.razor`
- `Components/Pages/Cart.razor`
- `Components/Pages/Checkout.razor`
- `Components/Pages/Orders.razor`

学习重点：

- Web 不直接依赖 Infrastructure，也不直接访问数据库。
- `ShoppingCartService` 是 scoped 服务，保存一次用户交互会话里的购物车状态。
- 页面通过注入服务调用 API，API 返回应用层 DTO。

## 4. MediatR

MediatR 在项目里承担两种角色：

- 命令与查询分发：Controller 调用 `mediator.Send(...)`。
- 领域事件通知：`MediatRDomainEventDispatcher` 把领域事件包装成 `INotification` 发布。

典型流程：

```text
Controller
  -> IMediator.Send(command/query)
  -> Handler.Handle(...)
  -> Repository / Domain / Event
  -> DTO
```

命名习惯：

- `GetProductsQuery`：查询，不改变状态。
- `SubmitOrderCommand`：命令，会改变状态。
- `OrderSubmittedDomainEventHandler`：领域事件处理器。

学习重点：

- Command/Query 把用例入口从 Controller 里拆出来。
- Handler 是应用层业务编排的核心位置。
- MediatR 可以减少 Controller 和具体用例之间的直接耦合。

## 5. 领域事件与集成事件

本项目有两类事件：

| 类型 | 位置 | 用途 |
| --- | --- | --- |
| 领域事件 | `Domain` | 表示领域内部发生的重要事实 |
| 集成事件 | `Application` / `ApiService` | 用于跨模块或跨服务发布消息 |

下单后的事件链路：

```text
Order.Submit
  -> AddDomainEvent(OrderSubmittedDomainEvent)
  -> SubmitOrderCommandHandler.DispatchAsync
  -> OrderSubmittedDomainEventHandler
  -> IIntegrationEventPublisher.PublishAsync("shop.orders.submitted", ...)
  -> CAP
  -> OrderSubmittedSubscriber.HandleAsync
```

学习重点：

- 领域事件在领域层产生，但不关心谁消费。
- 集成事件更偏系统边界之外的消息契约。
- 当前 CAP 使用内存存储和内存队列，适合演示，不适合生产持久消息。

## 6. 仓储模式

应用层定义接口：

- `IProductRepository`
- `IOrderRepository`

基础设施层实现接口：

- `MySqlProductRepository`
- `MySqlOrderRepository`

好处：

- 应用层不关心数据来自 MySQL、内存、文件还是远程服务。
- 测试时可以替换为内存仓储。
- SQL 变化不会污染领域层和应用层。

注意点：

- `Order.Submit` 会扣减商品库存，Handler 保存订单后还要保存商品。
- 当前订单保存和商品库存保存不在同一个数据库事务中，真实生产系统要考虑一致性。
- 当前 `ON DUPLICATE KEY UPDATE` 简化了保存逻辑，适合示例。

## 7. MySQL 初始化

`MySqlSchemaInitializer` 是一个 `IHostedService`，API 启动时执行：

- 创建 `products` 表。
- 创建 `orders` 表。
- 创建 `order_lines` 表。
- 如果商品表为空，则写入示例商品。

学习重点：

- 示例项目中直接用代码初始化 schema，便于快速启动。
- 正式项目通常使用迁移工具，例如 EF Core Migration、Flyway、Liquibase 等。

## 8. Blazor Server 前端

前端关键服务：

- `ShopApiClient`：封装 API 调用。
- `ShoppingCartService`：维护购物车状态。

典型页面：

- `Store.razor` / `Products.razor`：展示商品。
- `Cart.razor`：查看与修改购物车。
- `Checkout.razor`：提交订单。
- `Orders.razor`：查看订单。
- `FlashSale.razor`：秒杀入口。
- `DddLayers.razor`：项目分层展示页面。

学习重点：

- Blazor Server 的交互状态保存在服务端 circuit 中。
- scoped 服务在 Blazor Server 中常用于保存每个用户会话级状态。
- 前端 DTO 复用了 Application 项目中的 DTO 类型。

## 9. 秒杀模块

秒杀相关文件：

- `Application/FlashSales/GetFlashSaleItemsQuery.cs`
- `Application/FlashSales/SubmitFlashSaleOrderCommand.cs`
- `ApiService/Controllers/FlashSalesController.cs`
- `Web/Components/Pages/FlashSale.razor`

当前秒杀更像是对下单用例的一个变体入口：

- 查询秒杀商品。
- 提交秒杀订单。
- 最终仍然复用订单领域模型、商品库存校验和订单保存能力。

可继续学习的问题：

- 高并发库存扣减如何避免超卖？
- 秒杀是否需要 Redis 预扣库存？
- 订单创建和消息发布如何保证最终一致？
- 用户限购规则应该放在哪里？

## 10. 可观测性与健康检查

`ServiceDefaults` 通常承载：

- 健康检查端点。
- OpenTelemetry tracing、metrics。
- 服务发现。
- HTTP 客户端 resilience。

本项目的 `AppHost` 使用：

```csharp
.WithHttpHealthCheck("/health")
```

学习重点：

- 健康检查让 Aspire Dashboard 知道服务是否可用。
- OpenTelemetry 用于统一采集日志、指标和链路追踪。
- 服务发现让调用方使用逻辑服务名，而不是写死 IP 和端口。

## 11. 关键设计取舍

当前项目适合作为学习示例，有几处值得进一步思考：

- `OrderAppService` 和 `SubmitOrderCommandHandler` 有部分重复逻辑，可以统一入口。
- CAP 当前使用 in-memory queue/storage，服务重启后消息不会持久保存。
- 订单保存和库存保存分两步，生产环境要考虑事务边界和并发控制。
- 商品库存扣减在领域模型中表达规则，但数据库层还需要并发安全保障。
- Web 直接引用 Application DTO，开发方便，但大型系统可能会给前端单独定义契约。

## 12. 推荐练习

1. 给 `Product.Allocate` 写单元测试，覆盖库存不足、数量非法、商品下架。
2. 给 `Order.Submit` 写单元测试，验证订单行、总价和领域事件。
3. 给 `SubmitOrderCommandHandler` 增加测试，使用内存仓储模拟下单。
4. 将 CAP in-memory storage 替换为持久化存储。
5. 给订单增加取消功能：`Order.Cancel()`，并发布 `OrderCancelledDomainEvent`。
6. 给商品增加分类或 SKU，练习聚合边界设计。
7. 给秒杀加入用户限购规则。

## 13. 常用命令

还原依赖：

```bash
cd DDD.AspireShop
dotnet restore
```

构建项目：

```bash
cd DDD.AspireShop
dotnet build
```

从 Aspire 启动：

```bash
cd DDD.AspireShop
dotnet run --project DDD.AspireShop.AppHost
```

只启动 API：

```bash
cd DDD.AspireShop
dotnet run --project DDD.AspireShop.ApiService
```

只启动 Web：

```bash
cd DDD.AspireShop
dotnet run --project DDD.AspireShop.Web
```

