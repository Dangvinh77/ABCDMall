# Food Court Manager Design

Date: 2026-04-29

## Summary

Extend the existing mall rental workflow so a rented slot can be classified as either `Shop` or `FoodCourt`, then route managers into the correct self-service management experience automatically.

For `FoodCourt` rentals, add a dedicated `FoodCourtManager` flow that mirrors the ownership and quota behavior of `ManagerShops`, but manages one food stall per rented food-court slot and exposes CRUD for the stall profile plus its menu items.

The public food-court UI should keep its current visual structure while switching from partially mocked menu data to backend-driven stall and menu data.

## Goals

- Add `businessType = Shop | FoodCourt` to the rental area workflow.
- Let admins assign the tenant business type during rental registration.
- Keep admin control inside `Rental Areas` instead of adding a new admin tool card.
- Route managers through a single business-management entry point that resolves to the correct manager screen based on rental business type.
- Reuse the existing FoodCourt module instead of creating a parallel module with overlapping responsibility.
- Treat the current public food-court records as food stalls, not standalone dishes.
- Support `1 rented FoodCourt slot = 1 managed food stall`.
- Add CRUD for food stall menu items from the manager UI.
- Drive the public food detail page menu from backend data.

## Non-Goals

- Creating a separate `Food Court Manager` section in `AdminManagement`.
- Supporting multiple food stalls under one food-court rental slot.
- Building a mall-wide content management system for food-court themes or marketing copy.
- Reworking the current public food-court page layout beyond what is needed for backend integration.
- Redesigning shop-manager behavior outside the routing and rental-type changes needed for coexistence.

## Current Context

### Rental and Manager Flows

- [RentalAreasAdmin.jsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/auth/pages/RentalAreasAdmin.jsx) currently registers tenants against rental slots, but does not store a business type.
- The Users module already contains rental area entities, DTOs, services, repositories, and tests that drive this workflow.
- [ManagerShops.jsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/auth/pages/ManagerShops.jsx) already implements the manager-owned CRUD pattern tied to rented areas and creation quota.

### FoodCourt Module

- The FoodCourt backend already exposes public list/detail and generic CRUD endpoints through [FoodController.cs](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/BACKEND/ABCDMall.WebAPI/Controllers/FoodController.cs).
- The current `FoodItem` domain model only stores `Name`, `Slug`, `Description`, `ImageUrl`, and lightweight category metadata.
- The public food list page in [FoodPage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/food/pages/FoodPage.tsx) already behaves like a food-stall directory, not a dish catalog.
- The public detail page in [FoodDetailPage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/food/pages/FoodDetailPage.tsx) currently loads stall detail from backend but fabricates menu/gallery content in frontend via [foodStoreMedia.ts](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/food/data/foodStoreMedia.ts).

## Recommended Approach

Keep the existing FoodCourt module, reinterpret its current top-level record as a managed food stall, and add a child menu-item model plus a manager-owned CRUD flow similar to shop manager.

This is the recommended approach because it:

- preserves the public `/api/food` and `/food/{slug}` structure already in use
- avoids introducing a second overlapping food-court domain model
- matches the current UI semantics, where list/detail pages already behave like stall pages
- lets the rental system remain the source of manager entitlement and quota control

## Alternative Approaches Considered

### Option 1: Recommended

Reuse the current FoodCourt module and evolve `FoodItem` into a stall aggregate with child menu items.

Why this is recommended:

- lowest disruption to existing routes and UI
- smallest conceptual gap between current frontend behavior and persisted data
- aligns with the user's requirement to make food-court management behave like shop manager

### Option 2

Create a brand-new `FoodCourtManager` backend module with separate `FoodCourtStall` entities and leave the current `FoodItem` model untouched.

Why this is weaker:

- duplicates responsibility with the current FoodCourt module
- forces more route rewiring and migration work
- increases the risk of stale or parallel food data

### Option 3

