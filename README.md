# FIAP Cloud Games - Orders Service

Microsserviço responsável pelo **pedido de compra** de jogos do FIAP Cloud Games. É ele quem inicia a compra e coordena, por uma **saga** (MassTransit), o pagamento, a entrega do jogo à biblioteca e a conclusão do pedido.

## Finalidade e papel no sistema

- Recebe o pedido de compra (`POST /v1/purchase-order`), valida cada jogo e calcula o preço final.
- Consulta por HTTP o **Catalog** (jogo e preço), o **Promotions** (campanha) e o **Library** (o usuário já tem o jogo?).
- Publica eventos e coordena o fluxo por uma saga no Worker, com estado no PostgreSQL e Outbox do MassTransit.
- No desenho do grupo, o Orders cumpre o papel que o enunciado dá ao Catalog na compra: iniciar o fluxo e reagir ao resultado do pagamento.
- Tem banco próprio (PostgreSQL, banco `fcg_orders`).

## Rotas

Todas exigem token de serviço (veja "Autenticação").

| Método e rota | Scope | Finalidade |
|---|---|---|
| `POST /v1/purchase-order` | `orders.purchaseorder.create` | Cria um pedido de compra |
| `GET /v1/purchase-order/{id}` | `orders.purchaseorder.read` | Consulta o pedido |

Corpo de `POST /v1/purchase-order`:

```json
{
  "customerId": "<id do usuário>",
  "items": [
    { "productId": "<id do jogo no Catalog>", "promotionId": "<opcional>" }
  ]
}
```

Resposta (`PurchaseOrderResponse`): `id`, `customerId`, `createdAtUtc`. **O status do pedido não é devolvido pela API**; ele é mantido no banco e muda conforme a saga avança (veja "Estados do pedido").

Regras na criação (a API responde com erro de validação, 409 ou 422):

- o jogo precisa existir e estar ativo no Catalog;
- o usuário não pode já ter o jogo na biblioteca;
- o jogo não pode estar em outro pedido ativo do mesmo cliente (recebido, pagamento pendente ou pagamento aprovado);
- não pode haver itens duplicados no mesmo pedido;
- a `promotionId`, se informada, precisa existir e estar ativa.

O Swagger fica disponível em `/swagger` no ambiente `Development`.

## Autenticação

- **Quem chama o Orders:** token JWT de serviço do Identity (`POST /oauth2/token`, `client_credentials`), com audience `orders-api` e os scopes da tabela acima.
- **Quando o Orders chama os outros serviços:** ele pede o próprio token ao Identity, como cliente `orders-api`, com as audiences e scopes configurados em `CatalogApi`, `PromotionsApi` e `LibraryApi`.

## Fluxo da saga

```text
Orders API --HTTP--> Catalog, Promotions e Library (validação e preço)
Orders API --PurchaseOrderCreated--> saga
saga --PaymentRequested--> Payments
Payments --PaymentApproved | PaymentRejected--> saga
saga --GameGrantRequested (um por jogo)--> Library
Library --GameGranted--> saga
saga --OrderCompleted--> Notifications
```

- Pagamento rejeitado: a saga muda o pedido para "Pagamento Rejeitado" e termina. **Não publica `OrderFailed`**; o Notifications avisa a falha pelo evento do Payments.
- Se uma mensagem chega antes de a saga existir, ela é reentregue algumas vezes (intervalos de 1, 3 e 5 segundos) antes de ir para a fila de erro.

### Eventos e exchanges

| Evento | Sentido | Exchange |
|---|---|---|
| `PurchaseOrderCreatedEvent` | publica e consome | `orders.purchase-order-created.v1` |
| `PaymentRequestedEventV1` | publica | `payments.payment-requested.v1` |
| `PaymentApprovedEventV1` | consome | `payments.payment-approved.v1` |
| `PaymentRejectedEventV1` | consome | `payments.payment-rejected.v1` |
| `GameGrantRequestedEventV1` | publica (um por jogo) | `library.game-grant-requested.v1` |
| `GameGrantedEventV1` | consome | `library.game-granted.v1` |
| `OrderCompletedEventV1` | publica | `orders.order-completed.v1` |

Todos usam o envelope do MassTransit. Os contratos ficam em `FiapCloudGames.Orders.Contracts.Events.V1`.

### Estados do pedido

`Recebido` → `Pagamento Pendente` → `Pagamento Aprovado` → `Concluído`; ou `Pagamento Rejeitado`. Existem também `Falhou` e `Cancelado pelo usuário`.

Para acompanhar um pedido, use os logs do Worker (`Saga da ordem ... iniciada`, `Pagamento ... aprovado`, `Todos os ... jogos foram concedidos; ordem ... concluída`) ou o banco `fcg_orders`.

## Variáveis de ambiente

No ambiente do projeto os valores vêm do orquestrador (`config/*-orders-*.env` e Secrets); o `appsettings.json` do serviço fica vazio. `__` equivale a `:` na configuração do .NET.

### API (`orders-api`)

