# Food Court Seed Alignment Design

## Goal

Keep the current register and rental sync baseline, and correct the remaining mismatch so that the eight seeded `FoodCourt` rentals are represented by food-court managers and tenant shop data that match the food-court domain.

## Scope

- Reuse the existing `BusinessType` flow and current count of eight `FoodCourt` rentals.
- Update only seed data and targeted tests.
- Do not redesign the register flow or introduce new persistence fields.

## Approach Options

### Option 1: BusinessType only

Leave the eight `FoodCourt` slots as-is and trust routing/business type checks.

Trade-off: lowest effort, but seeded managers still look like regular retail tenants and `/food-court-manager` is not convincingly testable.

### Option 2: Align manager and shop seed names with food-court tenants

Keep the same slot IDs and `BusinessType` assignments, but replace the affected manager/shop seed identities with food-court tenant names and categories that match the food-court catalog.

Trade-off: focused fix, no schema churn, gives realistic seeded accounts and dashboards immediately.

### Option 3: Full cross-module relational linkage

Add deeper coupling between user/shop seeds and food item seeds.

Trade-off: higher complexity and unclear value if the app currently only needs realistic food-court tenant identities.

## Recommendation

Use Option 2. It fixes the actual mismatch without expanding the scope into new module coupling.

## Design

### Seed behavior

- Keep the existing eight `FoodCourt` rental area indexes.
- For each of those indexes, seed manager profile names, shop names, and shop category/floor text as food-court tenants instead of generic or retail identities.
- Keep non-food-court manager/shop seeds unchanged.

### Data consistency

- `RentalArea.BusinessType` remains the source for business classification.
- `ShopInfo.ShopName`, `ShopInfo.Category`, summaries, and manager identity for the eight food-court indexes must look like food-court tenants.
- The selected food-court tenant names should be drawn from the existing food-court catalog so the app presents one coherent dataset.

### Testing

- Add or extend a backend seed integration test that proves there are exactly eight rented `FoodCourt` rental areas.
- Assert that each seeded `FoodCourt` rental points to a shop with a food-oriented identity rather than the previous retail placeholders.

## Risks

- Existing tests may assume older seed names.
- Seed changes can affect snapshots or UI expectations if there are hidden tests tied to specific manager/shop labels.

## Out of Scope

- New food stall/menu ownership relations
- Register flow changes
- Migration or schema updates
