# Food Court Manager Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a rental-type-aware food-court manager flow so admins can classify rented slots as `Shop` or `FoodCourt`, managers are routed to the correct business-management screen, and the public food-court UI reads real stall and menu data from backend.

**Architecture:** Extend the existing Users rental flow with a persisted `BusinessType`, then evolve the current FoodCourt aggregate from a loose public catalog record into a manager-owned food stall with child menu items. Keep admin responsibilities inside `Rental Areas`, add one manager entry point that resolves to shop or food-court management, and replace frontend-generated food menu content with backend responses.

**Tech Stack:** ASP.NET Core Web API, EF Core, xUnit, React, Vite, Vitest, React Router, Tailwind CSS

---

## File Structure

### Backend rental/business type

- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Domain/Entities/RentalArea.cs`
  Persist `BusinessType` on rental slots.
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Persistence/Configurations/RentalAreaConfiguration.cs`
  Configure the new column and constraints.
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Persistence/Migrations/<timestamp>_AddRentalBusinessType.cs`
  Add EF Core migration for `BusinessType`.
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RegisterTenantDto.cs`
  Accept admin-selected business type during tenant registration.
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RentalAreas/RentalAreaResponseDto.cs`
  Return `BusinessType` in list responses.
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RentalAreas/RentalAreaDetailResponseDto.cs`
  Return `BusinessType` in detail responses.
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/RentalAreas/RentalAreaCommandService.cs`
  Validate and persist `BusinessType` during tenant registration and clear it correctly on cancel if needed by the domain rule.
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/RentalAreaCommandServiceTests.cs`
  Add tests for business-type validation and persistence.

### Backend manager business routing

- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/PublicCatalog/ManagerBusinessRouteDto.cs`
  Return the resolved manager entry target.
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/IManagerBusinessRouteService.cs`
  Contract for manager route resolution.
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/ManagerBusinessRouteService.cs`
  Determine whether a manager should go to shop or food-court management.
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/IManagerBusinessRouteRepository.cs`
  Read model contract for rental-type lookup.
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Repositories/ManagerBusinessRouteRepository.cs`
  Implement rental/business-type lookup from persisted manager/rental data.
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DependencyInjection.cs`
  Register new route service and repository.
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/ManagerBusinessController.cs`
  Expose the manager route-resolution endpoint.

### Backend food-court aggregate and APIs

- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Domain/Entities/FoodItem.cs`
  Evolve the top-level record into a food stall aggregate with manager-owned metadata.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Domain/Entities/FoodMenuItem.cs`
  Child entity for stall menu items.
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/FoodCourtDbContext.cs`
  Add `DbSet<FoodMenuItem>` and relationship configuration hookup.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Configurations/FoodMenuItemConfiguration.cs`
  Configure child table mapping and ordering.
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Configurations/FoodItemConfiguration.cs`
  Persist new stall columns and relationship to menu items.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Migrations/<timestamp>_AddFoodManagerAndMenuItems.cs`
  Add schema changes for stall ownership and menu items.
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/FoodItemDto.cs`
  Expand the public stall response.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodMenuItemDto.cs`
  Public and manager menu-item DTO.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodDetailDto.cs`
  Detailed stall response including menu items.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/UpsertFoodManagerRequestDto.cs`
  Manager-owned stall create/update DTO.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/UpsertFoodMenuItemRequestDto.cs`
  Menu-item create/update DTO.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodManagerCreationStatusDto.cs`
  Quota and available food-court rental locations for managers.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/IFoodManagerService.cs`
  Manager-owned stall and menu-item CRUD contract.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/FoodManagerService.cs`
  Implement ownership checks and quota logic.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/Validators/UpsertFoodManagerRequestDtoValidator.cs`
  Validate manager stall form input.
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/Validators/UpsertFoodMenuItemRequestDtoValidator.cs`
  Validate menu item input.
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/FoodController.cs`
  Add manager routes and richer public detail responses.

### Frontend rental/admin and manager routing

- Modify: `FRONTEND/src/features/auth/pages/RentalAreasAdmin.jsx`
  Add `businessType` selection to tenant registration and display business type in tables/details.
- Modify: `FRONTEND/src/features/auth/pages/RentalAreasAdmin.test.jsx`
  Lock the new field and request payload.
- Modify: `FRONTEND/src/features/auth/pages/Dashboard.jsx`
  Replace static shop-manager button logic with one dynamic business-management entry.
- Create: `FRONTEND/src/features/auth/services/managerBusinessApi.ts`
  Call the route-resolution endpoint.
- Modify: `FRONTEND/src/routes/AppRoutes.tsx`
  Register the new `FoodCourtManager` page route.