Keep the current FoodCourt entity as a dish and only add manager CRUD for dishes.

Why this is rejected:

- does not match the existing public UI, which presents stores/stalls rather than individual dishes
- cannot naturally support the requirement `1 rental slot = 1 food stall`
- makes the manager experience inconsistent with shop manager ownership rules

## Detailed Design

### 1. Rental Business Type

Extend the rental area domain so each tenant registration stores a business type:

- `Shop`
- `FoodCourt`

Required changes:

- add a business-type field to the rental area entity and persistence model
- include the field in admin rental registration DTOs and detail/list responses
- require admin selection of business type during tenant registration in `RentalAreasAdmin`
- show the selected business type in rental detail views

Behavior rules:

- `Shop` rentals continue to use the existing shop-manager flow
- `FoodCourt` rentals unlock the new food-court manager flow
- the business type is part of the rental record, not derived from slot naming or floor metadata

### 2. Manager Entry Routing

Managers should not see separate static buttons for shop management and food-court management.

Instead, the dashboard should expose one business-management entry point that:

- inspects the manager's rental/business status
- routes to `ManagerShops` when the applicable rental is `Shop`
- routes to `FoodCourtManager` when the applicable rental is `FoodCourt`

This keeps the manager UX simple and avoids showing tools that do not match the tenant type.

If a manager has no qualifying rental setup yet, the entry point should preserve the current "register rental area first" guidance pattern already used in manager flows.

### 3. Food Stall Aggregate

The current FoodCourt top-level record should become the managed food stall profile.

The evolved stall model should contain:

- `Id`
- `OwnerShopId`
- `Name`
- `Slug`
- `Description`
- `ImageUrl`
- `CategorySlug`
- `Location`
- `OpenHours`
- `Phone`
- `Promo`
- `IsActive`

Notes:

- `OwnerShopId` should use the same manager identity source already used by shop-manager flows so ownership checks stay consistent with authentication claims.
- `Location` and `OpenHours` are needed because the detail page currently displays location- and hours-oriented content, even when some of that is generated from presets.
- `CategorySlug` remains useful for theme/preset selection in the public UI.

### 4. Food Menu Item Model

Add a child entity for menu items under each food stall.

Each menu item should support the fields required by the current food detail UI:

- `Id`
- `FoodStallId`
- `Name`
- `Price`
- `Note`
- `Tag`
- `ImageUrl`
- `Ingredients`
- `IsAvailable`
- `DisplayOrder`

Recommended representation for ingredients:

- start with a serialized list field or a simple child table, whichever best matches current repository conventions
- expose it in DTOs as `string[]`

This keeps the backend aligned with the UI elements currently rendered in the menu modal and featured menu cards.

### 5. Ownership and Quota Rules

`FoodCourtManager` must follow the same ownership principles as `ManagerShops`.

Rules:

- a manager can only create or edit food stalls tied to their own rental identity
- a rented `FoodCourt` area can create at most one stall
- one manager cannot modify another manager's stall or menu items
- deleting a stall should remove or cascade its menu items

Creation-status checks should mirror the shop flow, but count only rentals with `BusinessType = FoodCourt`.

### 6. Backend API Shape

#### Public APIs

Keep the public controller surface in the food module, but return richer detail data:

- `GET /api/food`
  - returns stall directory items for the public list page
- `GET /api/food/slug/{slug}`
  - returns full stall detail including menu items

The public detail response should contain enough data to render:

- hero section
- detail/about content
- quick facts sourced from backend where available
- menu grid and menu modal from real menu items
- gallery fallback behavior when menu-item or stall imagery is limited

#### Manager APIs

Add manager-owned endpoints under the food controller or a food-manager controller:

- `GET /api/food/manager`
- `GET /api/food/manager/creation-status`
- `POST /api/food/manager`
- `PUT /api/food/manager/{id}`
- `DELETE /api/food/manager/{id}`

