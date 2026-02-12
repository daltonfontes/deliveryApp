# DeliveryApp API

API REST para gerenciamento de entregas, construida com .NET 8 e Clean Architecture.

## Tecnologias

- **.NET 8** (Minimal API)
- **Entity Framework Core 8** + PostgreSQL
- **ASP.NET Identity** + JWT Bearer Authentication
- **OpenTelemetry** (tracing, metrics, logging via OTLP)
- **Swashbuckle** (Swagger UI)

## Arquitetura

```
src/
├── DeliveryApp.Domain/          # Entidades, enums, interfaces de repositorio, exceções
├── DeliveryApp.Application/     # DTOs, interfaces de serviço, implementações de serviço
├── DeliveryApp.Data/            # DbContext, repositorios, configurações EF, Identity, AuthService
└── DeliveryApp.Adapter/         # Program.cs, endpoints, middleware, extensions
```

**Fluxo de dependências:** Domain ← Application ← Data ← Adapter

## Entidades

- **Customer** — cliente que realiza pedidos
- **Category** — categoria de produtos (1:N com Product)
- **Product** — produto disponivel para pedido
- **DeliveryDriver** — motorista de entrega
- **Order** / **OrderItem** — pedido com itens

## Endpoints

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| POST | `/api/auth/register` | Publico | Registrar usuario |
| POST | `/api/auth/login` | Publico | Login (retorna JWT) |
| GET | `/api/categories` | Publico | Listar categorias |
| GET | `/api/categories/{id}` | Publico | Buscar categoria |
| POST | `/api/categories` | Admin | Criar categoria |
| PUT | `/api/categories/{id}` | Admin | Atualizar categoria |
| DELETE | `/api/categories/{id}` | Admin | Remover categoria |
| GET | `/api/products` | Publico | Listar produtos |
| GET | `/api/products/active` | Publico | Listar produtos ativos |
| GET | `/api/products/{id}` | Publico | Buscar produto |
| POST | `/api/products` | Admin | Criar produto |
| PUT | `/api/products/{id}` | Admin | Atualizar produto |
| DELETE | `/api/products/{id}` | Admin | Remover produto |
| GET | `/api/customers` | Admin | Listar clientes |
| GET | `/api/customers/{id}` | Admin | Buscar cliente |
| POST | `/api/customers` | Admin | Criar cliente |
| PUT | `/api/customers/{id}` | Admin | Atualizar cliente |
| DELETE | `/api/customers/{id}` | Admin | Remover cliente |
| GET | `/api/delivery-drivers` | Admin | Listar motoristas |
| GET | `/api/delivery-drivers/available` | Admin | Listar motoristas disponiveis |
| GET | `/api/delivery-drivers/{id}` | Admin | Buscar motorista |
| POST | `/api/delivery-drivers` | Admin | Criar motorista |
| PUT | `/api/delivery-drivers/{id}` | Admin | Atualizar motorista |
| DELETE | `/api/delivery-drivers/{id}` | Admin | Remover motorista |
| GET | `/api/orders` | Admin | Listar pedidos |
| GET | `/api/orders/{id}` | Autenticado | Buscar pedido |
| GET | `/api/orders/customer/{id}` | Autenticado | Pedidos por cliente |
| POST | `/api/orders` | Customer | Criar pedido |
| PATCH | `/api/orders/{id}/status` | Admin | Atualizar status |
| PATCH | `/api/orders/{id}/assign-driver` | Admin | Atribuir motorista |
| DELETE | `/api/orders/{id}` | Admin | Remover pedido |

## Tratamento de Erros

Todas as respostas de erro seguem o formato **Problem Details (RFC 7807)**:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Customer with key '...' was not found."
}
```

| Exceção | HTTP Status |
|---------|-------------|
| `NotFoundException` | 404 |
| `ValidationException` | 400 (inclui campo `errors`) |
| `ConflictException` | 409 |
| `UnauthorizedException` | 401 |
| `DbUpdateException` (unique constraint) | 409 |
| Erro inesperado | 500 (mensagem generica) |

## Pre-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)

## Como rodar

1. **Configurar o banco** — edite a connection string em `src/DeliveryApp.Adapter/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=deliveryapp;Username=postgres;Password=postgres"
   }
   ```

2. **Aplicar migrations:**
   ```bash
   dotnet ef database update --project src/DeliveryApp.Data --startup-project src/DeliveryApp.Adapter
   ```

3. **Rodar a aplicacao:**
   ```bash
   dotnet run --project src/DeliveryApp.Adapter
   ```

4. **Acessar o Swagger:** `https://localhost:7001/swagger`

## Autenticacao

1. Registre um usuario via `POST /api/auth/register`:
   ```json
   {
     "email": "admin@example.com",
     "password": "SenhaForte123@",
     "fullName": "Admin",
     "role": "Admin"
   }
   ```

2. Use o token retornado no header `Authorization: Bearer <token>`

3. No Swagger, clique em **Authorize** e cole o token.

**Roles disponiveis:** `Admin`, `Customer`