- Create: `FRONTEND/src/features/auth/pages/FoodCourtManager.jsx`
  Manager UI for stall profile and menu CRUD.
- Create: `FRONTEND/src/features/auth/pages/FoodCourtManager.test.jsx`
  Cover quota states and manager CRUD interactions.

### Frontend food public integration

- Modify: `FRONTEND/src/features/food/api/foodApi.ts`
  Expose public stall detail and manager CRUD functions.
- Modify: `FRONTEND/src/features/food/hooks/useFood.ts`
  Keep list loading stable with expanded DTOs.
- Modify: `FRONTEND/src/features/food/data/foodStoreMedia.ts`
  Narrow the file to preset/theme helpers and stop generating menu items as the primary source of truth.
- Modify: `FRONTEND/src/features/food/pages/FoodDetailPage.tsx`
  Render backend `menuItems`.
- Modify: `FRONTEND/src/features/food/pages/__tests__/FoodDetailPage.test.tsx`
  Assert backend menu rendering.

---

### Task 1: Persist Rental `BusinessType`

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Domain/Entities/RentalArea.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Persistence/Configurations/RentalAreaConfiguration.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RegisterTenantDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RentalAreas/RentalAreaResponseDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RentalAreas/RentalAreaDetailResponseDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/RentalAreas/RentalAreaCommandService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/RentalAreaCommandServiceTests.cs`
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Persistence/Migrations/<timestamp>_AddRentalBusinessType.cs`

- [ ] **Step 1: Write the failing backend tests for required and persisted business type**

```csharp
[Fact]
public async Task RegisterTenantAsync_returns_bad_request_when_business_type_is_missing()
{
    var repository = new FakeRentalAreaCommandRepository
    {
        RentalArea = new RentalArea { Id = "1", AreaCode = "FC-01", Status = "Available" },
        Manager = new User { Id = "manager-1", Role = "Manager", FullName = "Food Manager", CCCD = "012345678901" }
    };

    var service = new RentalAreaCommandService(null!, repository, new FakeFileStorageService());

    var result = await service.RegisterTenantAsync("1", new RegisterTenantDto
    {
        CCCD = "012345678901",
        Location = "FC-01",
        BusinessType = "",
        StartDate = DateTime.Today.AddDays(1),
        ElectricityFee = 3000m,
        WaterFee = 12000m,
        ServiceFee = 500000m,
        LeaseTermDays = 180,
        ContractImage = new FormFile(Stream.Null, 0, 0, "contract", "contract.png")
    });

    Assert.Equal(ApplicationResultStatus.BadRequest, result.Status);
    Assert.Equal("Business type is required", result.Error);
}

[Fact]
public async Task RegisterTenantAsync_persists_business_type_to_rental_area()
{
    var repository = new FakeRentalAreaCommandRepository
    {
        RentalArea = new RentalArea { Id = "1", AreaCode = "FC-01", Status = "Available" },
        Manager = new User { Id = "manager-1", Role = "Manager", FullName = "Food Manager", CCCD = "012345678901" }
    };

    var service = new RentalAreaCommandService(null!, repository, new FakeFileStorageService());

    var result = await service.RegisterTenantAsync("1", new RegisterTenantDto
    {
        CCCD = "012345678901",
        Location = "FC-01",
        BusinessType = "FoodCourt",
        StartDate = DateTime.Today.AddDays(1),
        ElectricityFee = 3000m,
        WaterFee = 12000m,
        ServiceFee = 500000m,
        LeaseTermDays = 180,
        ContractImage = new FormFile(Stream.Null, 0, 0, "contract", "contract.png")
    });

    Assert.Equal(ApplicationResultStatus.Ok, result.Status);
    Assert.Equal("FoodCourt", repository.RentalArea!.BusinessType);
}
```

- [ ] **Step 2: Run the targeted backend tests and verify they fail**

Run: `dotnet test BACKEND/ABCDMall.sln --filter RentalAreaCommandServiceTests`
Expected: FAIL because `RegisterTenantDto` and `RentalArea` do not define `BusinessType` yet.

- [ ] **Step 3: Add the new DTO and entity property, validation, and persistence mapping**

```csharp
public sealed class RegisterTenantDto
{
    public string CCCD { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public decimal ElectricityFee { get; set; }
    public decimal WaterFee { get; set; }
    public decimal ServiceFee { get; set; }
    public int LeaseTermDays { get; set; }
    public IFormFile? ContractImage { get; set; }
}

public class RentalArea
{
    public string? Id { get; set; } = Guid.NewGuid().ToString("N");
    public string AreaCode { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public string Status { get; set; } = "Available";
    public string? TenantName { get; set; }
    public string? ShopInfoId { get; set; }
    public string? BusinessType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

```csharp
if (string.IsNullOrWhiteSpace(dto.BusinessType))
{
    return ApplicationResult<MessageResponseDto>.BadRequest("Business type is required");
}

