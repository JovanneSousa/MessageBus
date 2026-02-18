# 📦 Jovane.MessageBus

Uma biblioteca leve para comunicação assíncrona e Request/Response usando **RabbitMQ**, baseada em **Integration Events** e integrada ao ecossistema **.NET Dependency Injection**.

Permite implementar rapidamente:

- ✅ Publish/Subscribe (Event Driven)
- ✅ Request/Response via RabbitMQ
- ✅ Consumers desacoplados
- ✅ Integração com MediatR
- ✅ Serialização JSON automática
- ✅ Recuperação automática de conexão

---

## 🚀 Instalação

Via NuGet:

```bash
dotnet add package Jovane.MessageBus
```

---

## ⚙️ Configuração

Adicione no `appsettings.json`:

```json
{
  "rabbit": {
    "url": "amqp://guest:guest@localhost:5672",
    "exchange": "app.exchange"
  }
}
```

---

## 🔧 Registro no Dependency Injection

```csharp
builder.Services.AddRabbitConfiguration(builder.Configuration);
```

Isso registra automaticamente:

```csharp
IMessageBus
```

como Singleton.

---

## 📤 Publicando eventos (Publish)

Crie um evento:

```csharp
public class OrderCreatedEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
}
```

Publicação:

```csharp
await messageBus.PublishAsync(
    new OrderCreatedEvent { OrderId = Guid.NewGuid() },
    routingKey: "order.created",
    exchangeName: "orders"
);
```

---

## 🔄 Request / Response

### Request

```csharp
var response = await messageBus.RequestAsync<CreateOrderRequest, CreateOrderResponse>(
    request,
    exchange: "orders",
    routingKey: "order.create"
);
```

---

### Responder (Consumer)

```csharp
await messageBus.RespondAsync<CreateOrderRequest, CreateOrderResponse>(
    async request =>
    {
        return new CreateOrderResponse(...);
    });
```

A fila é criada automaticamente baseada no nome do request.

---

## 🧠 Conceitos

### IntegrationEvent
Evento base para comunicação entre serviços.

```csharp
public abstract class IntegrationEvent : Event
{
}
```

---

### ResponseMessage
Padroniza respostas incluindo validações.

```csharp
public class ResponseMessage
{
    public ValidationResult ValidationResult { get; set; }
}
```

---

## 🏗 Arquitetura

A biblioteca implementa:

- Exchange do tipo **Topic**
- CorrelationId automático
- Reply Queue temporária
- Persistent Messages
- Auto Recovery do RabbitMQ

Fluxo Request/Response:

```
Service A → Exchange → Queue → Consumer
                     ↓
                Response Queue
                     ↓
                  Service A
```

---

## 🔒 Recursos internos

- Serialização JSON camelCase
- ACK/NACK automático
- Retry via requeue
- Disposable consumer handler
- Reply queues exclusivas e temporárias

---

## 📋 Requisitos

- .NET 8+ (recomendado)
- RabbitMQ 3.12+

---

## 💡 Boas práticas

✅ Use IntegrationEvents pequenos  
✅ Evite enviar entidades completas  
✅ Prefira eventos imutáveis  
✅ Versione eventos quando necessário

---

## 📌 Exemplo de Arquitetura

Ideal para:

- Microservices
- Modular Monolith
- Event Driven Architecture
- CQRS

---

## 🧩 Dependências

- RabbitMQ.Client
- MediatR
- FluentValidation
- Microsoft.Extensions.*

---

## 👨‍💻 Autor

**Jovane Sousa**

GitHub:  
https://github.com/JovanneSousa/MessageBus

---

## 📄 Licença

MIT
