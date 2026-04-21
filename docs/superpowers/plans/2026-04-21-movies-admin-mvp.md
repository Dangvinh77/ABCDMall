# Movies Admin MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a working Movies Admin MVP with dedicated `MoviesAdmin` role support, backend admin APIs, and frontend admin pages wired to real data.

**Architecture:** Reuse `Modules.Users` as the single JWT identity provider, extend it with a `MoviesAdmin` role plus dev seed and admin account endpoints, then add isolated admin controllers/services/repositories in `Movies` for dashboard, movie CRUD, showtime CRUD, and booking views. Keep the existing public movie APIs unchanged while replacing the frontend prototype sections with guarded, data-backed admin pages.

**Tech Stack:** ASP.NET Core Web API, EF Core, SQL Server, React, React Router, existing app `api` client, localStorage-based auth state

---

## File Structure

### Users/Auth files

- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RegisterDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/UpdateUserAccountDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/Auth/MoviesAdminSummaryResponseDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/IUserCommandService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/IUserQueryService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/UserCommandService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/UserQueryService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/IUserReadRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Repositories/UserReadRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Seed/FrontendUsersSeed.cs`
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/AuthController.cs`

### Movies backend files

- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminDashboardResponseDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminMovieUpsertDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminMovieListItemDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminShowtimeUpsertDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminShowtimeListItemDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminBookingListItemDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminBookingDetailDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DependencyInjection.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs`
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminController.cs`
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminDashboardController.cs`
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminBookingsController.cs`

### Frontend files

- Create: `FRONTEND/src/features/movies-admin/services/moviesAdminApi.ts`
- Create: `FRONTEND/src/features/movies-admin/components/MoviesAdminRouteGuard.tsx`
- Create: `FRONTEND/src/features/movies-admin/components/MoviesAdminTable.tsx`
- Create: `FRONTEND/src/features/movies-admin/components/MoviesAdminFormCard.tsx`
- Modify: `FRONTEND/src/features/movies-admin/routes/MoviesAdminRoutes.tsx`
- Modify: `FRONTEND/src/features/movies-admin/pages/MoviesAdminShell.tsx`
- Modify: `FRONTEND/src/features/movies-admin/pages/MoviesAdminDashboardPage.tsx`
- Modify: `FRONTEND/src/features/movies-admin/pages/MoviesAdminSectionPage.tsx`
- Optionally create if splitting is cleaner:
  - `FRONTEND/src/features/movies-admin/pages/MoviesAdminMoviesPage.tsx`
  - `FRONTEND/src/features/movies-admin/pages/MoviesAdminShowtimesPage.tsx`
  - `FRONTEND/src/features/movies-admin/pages/MoviesAdminBookingsPage.tsx`

## Task 1: Extend Users auth for MoviesAdmin role and dev seeds

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RegisterDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/UpdateUserAccountDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/UserCommandService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Seed/FrontendUsersSeed.cs`

- [ ] Add optional role support to register/update DTOs with backend-side allow-list validation for `Manager` and `MoviesAdmin`.
- [ ] Update `UserCommandService.RegisterAsync(...)` so it defaults to `Manager` but accepts `MoviesAdmin` when requested by an authenticated `Admin`.
- [ ] Update `UserCommandService.UpdateUserAccountAsync(...)` so role changes are supported for non-system-admin accounts.
- [ ] Add three `MoviesAdmin` accounts to `FrontendUsersSeed.SeedData.Users` for dev bootstrap.
- [ ] Run backend build for Users-auth related projects and fix any compile errors before moving on.

## Task 2: Add MoviesAdmin management endpoints in Users module

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/Auth/MoviesAdminSummaryResponseDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/IUserQueryService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/IUserReadRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/UserQueryService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Repositories/UserReadRepository.cs`
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/AuthController.cs`

- [ ] Add a query path to list users filtered by role `MoviesAdmin`.
- [ ] Expose admin-only endpoints to list/create/update/delete MoviesAdmin accounts using the existing Auth controller pattern.
- [ ] Ensure delete/update behavior does not allow editing or removing the core `Admin` system account.
- [ ] Keep response payloads small and consistent with the current auth controller style.
- [ ] Run backend build for WebAPI and Users projects.

## Task 3: Add Movies admin application contracts and service layer

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/*.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DependencyInjection.cs`

- [ ] Define DTOs for dashboard summary, movie list/upsert, showtime list/upsert, booking list, and booking detail.
- [ ] Add one focused admin service interface and implementation rather than scattering CRUD into unrelated public services.
- [ ] Keep methods aligned to MVP only: dashboard, movies, showtimes, bookings.
- [ ] Register the service in Movies application DI.
- [ ] Build the Movies application project before wiring repositories.