var normalizedBusinessType = dto.BusinessType.Trim();
if (normalizedBusinessType is not ("Shop" or "FoodCourt"))
{
    return ApplicationResult<MessageResponseDto>.BadRequest("Business type must be Shop or FoodCourt");
}

rentalArea.BusinessType = normalizedBusinessType;
```

```csharp
builder.Property(x => x.BusinessType)
    .HasMaxLength(32);
```

- [ ] **Step 4: Add and review the EF migration**

Run: `dotnet ef migrations add AddRentalBusinessType --project BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure --startup-project BACKEND/ABCDMall.WebAPI`
Expected: migration file adds a nullable `BusinessType` column to `RentalAreas`.

- [ ] **Step 5: Run the targeted backend tests again**

Run: `dotnet test BACKEND/ABCDMall.sln --filter RentalAreaCommandServiceTests`
Expected: PASS with the new business-type assertions green.

- [ ] **Step 6: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Domain/Entities/RentalArea.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Persistence/Configurations/RentalAreaConfiguration.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RegisterTenantDto.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RentalAreas/RentalAreaResponseDto.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RentalAreas/RentalAreaDetailResponseDto.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/RentalAreas/RentalAreaCommandService.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/RentalAreaCommandServiceTests.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Persistence/Migrations
git commit -m "feat: persist rental business type"
```

### Task 2: Expose Manager Business Routing

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/PublicCatalog/ManagerBusinessRouteDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/IManagerBusinessRouteService.cs`
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/ManagerBusinessRouteService.cs`
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/IManagerBusinessRouteRepository.cs`
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Repositories/ManagerBusinessRouteRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DependencyInjection.cs`
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/ManagerBusinessController.cs`
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/ManagerBusinessRouteServiceTests.cs`

- [ ] **Step 1: Write the failing service tests for route resolution**

```csharp
[Fact]
public async Task GetRouteAsync_returns_food_court_target_for_food_court_rental()
{
    var repository = new FakeManagerBusinessRouteRepository
    {
        Snapshot = new ManagerBusinessRouteSnapshot
        {
            OwnerShopId = "shop-info-1",
            BusinessType = "FoodCourt",
            HasEligibleRental = true
        }
    };

    var service = new ManagerBusinessRouteService(repository);

    var result = await service.GetRouteAsync("shop-info-1");

    Assert.Equal("FoodCourt", result.BusinessType);
    Assert.Equal("/food-court-manager", result.TargetPath);
}

[Fact]
public async Task GetRouteAsync_returns_shop_target_for_shop_rental()
{
    var repository = new FakeManagerBusinessRouteRepository
    {
        Snapshot = new ManagerBusinessRouteSnapshot
        {
            OwnerShopId = "shop-info-1",
            BusinessType = "Shop",
            HasEligibleRental = true
        }
    };

    var service = new ManagerBusinessRouteService(repository);

    var result = await service.GetRouteAsync("shop-info-1");

    Assert.Equal("Shop", result.BusinessType);
    Assert.Equal("/manager-shops", result.TargetPath);
}
```

- [ ] **Step 2: Run the targeted backend tests and verify they fail**

Run: `dotnet test BACKEND/ABCDMall.sln --filter ManagerBusinessRouteServiceTests`
Expected: FAIL because the new service, DTO, and repository contracts do not exist yet.

- [ ] **Step 3: Implement the route DTO, service, repository contract, and controller**

```csharp
public sealed class ManagerBusinessRouteDto
{
    public string BusinessType { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public bool HasEligibleRental { get; set; }
}

public sealed class ManagerBusinessRouteService : IManagerBusinessRouteService
{
    private readonly IManagerBusinessRouteRepository _repository;

    public ManagerBusinessRouteService(IManagerBusinessRouteRepository repository)
    {
        _repository = repository;
    }

    public async Task<ManagerBusinessRouteDto> GetRouteAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        var snapshot = await _repository.GetSnapshotAsync(ownerShopId, cancellationToken);
        if (snapshot is null || !snapshot.HasEligibleRental)
        {
            return new ManagerBusinessRouteDto
            {
                BusinessType = string.Empty,
                TargetPath = "/rental-areas",
                HasEligibleRental = false
            };
        }

        return new ManagerBusinessRouteDto
        {
            BusinessType = snapshot.BusinessType,
            TargetPath = snapshot.BusinessType == "FoodCourt" ? "/food-court-manager" : "/manager-shops",
            HasEligibleRental = true
        };
    }
}
```

