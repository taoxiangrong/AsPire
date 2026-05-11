# DDD框架 · .NET Core 学习路线图

> 领域驱动设计 | CQRS | 微服务 | 完整技术生态 · 2026版

---

## 第一阶段：基础筑基（1-2个月）

### 1.1 C# 核心语法与 .NET 运行时
- 变量类型、控制结构、数组与集合
- 面向对象：类、继承、多态、封装、抽象
- 高级特性：泛型、LINQ、委托与事件、扩展方法
- 异步编程：async/await、Task 并行库
- 异常处理：try-catch-finally、自定义异常
- .NET 运行时：CLR 基础、内存管理、垃圾回收
- 常用工具包：`System.Text.Json`、`System.Threading.Channels`

### 1.2 ASP.NET Core 基础
- 配置系统（appsettings.json、环境变量、User Secrets）
- 依赖注入（DI）：三种生命周期（Singleton/Scoped/Transient）
- 中间件管道（Middleware）
- MVC/Razor Pages：控制器、视图、模型绑定、路由
- 过滤器：异常过滤器、动作过滤器、授权过滤器
- 日志系统：ILogger 接口
- Swagger/OpenAPI 自动生成文档
- CORS 跨源配置

### 1.3 ORM 核心：Entity Framework Core
- DbContext、实体映射、数据迁移
- 关系配置：一对多、多对多、外键约束
- 查询优化：Include/ThenInclude 预加载、避免 N+1 问题
- 事务管理：SaveChanges 隐式事务、显式事务
- 仓储模式基础
- **EF Core 8 新特性**：
  - `ComplexProperty`：值对象映射（优于 Owned Entity）
  - 数值类型行版本号（ulong 等）

### 1.4 项目工程化基础
- Git 版本控制 + Commit Message 规范
- 单元测试入门：xUnit + Moq
- 理解 .csproj、解决方案（Solution）与项目（Project）
- NuGet 包管理

> ✅ **阶段产出**：完成 2-3 个 CRUD 应用（如博客系统、简易电商），掌握 .NET Core 全栈开发基础。

---

## 第二阶段：DDD 核心理论（2个月）

### 2.1 战略设计
- 领域、子域（核心域 / 支撑域 / 通用域）
- 限界上下文（Bounded Context）
- 统一语言（Ubiquitous Language）
- 上下文映射（Context Mapping）：
  - 共享内核（Shared Kernel）
  - 客户-供应商（Customer-Supplier）
  - 防腐层（Anti-Corruption Layer）
- 事件风暴（Event Storming）

### 2.2 战术设计（核心构造块）
| 构造块 | 说明 | 示例 |
|--------|------|------|
| 实体（Entity） | 有唯一标识 | Customer、Order |
| 值对象（Value Object） | 无标识，不可变 | Money、Address |
| 聚合（Aggregate） | 实体+值对象的集群 | Order 包含 OrderItems |
| 聚合根（Aggregate Root） | 聚合入口，保证事务边界 | Order |
| 领域服务（Domain Service） | 跨聚合的业务逻辑 | 风险评估服务 |
| 仓储（Repository） | 聚合的持久化抽象 | IOrderRepository |
| 工厂（Factory） | 复杂对象创建 | CustomerFactory |
| 领域事件（Domain Event） | 重要业务事件 | OrderSubmitted |

### 2.3 分层架构设计
- DDD 经典四层：
  - 用户接口层（Presentation）→ 应用层（Application）→ 领域层（Domain）→ 基础设施层（Infrastructure）
- 依赖倒置原则：领域层不依赖基础设施
- 整洁架构（Clean Architecture）/ 六边形架构

> ✅ **阶段产出**：能够独立完成一个领域的战略与战术设计（例如订单域、产品域）。

---

## 第三阶段：DDD 实战框架与插件生态（3-4个月）

### 3.1 MediatR（CQRS 核心）
- 消息传递模式：请求/响应、命令（Command）、查询（Query）、通知（Notification）
- Command 与 Query 分离（CQRS）
- Pipeline Behavior 处理横切关注点（日志、验证、性能）
- 领域事件发布：`INotificationHandler`
- **NuGet 包**：`MediatR`、`MediatR.Extensions.Microsoft.DependencyInjection`

### 3.2 对象映射库
| 方案 | 特点 | NuGet 包 |
|------|------|----------|
| AutoMapper | 功能丰富，Profile 配置，反射映射 | `AutoMapper` |
| Mapster | 高性能，表达式树，零反射 | `Mapster` + `Mapster.DependencyInjection` |
| Mapperly | 源码生成，零运行时开销，AOT 友好 | `Mapperly` |

### 3.3 数据验证：FluentValidation
- 解耦验证逻辑，链式规则定义
- 可与 MediatR Pipeline Behavior 集成实现自动验证
- **NuGet**：`FluentValidation`、`FluentValidation.DependencyInjection`

### 3.4 日志与监控体系
- **Serilog 结构化日志**：
  - Sinks：Console、File、Seq、ElasticSearch
- **ELK 堆栈**：Filebeat → Elasticsearch → Kibana
- **NuGet**：`Serilog.AspNetCore`、`Serilog.Sinks.ElasticSearch`

### 3.5 弹性与容错：Polly
- 策略：重试（Retry）、熔断（Circuit Breaker）、超时（Timeout）、降级（Fallback）
- 策略组合：`Policy.WrapAsync`
- 与 `IHttpClientFactory` 集成：`Microsoft.Extensions.Http.Polly`
- **NuGet**：`Polly`、`Microsoft.Extensions.Http.Polly`

