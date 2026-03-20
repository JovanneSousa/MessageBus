# 🚌 Jovane.MessageBus

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512bd4?logo=dotnet)](https://dotnet.microsoft.com/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com/)

A **Jovane.MessageBus** é uma biblioteca de infraestrutura de alto desempenho para .NET, projetada para simplificar a integração com **RabbitMQ** em arquiteturas de microserviços. Ela abstrai a complexidade do driver oficial, oferecendo uma API fluida para mensageria assíncrona e RPC (Remote Procedure Call).

---

## 🌟 Principais Recursos

-   **Gestão Automática de Topologia:** Criação dinâmica de *Exchanges*, *Queues* e *Bindings* no RabbitMQ antes de cada operação.
-   **Mensageria Tipada:** Suporte total a classes C# como mensagens de integração (herança de `IntegrationEvent`).
-   **Padrão RPC (Request/Response):** Implementação robusta de requisição e resposta sobre filas temporárias e exclusivas, com suporte a correlação de IDs.
-   **Resiliência Nativa:** Recuperação automática de conexões e canais via `RabbitMQ.Client`.
-   **Injeção de Dependência:** Registro simplificado no container nativo do .NET via `IServiceCollection`.
-   **Roteamento Configurável:** Desacoplamento total via mapeamento de eventos em `appsettings.json`.

---

## 📦 Instalação

Adicione o pacote ao seu projeto (exemplo via CLI):

```bash
dotnet add package Jovane.MessageBus
```

---

## ⚙️ Configuração

A biblioteca utiliza uma seção no `appsettings.json` para mapear os eventos às entidades do RabbitMQ. Isso permite que você mude a infraestrutura de filas sem alterar o código-fonte.

```json
{
  "rabbit": {
    "Url": "amqp://guest:guest@localhost:5672",
    "Exchange": {
      "UsuarioRegistradoIntegrationEvent": "auth-service-exchange",
      "EmailIntegrationEvent": "notification-service-exchange"
    },
    "Queue": {
      "UsuarioRegistradoIntegrationEvent": "registrar-usuario-fila",
      "EmailIntegrationEvent": "enviar-email-fila"
    },
    "RoutingKey": {
      "UsuarioRegistradoIntegrationEvent": "usuario.registrado",
      "EmailIntegrationEvent": "email.prioridade.alta"
    }
  }
}
```

No seu `Program.cs`:

```csharp
using Configuration;

// Registro da biblioteca
builder.Services.AddRabbitConfiguration(builder.Configuration);
```

---

## 📖 Como Usar

### 1. Definindo Mensagens

Todas as mensagens de integração devem herdar de `IntegrationEvent`.

```csharp
public class UsuarioRegistradoIntegrationEvent : IntegrationEvent
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}
```

### 2. Publicando Eventos (Pub/Sub)

Injete `IMessageBus` em seus serviços para disparar mensagens do tipo "fire and forget".

```csharp
public class AuthService(IMessageBus bus)
{
    public async Task Registrar(Usuario usuario)
    {
        // ... lógica de negócio
        
        var evento = new UsuarioRegistradoIntegrationEvent 
        { 
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email 
        };

        await bus.PublishAsync(evento);
    }
}
```

### 3. Padrão Request/Response (RPC)

Ideal para quando um microserviço precisa de uma resposta imediata ou validação de outro sistema de forma síncrona sobre mensageria.

#### Enviando a Requisição (Requester):
```csharp
// Enviando e aguardando a resposta
var response = await _messageBus.RequestAsync<MeuPedidoRequest, ResponseMessage>(meuPedido);

if (response.ValidationResult.IsValid) 
{
    // Sucesso na integração
}
```

#### Processando a Requisição (Responder):
```csharp
// Geralmente implementado em um BackgroundService ou Worker
await _messageBus.RespondAsync<MeuPedidoRequest, ResponseMessage>(async request => 
{
    // Lógica de processamento...
    var validacao = new ValidationResult(); // FluentValidation
    
    return new ResponseMessage(validacao);
});
```

---

## 🛠️ Arquitetura e Fluxo

A biblioteca utiliza o `IEventRouteResolver` para buscar as configurações. O fluxo segue estas etapas:

1.  **Resolução:** Identifica `Exchange`, `RoutingKey` e `Queue` baseando-se no **nome da classe** do evento.
2.  **Garantia de Infra (Ensure):** Declara a *Exchange* (Topic), a *Queue* (Durable) e o *Binding* automaticamente.
3.  **Serialização:** Utiliza `System.Text.Json` com `CamelCase` para padronização.
4.  **Execução:** Publica ou consome utilizando as propriedades de correlação (`CorrelationId`) e retorno (`ReplyTo`) quando necessário.

---

## 🧪 Dependências

-   **RabbitMQ.Client (7.2.0)** - Core da comunicação.
-   **FluentValidation** - Padronização das mensagens de resposta.
-   **MediatR** - Base para eventos e notificações.
-   **Microsoft.Extensions.Options** - Gerenciamento de configurações.

---

## 👨‍💻 Autor

**Jovane Sousa**  
Biblioteca criada para padronizar e facilitar a comunicação entre microserviços em ecossistemas .NET.

---
**Dica:** Sinta-se à vontade para contribuir com Pull Requests ou abrir Issues no repositório oficial! 🚀