```csharp
[Authorize(Roles = "Manager")]
[ApiController]
[Route("api/manager-business")]
public sealed class ManagerBusinessController : ControllerBase
{
    private readonly IManagerBusinessRouteService _service;

    public ManagerBusinessController(IManagerBusinessRouteService service)
    {
        _service = service;
    }

    [HttpGet("route")]
    public async Task<ActionResult<ManagerBusinessRouteDto>> GetRoute(CancellationToken cancellationToken)
    {
        var ownerShopId = User.FindFirstValue("shopId");
        if (string.IsNullOrWhiteSpace(ownerShopId))
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        return Ok(await _service.GetRouteAsync(ownerShopId, cancellationToken));
    }
}
```

- [ ] **Step 4: Run the targeted backend tests again**

Run: `dotnet test BACKEND/ABCDMall.sln --filter ManagerBusinessRouteServiceTests`
Expected: PASS with correct route mapping for `Shop` and `FoodCourt`.

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/PublicCatalog/ManagerBusinessRouteDto.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/IManagerBusinessRouteService.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/ManagerBusinessRouteService.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/IManagerBusinessRouteRepository.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Repositories/ManagerBusinessRouteRepository.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DependencyInjection.cs BACKEND/ABCDMall.WebAPI/Controllers/ManagerBusinessController.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/ManagerBusinessRouteServiceTests.cs
git commit -m "feat: resolve manager business route"
```

### Task 3: Add Food Stall Ownership and Menu Persistence

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Domain/Entities/FoodItem.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Domain/Entities/FoodMenuItem.cs`
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/FoodCourtDbContext.cs`
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Configurations/FoodItemConfiguration.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Configurations/FoodMenuItemConfiguration.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Migrations/<timestamp>_AddFoodManagerAndMenuItems.cs`
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/FoodItemDto.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodMenuItemDto.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodDetailDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/IFoodRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Repositories/FoodRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/IFoodQueryService.cs`
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/FoodQueryService.cs`

- [ ] **Step 1: Write the failing query test for backend menu items in food detail**

```csharp
[Fact]
public async Task GetBySlugAsync_returns_menu_items_for_food_stall_detail()
{
    var repository = new FakeFoodRepository();
    repository.Stalls.Add(new FoodItem
    {
        Id = "stall-1",
        OwnerShopId = "shop-info-1",
        Name = "Boba Bella",
        Slug = "boba-bella",
        Description = "Milk tea stall",
        ImageUrl = "/images/boba.png",
        CategorySlug = "drinks",
        MenuItems =
        [
            new FoodMenuItem
            {
                Id = "menu-1",
                FoodStallId = "stall-1",
                Name = "Brown Sugar Milk Tea",
                Price = 49000m,
                Note = "Best seller",
                Tag = "Signature",
                ImageUrl = "/images/milk-tea.png",
                IngredientsJson = "[\"Black tea\",\"Boba\"]",
                IsAvailable = true,
                DisplayOrder = 1
            }
        ]
    });

    var service = new FoodQueryService(repository);

    var result = await service.GetBySlugAsync("boba-bella");

    Assert.NotNull(result);
    Assert.Single(result!.MenuItems);
    Assert.Equal("Brown Sugar Milk Tea", result.MenuItems[0].Name);
}
```

- [ ] **Step 2: Run the targeted food query tests and verify they fail**

Run: `dotnet test BACKEND/ABCDMall.sln --filter FoodQueryService`
Expected: FAIL because the aggregate does not define menu items and detail DTOs yet.

- [ ] **Step 3: Implement the stall aggregate, child entity, repository methods, and DTO mapping**

```csharp
public class FoodItem
{
    public string Id { get; set; } = string.Empty;
    public string OwnerShopId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string MallSlug { get; set; } = "ABCD Mall";
    public string CategorySlug { get; set; } = "international";
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string OpenHours { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Promo { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<FoodMenuItem> MenuItems { get; set; } = [];
}

public class FoodMenuItem
{
    public string Id { get; set; } = string.Empty;
    public string FoodStallId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Note { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string IngredientsJson { get; set; } = "[]";
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; }
    public FoodItem? FoodStall { get; set; }
}
```

```csharp
public sealed class FoodDetailDto : FoodItemDto
{
    public string CategorySlug { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string OpenHours { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Promo { get; set; } = string.Empty;
    public IReadOnlyList<FoodMenuItemDto> MenuItems { get; set; } = [];
}
```

```csharp
return await _dbContext.FoodItems
    .AsNoTracking()
    .Include(x => x.MenuItems.OrderBy(menu => menu.DisplayOrder))
    .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
```

