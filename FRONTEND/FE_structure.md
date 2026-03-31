# ABCD Mall Client

Frontend structure for the ABCD Mall client, organized around a feature-first approach that maps cleanly to the backend modulith.

## Current frontend stack

- Vite 8
- React 19
- TypeScript 5
- Tailwind CSS 4
- ESLint 9

## Target folder structure

```bash
abcd-mall-client/
├── src/
│   ├── core/                       # Foundation layer: app-wide wiring and shared runtime concerns
│   │   ├── api/                    # Axios/fetch client, interceptors, request config
│   │   ├── hooks/                  # Global hooks such as useAuth, useTheme
│   │   └── layouts/                # MainLayout, AdminLayout, ShopLayout
│   │
│   ├── features/                   # Business modules aligned with backend bounded contexts
│   │   ├── auth/                   # Login, session, role-based access
│   │   ├── movies/                 # Movie catalog, showtimes, booking flow
│   │   ├── shops/                  # Shops, products, manager operations
│   │   └── feedbacks/              # Guest feedback and contact flows
│   │
│   ├── components/                 # Shared presentational UI components
│   ├── pages/                      # Route-level page components
│   ├── routes/                     # Router config, guards, route definitions
│   └── store/                      # Global client state such as Zustand stores
│
├── .env                            # Contains VITE_API_URL
└── vite.config.ts                  # Vite config and path aliases
```

## Module convention

Each feature should encapsulate its own API calls, UI, state, types, and helper logic when needed.

```bash
src/features/movies/
├── api/                            # API calls for movies and booking
├── components/                     # Movie-specific UI pieces
├── hooks/                          # Feature-level hooks
├── types/                          # TypeScript contracts
├── utils/                          # Local helpers
└── index.ts                        # Public exports for the feature
```

Suggested internal pattern:

- `api/`: backend communication for that feature only
- `components/`: reusable UI inside the feature
- `hooks/`: stateful orchestration and async flow
- `types/`: DTOs, view models, request and response types
- `utils/`: pure helpers with no React dependency
- `index.ts`: controlled public entry point

## Layer responsibilities

- `core/` holds app-wide concerns shared by multiple modules.
- `features/` owns business logic and should be the main place for domain-specific code.
- `components/` stays generic and reusable across features.
- `pages/` composes layouts and feature components into route screens.
- `routes/` centralizes public/private route definitions and guards.
- `store/` contains truly global client state, not feature-local state.

## Import guidance

- Prefer `@/` aliases for imports from `src`.
- Keep cross-feature imports limited.
- Import through each feature's `index.ts` when exposing reusable feature APIs.

## Environment

Create a local `.env` file in the frontend root:

```bash
VITE_API_URL=https://localhost:7001/api/v1
```

## Notes

- The folder skeleton above has been scaffolded under `src/`.
- Existing starter files such as `App.tsx` can be migrated gradually into this structure.
