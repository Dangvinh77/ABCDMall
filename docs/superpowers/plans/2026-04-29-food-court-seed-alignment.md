# Food Court Seed Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align the eight seeded `FoodCourt` rentals with realistic food-court manager/shop identities while preserving the current register and rental sync baseline.

**Architecture:** Update only user/shop seed composition in the Users module and prove the behavior with focused seed integration tests. Reuse existing `BusinessType` logic and existing food-court catalog names instead of adding new cross-module persistence.

**Tech Stack:** ASP.NET Core, C#, xUnit, EF Core InMemory

---

### Task 1: Lock the expected food-court seed behavior in tests

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/FrontendSeedIntegrationTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/FrontendSeedIntegrationTests.cs`

- [ ] **Step 1: Write the failing test**

Add assertions that the eight `FoodCourt` rental areas exist and map to expected food-court shop names.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test .\BACKEND\ABCDMall.Modules\Users\ABCDMall.Modules.Users.Tests\ABCDMall.Modules.Users.Tests.csproj --filter FrontendSeedIntegrationTests`
Expected: FAIL because current seed data still uses retail-style names for some food-court indexes.

- [ ] **Step 3: Write minimal implementation**

Update seed profile/shop composition for the eight food-court indexes only.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test .\BACKEND\ABCDMall.Modules\Users\ABCDMall.Modules.Users.Tests\ABCDMall.Modules.Users.Tests.csproj --filter FrontendSeedIntegrationTests`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Seed/FrontendUsersSeed.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/FrontendSeedIntegrationTests.cs docs/superpowers/specs/2026-04-29-food-court-seed-alignment-design.md docs/superpowers/plans/2026-04-29-food-court-seed-alignment.md
git commit -m "feat: align food court seed managers"
```