- [ ] **Step 4: Add and review the EF migration**

Run: `dotnet ef migrations add AddFoodManagerAndMenuItems --project BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure --startup-project BACKEND/ABCDMall.WebAPI`
Expected: migration adds new stall metadata columns plus a `FoodMenuItems` table.

- [ ] **Step 5: Run the targeted backend food tests again**

Run: `dotnet test BACKEND/ABCDMall.sln --filter Food`
Expected: PASS for the new detail/menu coverage and adjusted query tests.

- [ ] **Step 6: Commit**

```bash
git add BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Domain/Entities/FoodItem.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Domain/Entities/FoodMenuItem.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/FoodCourtDbContext.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Configurations/FoodItemConfiguration.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Configurations/FoodMenuItemConfiguration.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/Migrations BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/FoodItemDto.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodMenuItemDto.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodDetailDto.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/IFoodRepository.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Repositories/FoodRepository.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/IFoodQueryService.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/FoodQueryService.cs
git commit -m "feat: persist food stalls and menu items"
```

### Task 4: Implement Food-Court Manager Service and APIs

**Files:**
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/UpsertFoodManagerRequestDto.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/UpsertFoodMenuItemRequestDto.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodManagerCreationStatusDto.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/IFoodManagerService.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/FoodManagerService.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/Validators/UpsertFoodManagerRequestDtoValidator.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/Validators/UpsertFoodMenuItemRequestDtoValidator.cs`
- Modify: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DependencyInjection.cs`
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/FoodController.cs`
- Create: `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Tests/FoodManagerServiceTests.cs`

- [ ] **Step 1: Write the failing manager service tests for quota and ownership**

```csharp
[Fact]
public async Task CreateMyFoodStallAsync_rejects_when_manager_has_no_available_food_court_slot()
{
    var repository = new FakeFoodRepository
    {
        FoodCourtRentalCount = 1,
        ManagedFoodStallCount = 1
    };
    var service = new FoodManagerService(repository);

    await Assert.ThrowsAsync<InvalidOperationException>(() =>
        service.CreateMyFoodStallAsync("shop-info-1", new UpsertFoodManagerRequestDto
        {
            Name = "Boba Bella",
            Slug = "boba-bella",
            Description = "Milk tea stall",
            CategorySlug = "drinks",
            Location = "F1-FC-01",
            OpenHours = "09:00 - 22:00"
        }));
}

