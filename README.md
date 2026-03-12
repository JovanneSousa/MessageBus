# 🚌 Jovane.MessageBus

Uma biblioteca de infraestrutura robusta e leve para facilitar a comunicação assíncrona em arquiteturas distribuídas .NET, utilizando **RabbitMQ**.

---

## 🌟 Funcionalidades

*   **Abstração do RabbitMQ:** Esconde a complexidade de canais, conexões e exchanges.
*   **Publish/Subscribe:** Disparo de eventos de integração para múltiplos consumidores.
*   **Request/Response:** Comunicação síncrona sobre mensageria (RPC), com filas de resposta temporárias e exclusivas.
*   **Injeção de Dependência:** Integração nativa com `IServiceCollection`.
*   **Resiliência:** Suporte a auto-recovery de conexão.
*   **Tipagem Forte:** Utiliza classes C# para definir eventos e mensagens de integração.

---

## 🏗️ Estrutura da Biblioteca

*   **Bus:** Contém a interface `IMessageBus` e sua implementação principal.
*   **Messages:** Define as classes base `IntegrationEvent` e `ResponseMessage`.
*   **Configuration:** Extensões para facilitar o registro da biblioteca no `Program.cs`.

---

## 🚀 Como Utilizar

### 1. Registro no Container de DI
No arquivo `Program.cs` de qualquer serviço:

```csharp
builder.Services.AddRabbitConfiguration(builder.Configuration);
```

### 2. Publicando um Evento (Fire and Forget)
```csharp
public class PedidoCriadoEvent : IntegrationEvent { ... }

// No seu serviço:
await _messageBus.PublishAsync(new PedidoCriadoEvent(pedidoId));
```

### 3. Padrão Request/Response (Síncrono via Bus)
Utilizado quando um serviço precisa de uma resposta imediata de outro sistema.

```csharp
// Enviando a requisição
var response = await _messageBus.RequestAsync<UsuarioRegistradoEvent, ResponseMessage>(meuEvento);

if (response.ValidationResult.IsValid) {
    // Sucesso na integração
}
```

---

## 📊 Fluxo de Trabalho (Request/Response)

```text
[Serviço A] --(Publica na Fila Principal)--> [Exchange] --> [Fila de Trabalho]
                                                                  |
                                                           [Serviço B (Consumidor)]
                                                                  |
[Serviço A] <--(Retorna via Fila de Resposta)-- [Exchange] <------|
```

---

## 🛠️ Tecnologias e Dependências

*   **RabbitMQ.Client:** Driver oficial do RabbitMQ para .NET.
*   **Polly:** (Opcional/Interno) Para políticas de retry e resiliência.
*   **Newtonsoft.Json / System.Text.Json:** Para serialização de mensagens.

---

## 👨‍💻 Autor

**Jovane Sousa**  
Biblioteca criada para padronizar a comunicação entre microserviços e estudos de arquitetura distribuída.
