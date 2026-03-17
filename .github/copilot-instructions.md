# ISpanShop Copilot Instructions

## Build & Run

```bash
# Build entire solution
dotnet build ISpanShop.sln

# Run the admin back-office (main runnable project)
dotnet run --project ISpanShop.MVC

# Build a single project
dotnet build ISpanShop.MVC/ISpanShop.MVC.csproj
```

No test projects exist in this solution.

## Architecture

Five-layer architecture targeting **net8.0** with SQL Server:

```
ISpanShop.Common        → Shared utilities (EcpayHelper, SecurityHelper, Enums)
ISpanShop.Models        → EF Core entities (EfModels/) + DTOs (DTOs/)
ISpanShop.Repositories  → Data access — interfaces + implementations, grouped by domain
ISpanShop.Services      → Business logic — interfaces + implementations, grouped by domain
ISpanShop.MVC           → Admin back-office MVC + frontend RESTful API controllers
ISpanShop.WebAPI        → Folder only (no .csproj); SignalR ChatHub lives here
```

Dependency direction: `MVC/WebAPI → Services → Repositories → Models → Common`

### ISpanShop.MVC

Two kinds of controllers co-exist inside this single project:

- **Admin back-office** under `Areas/Admin/Controllers/<Domain>/` — Razor MVC, all inherit `AdminBaseController` (`[Area("Admin")] [Authorize]`). Controllers requiring SuperAdmin access add `[Authorize(Roles = "SuperAdmin")]`. The default route redirects to the Orders dashboard.
- **Frontend REST API** under `Controllers/Api/<Domain>/` — JSON API used by the front-end SPA (products, categories, seller inventory, etc.).

```
Areas/Admin/
  Controllers/<Domain>/   ← AdminBaseController subclasses
  Models/<Domain>/        ← ViewModels (Vm suffix)
  Views/<Domain>/
Controllers/Api/<Domain>/ ← RESTful API controllers (no Area)
```

`Program.cs` runs schema migrations as raw SQL at startup using the `IF NOT EXISTS … ALTER TABLE` pattern — there are no EF migrations.

### Data Access: EF Core vs ADO.NET

The repository layer mixes two approaches:

- **EF Core** (`ISpanShopDBContext`) — used by most repositories (Members, Products, LoginHistory, etc.). The context and entity models are scaffolded via EF Power Tools (`efpt.config.json`). Entity configuration uses **Fluent API only** (no Data Annotations on EF models).
- **ADO.NET** (`SqlConnection` / `SqlCommand`) — used by `AdminRepository` and `AdminRoleRepository`. These repositories inject `IConfiguration` and build queries manually.

`ISpanShopDBContext` lives in `ISpanShop.Models.EfModels`.

## Key Conventions

### Naming

| Artifact | Suffix/Prefix | Example |
|---|---|---|
| EF entity | (none) | `User`, `Product` |
| DTO | `Dto` | `MemberDto`, `AdminDto` |
| ViewModel (MVC) | `Vm` | `MemberIndexVm`, `AdminIndexVm` |
| Repository interface | `I…Repository` | `IMemberRepository` |
| Repository implementation | `…Repository` | `MemberRepository` |
| Service interface | `I…Service` | `IAdminService` |
| Service implementation | `…Service` | `AdminService` |

### File Layout

- DTOs: `ISpanShop.Models/DTOs/<Domain>/`
- Repository interfaces + implementations: `ISpanShop.Repositories/<Domain>/` (some domains use an `Implementations/` subfolder)
- Service interfaces + implementations: `ISpanShop.Services/<Domain>/`
- Admin ViewModels: `ISpanShop.MVC/Areas/Admin/Models/<Domain>/`

### Service Layer

Services receive DTOs/primitives from controllers and return DTOs. They are responsible for Entity→DTO mapping and business-rule enforcement. They never expose EF entities to the MVC layer.

Not all services have interfaces — some (e.g., `MemberService`, `PaymentService`, `CheckoutService`, `PointService`) are registered and injected as concrete classes. When an interface exists, always depend on the interface. New services should match the pattern already used in that domain.

### Pagination

Use `PagedResult<T>` from `ISpanShop.Models.DTOs.Common` to return paged data from services. The MVC layer wraps it in a ViewModel that also holds search/sort state.

### Authentication

Cookie authentication (`ISpanShop.Admin.Auth`, 8-hour sliding expiration). Login/logout/access-denied routes all point to `/Admin/Account/...`. Roles in the database: `SuperAdmin`, `Admin`, and member-level roles.

### Payments

ECPay integration lives in `ISpanShop.Common/EcpayHelper.cs`. NewebPay integration is in `ISpanShop.Services/Payments/NewebPayService.cs`. Payment callbacks are handled by `ISpanShop.WebAPI`.

### DI Registration

All repositories and services are registered as `AddScoped` in `ISpanShop.MVC/Program.cs`. When adding a new domain feature, register both the interface→implementation pairs there.

### Database Seeding

`ISpanShop.Models/Seeding/DataSeeder.cs` is called on every startup. It seeds admin users and test products. For development, the app ensures at least 15 pending-review products exist.

## Connection String

Set `DefaultConnection` in `appsettings.json` (or `appsettings.Development.json`):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ISpanShop;User Id=sa;Password=your_password;"
}
```