[Fact]
public async Task AddMenuItemAsync_rejects_when_stall_is_not_owned_by_manager()
{
    var repository = new FakeFoodRepository
    {
        ManagedStall = new FoodItem { Id = "stall-1", OwnerShopId = "shop-info-2", Name = "Other Stall" }
    };
    var service = new FoodManagerService(repository);

    var result = await service.AddMenuItemAsync("shop-info-1", "stall-1", new UpsertFoodMenuItemRequestDto
    {
        Name = "Milk Tea",
        Price = 49000m,
        Note = "Best seller",
        Tag = "Signature",
        Ingredients = ["Black tea", "Boba"]
    });

    Assert.Null(result);
}
```

- [ ] **Step 2: Run the targeted backend tests and verify they fail**

Run: `dotnet test BACKEND/ABCDMall.sln --filter FoodManagerServiceTests`
Expected: FAIL because the manager service and DTOs do not exist yet.

- [ ] **Step 3: Implement manager DTOs, service logic, validators, and controller endpoints**

```csharp
public sealed class FoodManagerCreationStatusDto
{
    public int StallCount { get; set; }
    public int RentedFoodCourtCount { get; set; }
    public bool CanCreate { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<AvailableRentalLocationDto> AvailableRentalLocations { get; set; } = [];
}
```

```csharp
public async Task<FoodManagerCreationStatusDto> GetCreationStatusAsync(string ownerShopId, CancellationToken cancellationToken = default)
{
    var rentedFoodCourtCount = await _repository.CountFoodCourtRentalsAsync(ownerShopId, cancellationToken);
    var stallCount = await _repository.CountManagedStallsAsync(ownerShopId, cancellationToken);
    var available = await _repository.GetAvailableFoodCourtLocationsAsync(ownerShopId, cancellationToken);

    return new FoodManagerCreationStatusDto
    {
        StallCount = stallCount,
        RentedFoodCourtCount = rentedFoodCourtCount,
        CanCreate = stallCount < rentedFoodCourtCount,
        Message = stallCount < rentedFoodCourtCount
            ? "You can create a food stall for an available food-court rental."
            : "All rented food-court slots already have a managed stall.",
        AvailableRentalLocations = available
    };
}
```

```csharp
[Authorize(Roles = "Manager")]
[HttpGet("manager")]
public async Task<ActionResult<IReadOnlyList<FoodDetailDto>>> GetMyFoodStalls(CancellationToken cancellationToken)
{
    var ownerShopId = User.FindFirstValue("shopId");
    if (string.IsNullOrWhiteSpace(ownerShopId))
    {
        return BadRequest("Manager account does not have a shop id.");
    }

    return Ok(await _foodManagerService.GetMyFoodStallsAsync(ownerShopId, cancellationToken));
}
```

- [ ] **Step 4: Run the targeted backend tests again**

Run: `dotnet test BACKEND/ABCDMall.sln --filter FoodManagerServiceTests`
Expected: PASS with quota and ownership coverage green.

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/UpsertFoodManagerRequestDto.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/UpsertFoodMenuItemRequestDto.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DTOs/Foods/FoodManagerCreationStatusDto.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/IFoodManagerService.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/FoodManagerService.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/Validators/UpsertFoodManagerRequestDtoValidator.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/Services/Foods/Validators/UpsertFoodMenuItemRequestDtoValidator.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DependencyInjection.cs BACKEND/ABCDMall.WebAPI/Controllers/FoodController.cs BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Tests/FoodManagerServiceTests.cs
git commit -m "feat: add food court manager backend"
```

### Task 5: Update Admin Rental UI and Manager Dashboard Routing

**Files:**
- Modify: `FRONTEND/src/features/auth/pages/RentalAreasAdmin.jsx`
- Modify: `FRONTEND/src/features/auth/pages/RentalAreasAdmin.test.jsx`
- Modify: `FRONTEND/src/features/auth/pages/Dashboard.jsx`
- Create: `FRONTEND/src/features/auth/services/managerBusinessApi.ts`
- Modify: `FRONTEND/src/routes/AppRoutes.tsx`

- [ ] **Step 1: Write the failing frontend tests for business type and dynamic routing**

```jsx
it("submits businessType when registering a tenant", async () => {
  render(<RentalAreasAdmin />);

  await userEvent.click(screen.getByRole("button", { name: /register tenant/i }));
  await userEvent.selectOptions(screen.getByLabelText(/business type/i), "FoodCourt");
  await userEvent.click(screen.getByRole("button", { name: /submit rental/i }));

  expect(api.put).toHaveBeenCalledWith(
    expect.stringContaining("/register-tenant"),
    expect.any(FormData),
    expect.any(Object),
  );
});

it("shows a single business management entry for managers", async () => {
  localStorage.setItem("role", "Manager");
  vi.mocked(getManagerBusinessRoute).mockResolvedValue({
    businessType: "FoodCourt",
    targetPath: "/food-court-manager",
    hasEligibleRental: true,
  });

  render(<DashboardMall />);

  expect(await screen.findByRole("link", { name: /my business management/i })).toHaveAttribute(
    "href",
    "/food-court-manager",
  );
});
```

- [ ] **Step 2: Run the targeted frontend tests and verify they fail**

Run: `pnpm --dir FRONTEND test RentalAreasAdmin.test.jsx Dashboard`
Expected: FAIL because the business-type selector and route service do not exist yet.

- [ ] **Step 3: Implement the rental form field, API call, dynamic manager button, and route registration**

```jsx
const initialRentalForm = {
  cccd: "",
  managerName: "",
  shopName: "",
  location: "",
  businessType: "Shop",
  startDate: "",
  electricityFee: "",
  waterFee: "",
  serviceFee: "",
  leaseTermDays: "",
};
```

```jsx
<div>
  <label className="mb-2 block text-sm font-semibold text-slate-700">Business Type</label>
  <select
    value={rentalForm.businessType}
    onChange={(event) => updateRentalForm("businessType", event.target.value)}
    className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
  >
    <option value="Shop">Shop</option>
    <option value="FoodCourt">FoodCourt</option>
  </select>
</div>
```

```jsx
formData.append("businessType", rentalForm.businessType);
```

```ts
export async function getManagerBusinessRoute() {
  return api.get<{
    businessType: string;
    targetPath: string;
    hasEligibleRental: boolean;
  }>("/manager-business/route");
}
```

- [ ] **Step 4: Run the targeted frontend tests again**

Run: `pnpm --dir FRONTEND test RentalAreasAdmin.test.jsx Dashboard`
Expected: PASS with correct payload and one dynamic business-management entry.

- [ ] **Step 5: Commit**

```bash
git add FRONTEND/src/features/auth/pages/RentalAreasAdmin.jsx FRONTEND/src/features/auth/pages/RentalAreasAdmin.test.jsx FRONTEND/src/features/auth/pages/Dashboard.jsx FRONTEND/src/features/auth/services/managerBusinessApi.ts FRONTEND/src/routes/AppRoutes.tsx
git commit -m "feat: route managers by rental business type"
```

### Task 6: Build the `FoodCourtManager` Frontend

**Files:**
- Create: `FRONTEND/src/features/auth/pages/FoodCourtManager.jsx`
- Create: `FRONTEND/src/features/auth/pages/FoodCourtManager.test.jsx`
- Modify: `FRONTEND/src/features/food/api/foodApi.ts`

- [ ] **Step 1: Write the failing frontend tests for stall and menu CRUD states**

```jsx
it("renders creation quota and available food-court locations", async () => {
  vi.mocked(getMyFoodCourtCreationStatus).mockResolvedValue({
    stallCount: 0,
    rentedFoodCourtCount: 1,
    canCreate: true,
    message: "You can create a food stall for an available food-court rental.",
    availableRentalLocations: [{ locationSlot: "FC-01", floor: "1", areaName: "Food Court East" }],
  });
  vi.mocked(getMyFoodStalls).mockResolvedValue([]);

  render(<FoodCourtManager />);

  expect(await screen.findByText(/food stall quota/i)).toBeInTheDocument();
  expect(screen.getByText(/food court east/i)).toBeInTheDocument();
});

it("submits menu items with the food stall form", async () => {
  vi.mocked(createMyFoodStall).mockResolvedValue({ id: "stall-1", name: "Boba Bella", menuItems: [] });
  render(<FoodCourtManager />);

  await userEvent.type(screen.getByLabelText(/stall name/i), "Boba Bella");
  await userEvent.type(screen.getByLabelText(/menu item name/i), "Brown Sugar Milk Tea");
  await userEvent.click(screen.getByRole("button", { name: /create food stall/i }));

  expect(createMyFoodStall).toHaveBeenCalled();
});
```

- [ ] **Step 2: Run the targeted frontend tests and verify they fail**

Run: `pnpm --dir FRONTEND test FoodCourtManager.test.jsx`
Expected: FAIL because the page and related API functions do not exist yet.

- [ ] **Step 3: Implement the page and API helpers using the `ManagerShops` interaction pattern**

```ts
export async function getMyFoodStalls() {
  return api.get("/food/manager");
}

export async function getMyFoodCourtCreationStatus() {
  return api.get("/food/manager/creation-status");
}

export async function createMyFoodStall(request: FormData) {
  return api.post("/food/manager", request);
}

export async function updateMyFoodStall(id: string, request: FormData) {
  return api.put(`/food/manager/${id}`, request);
}

export async function deleteMyFoodStall(id: string) {
  return api.delete(`/food/manager/${id}`);
}
```

```jsx
<Route path="/food-court-manager" element={<FoodCourtManager />} />
```

```jsx
<button
  type="button"
  onClick={addMenuItem}
  className="rounded-full border border-slate-300 bg-white px-4 py-2 text-sm font-semibold text-slate-700"
>
  Add Menu Item
</button>
```

- [ ] **Step 4: Run the targeted frontend tests again**

Run: `pnpm --dir FRONTEND test FoodCourtManager.test.jsx`
Expected: PASS with quota loading and form submission states covered.

- [ ] **Step 5: Commit**

```bash
git add FRONTEND/src/features/auth/pages/FoodCourtManager.jsx FRONTEND/src/features/auth/pages/FoodCourtManager.test.jsx FRONTEND/src/features/food/api/foodApi.ts FRONTEND/src/routes/AppRoutes.tsx
git commit -m "feat: add food court manager ui"
```

### Task 7: Switch Public Food Detail to Backend Menu Data

**Files:**
- Modify: `FRONTEND/src/features/food/data/foodStoreMedia.ts`
- Modify: `FRONTEND/src/features/food/pages/FoodDetailPage.tsx`
- Modify: `FRONTEND/src/features/food/pages/__tests__/FoodDetailPage.test.tsx`
- Modify: `FRONTEND/src/features/food/hooks/useFood.ts`
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/FoodController.cs`

- [ ] **Step 1: Write the failing frontend test for backend-rendered menu items**

```tsx
it("renders backend menu items instead of generated fallback labels", async () => {
  vi.mocked(getFoodBySlug).mockResolvedValue({
    id: "stall-1",
    name: "Boba Bella",
    slug: "boba-bella",
    description: "Milk tea stall",
    imageUrl: "/images/boba.png",
    categorySlug: "drinks",
    location: "Food Court East",
    openHours: "09:00 - 22:00",
    phone: "1900 1234",
    promo: "Buy 1 get 1",
    menuItems: [
      {
        id: "menu-1",
        name: "Brown Sugar Milk Tea",
        price: 49000,
        note: "Best seller",
        tag: "Signature",
        imageUrl: "/images/milk-tea.png",
        ingredients: ["Black tea", "Boba"],
        isAvailable: true,
      },
    ],
  });

  render(<FoodDetailPage />);

  expect(await screen.findByText("Brown Sugar Milk Tea")).toBeInTheDocument();
});
```

- [ ] **Step 2: Run the targeted frontend test and verify it fails**

Run: `pnpm --dir FRONTEND test FoodDetailPage.test.tsx`
Expected: FAIL because the page still derives menu content from `buildFoodMenu`.

- [ ] **Step 3: Update the detail page to consume backend `menuItems` and narrow the preset helper to theme/fallback responsibilities**

```ts
export type FoodMenuItemDto = {
  id: string;
  name: string;
  price: number;
  note: string;
  tag: string;
  imageUrl: string;
  ingredients: string[];
  isAvailable: boolean;
};

export type FoodDetailDto = FoodDto & {
  categorySlug: string;
  location: string;
  openHours: string;
  phone: string;
  promo: string;
  menuItems: FoodMenuItemDto[];
};
```

```tsx
const menu = useMemo(
  () =>
    (food?.menuItems ?? [])
      .filter((item) => item.isAvailable)
      .map((item) => ({
        ...item,
        priceLabel: new Intl.NumberFormat("vi-VN").format(item.price) + " VND",
      })),
  [food],
);
```

- [ ] **Step 4: Run the targeted frontend test again**

Run: `pnpm --dir FRONTEND test FoodDetailPage.test.tsx`
Expected: PASS with real menu-item content rendered from backend data.

- [ ] **Step 5: Commit**

```bash
git add FRONTEND/src/features/food/data/foodStoreMedia.ts FRONTEND/src/features/food/pages/FoodDetailPage.tsx FRONTEND/src/features/food/pages/__tests__/FoodDetailPage.test.tsx FRONTEND/src/features/food/hooks/useFood.ts FRONTEND/src/features/food/api/foodApi.ts BACKEND/ABCDMall.WebAPI/Controllers/FoodController.cs
git commit -m "feat: drive food detail menu from backend"
```

### Task 8: Full Verification

**Files:**
- Modify: none
- Test: `BACKEND/ABCDMall.sln`
- Test: `FRONTEND`

- [ ] **Step 1: Run the focused backend suite for rental, food manager, and food query coverage**

Run: `dotnet test BACKEND/ABCDMall.sln --filter "RentalAreaCommandServiceTests|ManagerBusinessRouteServiceTests|FoodManagerServiceTests|Food"`
Expected: PASS for all changed backend slices.

- [ ] **Step 2: Run the focused frontend suite for routing, rental admin, manager page, and public food detail**

Run: `pnpm --dir FRONTEND test RentalAreasAdmin.test.jsx FoodCourtManager.test.jsx FoodDetailPage.test.tsx AppRoutes.test.tsx`
Expected: PASS for the feature slice touched by this plan.

- [ ] **Step 3: Run the broader quality gates**

Run: `pnpm --dir FRONTEND build`
Expected: PASS with no TypeScript or Vite build failures.

Run: `dotnet test BACKEND/ABCDMall.sln`
Expected: PASS across the full backend solution.

- [ ] **Step 4: Commit the final verification sweep if code changed during fixes**

```bash
git add -A
git commit -m "test: finalize food court manager verification"
```

## Self-Review

### Spec Coverage Check

- Rental `BusinessType`: covered in Task 1.
- Dynamic manager routing: covered in Task 2 and Task 5.
- Reuse existing FoodCourt module as stall aggregate: covered in Task 3.
- One food-court slot equals one stall with quota rules: covered in Task 4.
- FoodCourtManager UI: covered in Task 6.
- Backend-driven public food detail menu: covered in Task 7.
- No new `AdminManagement` tool: preserved by limiting admin UI work to `RentalAreasAdmin` in Task 5.

### Placeholder Scan

- No `TBD`, `TODO`, or "similar to previous task" placeholders remain.
- Each task includes explicit files, code snippets, commands, and expected outcomes.

### Type Consistency Check

- `BusinessType` is used consistently as `Shop | FoodCourt`.
- `FoodItem` remains the top-level stall aggregate across Tasks 3 through 7.
- `FoodMenuItem` is the only child menu entity name used in the plan.
- Manager route DTO consistently returns `businessType`, `targetPath`, and `hasEligibleRental`.