### 3.6 分布式事务：CAP 框架
- 基于本地消息表 + 消息队列，实现最终一致性（Outbox Pattern）
- 内置事件总线，支持发布/订阅
- 消息队列适配：RabbitMQ、Kafka、Azure Service Bus
- 存储适配：SQL Server、MySQL、PostgreSQL
- **NuGet**：`DotNetCore.CAP` + 对应消息队列和存储包

### 3.7 DDD 专用框架选型
- **NetCorePal Cloud Framework**：强类型ID、MediatR、CAP、SkyAPM + OpenTelemetry，支持 .NET 8/9/10
- **ABP Framework**：企业级模块化 DDD，与 .NET Aspire 集成
- **ddddify**：轻量级 ASP.NET Core DDD 框架

### 3.8 工具链速查表
| 类别 | 推荐工具 | 核心 NuGet 包 |
|------|----------|----------------|
| 测试 | xUnit + Moq | `xunit`、`Moq` |
| 日志 | Serilog + ELK/Seq | `Serilog.AspNetCore` |
| 对象映射 | Mapster / AutoMapper | `Mapster` / `AutoMapper` |
| 验证 | FluentValidation | `FluentValidation` |
| CQRS | MediatR | `MediatR` |
| 分布式事务 | DotNetCore.CAP | `DotNetCore.CAP` + 队列/存储包 |
| 弹性治理 | Polly | `Polly` |
| API文档 | Swashbuckle.AspNetCore | `Swashbuckle.AspNetCore` |
| 缓存（内存） | IMemoryCache | `Microsoft.Extensions.Caching.Memory` |
| 缓存（分布式） | Redis | `Microsoft.Extensions.Caching.StackExchangeRedis` |

> ✅ **阶段产出**：按 DDD 架构开发一个完整功能模块（如订单、产品），使用上述技术栈完成实现与测试。

---

## 第四阶段：分布式与微服务进阶（2-3个月）

### 4.1 微服务核心概念
- 基于限界上下文的服务拆分
- API 网关：YARP（微软）、Ocelot
- 服务发现：Consul / Nacos / etcd
- 配置中心：Apollo、Nacos Configuration
- 链路追踪：OpenTelemetry、SkyAPM

### 4.2 Docker 容器化与编排
- Dockerfile 编写、多阶段构建、镜像优化
- docker-compose 多服务编排（API + Redis + RabbitMQ + SQL Server）
- Kubernetes 基础：Pod、Service、Deployment、Ingress

### 4.3 CI/CD 流水线
- GitHub Actions / Azure DevOps / GitLab CI
- 自动化构建、测试、部署

### 4.4 .NET Aspire
- 微软官方云原生微服务开发框架（2026 主流）
- AppHost 统一编排，内置 OpenTelemetry 仪表板
- 与 Clean Architecture + DDD + CQRS + 事件溯源 天然兼容
- 支持 Modular Monolith 逐步演进到微服务

> ✅ **阶段产出**：完成一个模块化单体项目或小型微服务系统（如电商的订单、产品、支付模块），使用 CAP 处理跨服务数据一致性。

---

## 第五阶段：高可用与云原生实践（持续迭代）

### 5.1 消息队列与异步通信
- RabbitMQ：交换机类型、死信队列
- Kafka：高吞吐量、分区策略
- 与 CAP 框架集成作为消息传输层

### 5.2 可靠事件模式与过期处理
- 本地消息表模式（Outbox Pattern）+ CAP 事务性保证
- 消息幂等性设计
- 死信队列与 TTL（Time To Live）监控

### 5.3 API 网关与安全
- YARP 反向代理
- JWT + IdentityServer / Duende IdentityServer
- 限流：`AspNetCore.RateLimiting`
- HTTPS/TLS 配置

### 5.4 高性能与可观测性
- 诊断工具：`dotnet-trace`、`dotnet-dump`
- Metrics：Prometheus + Grafana
- Tracing：OpenTelemetry + Jaeger
- 结构化日志 + 死信队列监控

> ✅ **持续学习**：参与开源贡献、每月技术博客、紧跟 .NET 官方更新。

---

## 📚 推荐学习资源

| 类型 | 资源 |
|------|------|
| **书籍** | 《领域驱动设计》（Eric Evans）<br/>《实现领域驱动设计》（Vaughn Vernon）<br/>《Cloud-Native Microservices with .NET Aspire》 |
| **官方文档** | Microsoft Learn .NET Core 路径<br/>EF Core 官方指南<br/>.NET Aspire 文档 |
| **开源项目** | eShopOnContainers（微软微服务 DDD 参考）<br/>ABP Framework<br/>NetCorePal Cloud Framework |
| **在线课程** | Pluralsight 进阶系列<br/>Coursera “.NET 8 Backend Bootcamp – Modulith, DDD & CQRS” |
| **社区** | .NET 中文社区、DDD China、Stack Overflow |

---

## 🧭 关键原则与建议

- **代码量驱动**：每个知识点手写代码完成闭环（文档 → 编码 → 调试 → 复盘）
- **深度优先**：理解 DDD 的 why，而非单纯照搬模式
- **领域优先，技术次之**：始终从业务问题出发设计模型
- **渐进增强**：从传统分层 → DDD + CQRS → 事件溯源 / 微服务
- **警惕过度设计**：保持聚合小而完整，避免“大聚合”反模式

---

> 💡 建议学习周期：6~9 个月，每个阶段完成至少一个配套 demo 或实战模块（订单系统、产品管理等）。  
> 推荐从“小型模块化单体 + DDD + MediatR + CAP”开始，再逐步引入微服务和 .NET Aspire。

**最后更新：2026年5月**