Add menu-item CRUD beneath the owned stall:

- `POST /api/food/manager/{stallId}/menu-items`
- `PUT /api/food/manager/{stallId}/menu-items/{menuItemId}`
- `DELETE /api/food/manager/{stallId}/menu-items/{menuItemId}`

These endpoints should use manager identity from the authenticated claims and apply ownership checks before mutation.

### 7. Manager UI

Create a dedicated `FoodCourtManager` page that follows the broad pattern of `ManagerShops`:

- creation/edit form on one side
- existing owned stall summary on the other side
- creation-status messaging tied to rented food-court slots

The form should have two main sections:

#### Stall Profile

- stall name
- slug
- description
- image
- category
- location
- open hours
- phone
- promo

#### Menu Management

- list current menu items
- add menu item
- edit menu item
- delete menu item
- upload menu item image
- manage ingredients as a comma-separated or tokenized list

The page should feel like a food-court-specific version of shop manager, not a clone of the shop product form.

### 8. Public Food UI Integration

The public list page can stay structurally similar because it already renders a stall directory.

The public detail page should change as follows:

- keep the current layout and visual rhythm
- stop generating primary menu content purely from frontend fallbacks
- consume backend `menuItems`
- continue to use frontend presets only for presentation values that are intentionally not persisted yet, such as gradient themes

Fallback behavior is acceptable for missing optional assets, but menu cards and menu modal should prefer backend menu items as the source of truth.

### 9. Admin Scope

Do not add a new `Food Court Manager` card to [AdminManagement.jsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/auth/pages/AdminManagement.jsx).

Admin responsibility remains:

- manage rental slots
- register tenant to slot
- choose `businessType`

Manager responsibility becomes:

- maintain either shop data or food-court stall data based on the assigned rental type

This preserves the existing admin-management boundary used by the shop workflow.

## Data Flow

1. Admin opens rental area management.
2. Admin registers a tenant for a slot and selects `BusinessType = FoodCourt`.
3. Manager signs in and opens the single business-management entry point from the dashboard.
4. Frontend resolves the manager's eligible business type from backend status.
5. Frontend routes the manager to `FoodCourtManager`.
6. Manager creates the stall profile for the rented food-court slot if it does not exist yet.
7. Manager creates and maintains menu items for that stall.
8. Public food pages load stall profile and menu items from backend.

## Error Handling

### Rental and Routing

- reject tenant registration if business type is missing
- reject manager create/update requests when the manager has no qualifying `FoodCourt` rental
- reject create requests when the manager already used the allowed quota for food-court rentals

### Ownership

- return not-found or forbidden-style behavior when a manager tries to access a stall outside their ownership scope
- apply the same ownership checks to menu-item mutations

### Validation

- stall name and slug are required
- category is required if the public UI depends on it for rendering presets
- menu-item name, price, and image should be validated according to the current UI needs
- ingredient input should be normalized into a clean list

## Testing

### Backend

Verification should cover:

- rental area registration persists `BusinessType`
- rental queries return `BusinessType`
- food-court creation-status counts only food-court rentals
- manager cannot create more than one stall per food-court rental slot
- manager ownership is enforced for stall CRUD
- manager ownership is enforced for menu-item CRUD
- public food detail returns backend menu items

### Frontend

Verification should cover:

- admin rental registration form requires and submits `businessType`
- manager dashboard business-management entry routes to the correct screen
- food-court managers see the correct quota and rental guidance states
- food-court manager can create, edit, and delete menu items
- public food detail renders backend menu data instead of only generated fallback menu data

## Scope

In scope:

- rental-area `BusinessType` support
- manager dynamic business routing
- food-court manager page
- stall ownership and quota logic
- menu-item persistence and CRUD
- backend-driven public food detail menu

Out of scope:

- multi-stall food-court rentals
- admin direct CRUD page for food-court stalls
- redesigning `AdminManagement`
- broad public food-court visual redesign
