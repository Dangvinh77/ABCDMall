# Shop Info Shared Stripe Checkout Design

**Goal**

Reuse the existing Stripe checkout API path for `/shop-info` rental bill payments without keeping a separate Stripe session creation flow in the Users module.

**Architecture**

The Stripe integration is split into two concerns:

1. A shared WebAPI payment endpoint that creates Stripe Checkout Sessions.
2. Domain-specific services that validate the business object being paid and provide Stripe session request data.

For rental bills, `/shop-info` should stop calling `POST /api/RentalPayments/{billId}/checkout-session` and instead call a shared payment endpoint that accepts a rental bill reference. The backend keeps rental authorization and status transitions in the Users module, while reusing the common payment controller shape and Stripe checkout response contract already used by movies.

**Components**

- `FRONTEND/src/features/auth/pages/ShopInfo.jsx`
  Switch checkout initiation from the rental-specific controller route to the shared payments route.
- `FRONTEND/src/features/auth/pages/ShopInfo.test.jsx`
  Lock the new request shape with a failing test first.
- `BACKEND/ABCDMall.WebAPI/Controllers/PaymentsController.cs`
  Add a shared Stripe checkout action for rental bill references instead of forcing `/shop-info` through the movies booking endpoint.
- Users rental payment service/contracts
  Expose a method that validates manager ownership, unpaid status, and returns the shared checkout response DTO.
- Existing rental webhook path
  Keep current rental Stripe webhook/business processing so paid status updates remain scoped to rental bills.

**Data Flow**

1. Manager opens `/shop-info`.
2. Manager clicks pay on an unpaid rental bill.
3. Frontend posts to the shared payments API with a rental bill reference.
4. Shared controller delegates rental validation/session creation to the Users rental payment service.
5. Service creates Stripe session through the existing rental Stripe client and returns the shared checkout session response.
6. Frontend redirects to `checkoutUrl`.
7. Existing rental Stripe webhook completes bill status updates.

**Error Handling**

- Unauthorized manager-to-bill access remains rejected by the Users rental payment service.
- Missing or already-paid bill returns a non-success result from the service and surfaces through the shared controller.
- Missing Stripe configuration or Stripe API errors continue to surface as payment API failures with problem details.

**Testing**

- Frontend test must assert `/shop-info` posts to the shared payments route instead of the rental-specific route.
- Backend test must assert the shared payments controller/service path can create a checkout session for a rental bill and still persists the Stripe session id.
- Existing movie payment tests should remain unchanged.