## Task 4: Implement Movies admin repository in infrastructure

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs`
- Read/Reuse: `MoviesCatalogDbContext`, `MoviesBookingDbContext`, `Movie`, `Showtime`, `Bookingg`, existing repository patterns

- [ ] Implement dashboard aggregation queries across catalog and booking contexts.
- [ ] Implement movie list/detail/create/update/delete-or-disable operations using existing movie entity fields.
- [ ] Implement showtime list/create/update/delete-or-cancel operations with collision checks in the same hall/time range.
- [ ] Implement booking list/detail queries that enrich booking rows with booking/payment/customer information already stored.
- [ ] Register the repository in Movies infrastructure DI.
- [ ] Build the Movies infrastructure and WebAPI projects.

## Task 5: Expose secured Movies admin controllers

**Files:**
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminController.cs`
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminDashboardController.cs`
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminBookingsController.cs`

- [ ] Add `[Authorize(Roles = "MoviesAdmin,Admin")]` to all admin controllers.
- [ ] Keep route structure explicit and stable, e.g. `/api/movies/admin/dashboard`, `/api/movies/admin/movies`, `/api/movies/admin/showtimes`, `/api/movies/admin/bookings`.
- [ ] Return standard `BadRequest`, `NotFound`, and `Ok` results in the same style as existing controllers.
- [ ] Verify there is no overlap or route conflict with existing public movie controllers.
- [ ] Build WebAPI after controller registration.

## Task 6: Add frontend auth guard and API client for Movies admin

**Files:**
- Create: `FRONTEND/src/features/movies-admin/services/moviesAdminApi.ts`
- Create: `FRONTEND/src/features/movies-admin/components/MoviesAdminRouteGuard.tsx`
- Modify: `FRONTEND/src/features/movies-admin/routes/MoviesAdminRoutes.tsx`

- [ ] Add a small API wrapper for dashboard, movies, showtimes, and bookings admin endpoints.
- [ ] Add a route guard that checks `token` plus role `MoviesAdmin` or `Admin`.
- [ ] Redirect unauthenticated users to `/login`.
- [ ] Redirect unauthorized users away from `/movies/admin/*`.
- [ ] Verify route loading still works under the current lazy-loaded router.

## Task 7: Replace prototype dashboard with live dashboard data

**Files:**
- Modify: `FRONTEND/src/features/movies-admin/pages/MoviesAdminDashboardPage.tsx`
- Modify: `FRONTEND/src/features/movies-admin/pages/MoviesAdminShell.tsx`

- [ ] Fetch dashboard summary from backend on page load.
- [ ] Replace hardcoded stats and top movie widgets with real values.
- [ ] Show loading, error, and empty states rather than assuming data is present.
- [ ] Update shell header user badge to show signed-in role/name from available local state where reasonable.
- [ ] Run frontend build.

## Task 8: Replace prototype sections for movies, showtimes, bookings

**Files:**
- Modify or split: `FRONTEND/src/features/movies-admin/pages/MoviesAdminSectionPage.tsx`
- Create if needed:
  - `FRONTEND/src/features/movies-admin/pages/MoviesAdminMoviesPage.tsx`
  - `FRONTEND/src/features/movies-admin/pages/MoviesAdminShowtimesPage.tsx`
  - `FRONTEND/src/features/movies-admin/pages/MoviesAdminBookingsPage.tsx`
- Create shared helpers/components if needed:
  - `FRONTEND/src/features/movies-admin/components/MoviesAdminTable.tsx`
  - `FRONTEND/src/features/movies-admin/components/MoviesAdminFormCard.tsx`

- [ ] Wire the Movies page to list/create/update/delete movies.
- [ ] Wire the Showtimes page to list/create/update/delete showtimes.
- [ ] Wire the Bookings page to list bookings and open booking detail.
- [ ] Leave non-MVP sections as clearly marked placeholder sections.
- [ ] Run frontend build again after the page split/wiring.

## Task 9: Verify end-to-end integration

**Files:**
- No new files required

- [ ] Run `dotnet build BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj`
- [ ] Run `dotnet build BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj`
- [ ] Run `npm run build` from `FRONTEND`
- [ ] Manually validate login with one seeded `MoviesAdmin` account.
- [ ] Manually validate dashboard, movie CRUD, showtime CRUD, and booking list pages.
- [ ] Document any remaining schema or seed limitations before closing the task.