| Variável | Obrigatória | Onde fica no cluster | Exemplo | Para que serve |
|---|---|---|---|---|
| `ConnectionStrings__Database` | sim | Secret (`orders-connection-string`) | `Host=postgres;Database=fcg_orders;...` | Conexão com o PostgreSQL |
| `RabbitMq__Host` | sim | ConfigMap | `rabbitmq` | Host do RabbitMQ |
| `RabbitMq__Port` | não | ConfigMap | `5672` | Porta do RabbitMQ |
| `RabbitMq__VirtualHost` | não | ConfigMap | `/` | Virtual host |
| `RabbitMq__Username`, `RabbitMq__Password` | sim | Secret (`fcg-dev-secrets`) | - | Credenciais do broker |
| `RabbitMq__Topology__Exchanges__*` | sim | ConfigMap | `orders.purchase-order-created.v1` | Nome de cada exchange (veja a tabela de eventos) |
| `RabbitMq__Topology__Queues__PurchaseOrderSaga` | sim | ConfigMap | `fcg-orders-purchase-order-saga` | Fila da saga |
| `Authentication__Authority` | sim | ConfigMap | `http://identity-api:8080` | Identity que valida os tokens recebidos |
| `Authentication__Audience` | sim | ConfigMap | `orders-api` | Audience aceita |
| `Authentication__Issuer` | sim | ConfigMap | `fiap-cloud-games-identity` | Emissor esperado |
| `Authentication__AllowedTokenUses__0` | sim | ConfigMap | `service` | Só tokens de serviço |
| `ServiceAuthentication__Authority` | sim | ConfigMap | `http://identity-api:8080` | Identity de onde o Orders pede o próprio token |
| `ServiceAuthentication__TokenEndpoint` | sim | ConfigMap | `http://identity-api:8080/oauth2/token` | Endpoint do token |
| `ServiceAuthentication__ClientId` | sim | ConfigMap | `orders-api` | Cliente OAuth2 do Orders |
| `ServiceAuthentication__ClientSecret` | sim | Secret (`orders-client-secret`) | - | Segredo do cliente |
| `CatalogApi__BaseAddress`, `__Audience`, `__Scopes` | sim | ConfigMap | `http://catalog-api:8080/`, `catalog-api`, `catalog.games.read` | Chamada ao Catalog |
| `PromotionsApi__BaseAddress`, `__Audience`, `__Scopes` | sim | ConfigMap | `http://promotions-api:8080/`, `promotions-api`, `promotions.calculate promotions.campaigns.read` | Chamada ao Promotions |
| `LibraryApi__BaseAddress`, `__Audience`, `__Scopes` | sim | ConfigMap | `http://library-api:8080/`, `library-api`, `library.ownership.read` | Chamada ao Library |
| `ServiceAuthentication__RetryCount`, `__HttpTimeout`, `__RefreshBeforeExpiration` e demais `ServiceAuthentication__*` | não | ConfigMap | `2`, `00:00:10`, `00:00:30` | Retentativa, timeout e renovação do token |
| `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS` | não | ConfigMap | `Development`, `http://+:8080` | Ambiente e porta |

### Worker (`orders-worker`)

| Variável | Obrigatória | Onde fica | Para que serve |
|---|---|---|---|
| `ConnectionStrings__Database` | sim | Secret | Estado da saga e Outbox |
| `RabbitMq__Host`, `__Port`, `__VirtualHost` | sim / não | ConfigMap | Conexão com o RabbitMQ |
| `RabbitMq__Username`, `RabbitMq__Password` | sim | Secret | Credenciais do broker |
| `RabbitMq__Topology__Exchanges__*` e `RabbitMq__Topology__Queues__PurchaseOrderSaga` | sim | ConfigMap | Exchanges e fila (os mesmos da API) |
| `PurchaseOrderSaga__MissingInstanceRedeliveryIntervals__0..2` | não | ConfigMap | Intervalos de reentrega (`00:00:01`, `00:00:03`, `00:00:05`) |
| `DOTNET_ENVIRONMENT` | não | ConfigMap | Ambiente do Worker |

### Migrations (`orders-migrations`)

| Variável | Obrigatória | Onde fica | Para que serve |
|---|---|---|---|
| `ConnectionStrings__Database` (ou `ORDERS_MIGRATIONS_CONNECTION`) | sim | Secret | Conexão usada para aplicar as migrations |
| `DOTNET_ENVIRONMENT` | não | ConfigMap | Ambiente do executável |

## Estrutura do código

```text
src/
  FiapCloudGames.Orders.Api             # composition root e pipeline HTTP
  FiapCloudGames.Orders.Presentation    # controllers, requests, responses, políticas
  FiapCloudGames.Orders.Application     # caso de uso de criação e portas para os clientes HTTP
  FiapCloudGames.Orders.Domain          # pedido, itens, status
  FiapCloudGames.Orders.Infrastructure  # EF Core, repositórios, clientes HTTP, estado da saga
  FiapCloudGames.Orders.Contracts       # eventos de integração V1
  FiapCloudGames.Orders.Worker          # saga (PurchaseOrderStateMachine) e atividades
  FiapCloudGames.Orders.Migrations      # migrations e executável que aplica no banco

tests/
  FiapCloudGames.Orders.UnitTests
```

## Como executar

### Pela orquestração (recomendado)

O Orders sobe junto com os outros serviços pelo repositório do orquestrador (`fgc-orchestrator-service`, clonado como pasta `fcg-orchestration`):

```powershell
cd ..\fcg-orchestration
.\FCG-COMPOSE.cmd   # Orders em http://127.0.0.1:5086/swagger
.\FCG-K8S.cmd       # Orders em http://127.0.0.1:5186/swagger
```

### Testes

```powershell
dotnet test FiapCloudGames.Orders.slnx
```

## Docker

O `Dockerfile` é multi-stage e tem três alvos finais: `final` (API), `worker` (saga) e `migrations`.

## Kubernetes

A pasta `k8s/` tem `api.yaml`, `worker.yaml` e `migrations-job.yaml`. Os ConfigMaps e os Secrets são criados pelo instalador do orquestrador, a partir de `config/*.env`.
