# Stripe And PayPal Integration Guide

## Muc tieu

Tai lieu nay mo ta:

- He thong `movies` hien dang co gi ve payment.
- Chua co gi doi voi Stripe va PayPal.
- Can lam gi trong backend va frontend de tich hop that.
- Can thiet lap gi tren trang quan tri Stripe va PayPal.
- Cach test end-to-end.

## Trang thai hien tai trong repo

### Da co

- Enum provider da ho tro `Stripe` va `PayPal`:
  - `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain/Enums/PaymentProvider.cs`
- Bang `Payments` da co cac cot huu ich cho provider:
  - `PaymentIntentId`
  - `ProviderTransactionId`
  - `CallbackPayloadJson`
  - `FailureReason`
  - `Status`
- Backend da co endpoint noi bo de nhan ket qua thanh toan:
  - `POST /api/bookings/{bookingId}/payment-result`
  - file: `BACKEND/ABCDMall.WebAPI/Controllers/BookingsController.cs`
- Sau khi payment success, backend da:
  - confirm booking
  - issue ticket
  - queue email ticket

### Chua co

- Chua thay Stripe SDK integration that.
- Chua thay PayPal SDK/API integration that.
- Chua thay webhook endpoint cho Stripe.
- Chua thay webhook endpoint cho PayPal.
- Chua thay logic tao Stripe Checkout Session hay PaymentIntent.
- Chua thay logic tao PayPal Order va capture.
- Frontend dang dung mock flow trong:
  - `FRONTEND/src/features/movies/pages/CheckOutPage.tsx`
- Frontend hien chua co `paypal` trong `PaymentMethod`:
  - `FRONTEND/src/features/movies/data/booking.ts`

## Ket luan ngan

Hien tai repo moi co:

- domain model cho provider
- bang payment
- flow "nhan payment result"

Nhung chua co:

- provider integration
- webhook verification
- redirect/callback flow
- live payment confirmation tu Stripe hoac PayPal

Noi cach khac: he thong chua tich hop Stripe va PayPal that.

## Phuong an de xuat

Nen tich hop theo huong:

1. Backend la noi tao payment session/order.
2. Frontend chi redirect hoac render provider UI.
3. Webhook tu provider la nguon su that de xac nhan thanh toan thanh cong.
4. Sau webhook thanh cong, backend goi lai flow hien co de:
   - confirm booking
   - book seat
   - issue ticket
   - send email

Khong nen de frontend tu goi `payment-result` voi trang thai `Succeeded` trong production.

## Kien truc de xuat

### Backend

Tao abstraction chung:

- `IPaymentGateway`
- `StripePaymentGateway`
- `PayPalPaymentGateway`

De xuat interface:

- `CreateCheckoutAsync`
- `CaptureAsync` neu provider can capture server-side
- `HandleWebhookAsync`
- `VerifyWebhookAsync`

### API moi de xuat

- `POST /api/payments/checkout-session`
  - input:
    - `bookingId`
    - `provider`
    - `returnUrl`
    - `cancelUrl`
  - output:
    - `redirectUrl`
    - hoac `clientSecret`
    - hoac `providerOrderId`

- `POST /api/payments/webhooks/stripe`
- `POST /api/payments/webhooks/paypal`

- `GET /api/payments/{paymentId}`
  - da co

### Frontend

Checkout page can:

- goi backend tao checkout session/order
- neu la Stripe Checkout:
  - redirect den Stripe hosted checkout
- neu la PayPal:
  - redirect approve URL
  - hoac render PayPal button neu chon JS SDK flow

## Phuong an cu the cho Stripe

### Lua chon de xuat

Chon `Stripe Checkout Session` cho phase 1 vi:

- nhanh hon Payment Element
- it phan UI phai tu lam
- de test
- webhook ro rang

### Luong Stripe de xuat

1. Frontend tao booking pending payment nhu hien tai.
2. Frontend goi `POST /api/payments/checkout-session` voi `provider = Stripe`.
3. Backend tao `Stripe Checkout Session`.
4. Backend luu:
   - `Payment.Provider = Stripe`
   - `Payment.Status = Pending`
   - `Payment.PaymentIntentId` hoac session id
   - `Payment.Amount`
   - `Payment.Currency`
5. Backend tra `redirectUrl`.
6. Frontend redirect sang Stripe Checkout.
7. Sau khi user thanh toan, Stripe goi webhook.
8. Backend verify webhook signature.
9. Neu event hop le va thanh toan thanh cong:
   - map sang `ApplyPaymentResultAsync(...)`
   - confirm booking
   - issue ticket
   - send email

### Event Stripe nen nghe

Toi thieu:

- `checkout.session.completed`
- `payment_intent.succeeded`
- `payment_intent.payment_failed`

Neu chi dung Checkout Session, co the bat dau voi:

- `checkout.session.completed`

Nhung van nen luu y `payment_intent.*` de debug.

### Config can them trong backend

De xuat section:

```json
"StripeSettings": {
  "SecretKey": "",
  "PublishableKey": "",
  "WebhookSecret": "",
  "SuccessUrl": "http://localhost:5173/movies/payment/success",
  "CancelUrl": "http://localhost:5173/movies/payment/cancel",
  "Currency": "usd"
}
```

Ghi chu:

- Stripe card thanh toan phu hop nhat khi dung `USD` hoac currency Stripe account da ho tro.
- Neu business flow cua nhom dang tinh `VND`, can xac nhan Stripe account cua ban co support currency/settlement tuong ung khong.

### Can lam trong code

- Tao `StripeSettings`.
- Add Stripe SDK package.
- Tao `StripePaymentGateway`.
- Tao endpoint create checkout session.
- Tao webhook endpoint.
- Verify chu ky webhook bang `WebhookSecret`.
- Mapping event -> `ApplyPaymentResultAsync`.
- Luu `sessionId`, `paymentIntentId`, `chargeId` vao payment record.

## Phuong an cu the cho PayPal

### Lua chon de xuat

Chon `PayPal Orders API` cho phase 1.

### Luong PayPal de xuat

1. Frontend tao booking pending payment.
2. Frontend goi `POST /api/payments/checkout-session` voi `provider = PayPal`.
3. Backend tao `PayPal Order`.
4. Backend luu:
   - `Payment.Provider = PayPal`
   - `Payment.Status = Pending`
   - `Payment.PaymentIntentId = PayPal order id`
5. Backend tra:
   - `approveUrl`
   - hoac order id de FE dung PayPal JS SDK
6. User approve thanh toan.
7. Backend capture order.
8. PayPal webhook gui event.
9. Backend verify webhook.
10. Neu capture completed:
   - map sang `ApplyPaymentResultAsync(...)`
   - confirm booking
   - issue ticket
   - send email

### Event PayPal nen nghe

Toi thieu:

- `CHECKOUT.ORDER.APPROVED`
- `PAYMENT.CAPTURE.COMPLETED`
- `PAYMENT.CAPTURE.DENIED`
- `PAYMENT.CAPTURE.PENDING`
- `CHECKOUT.PAYMENT-APPROVAL.REVERSED`

### Config can them trong backend

De xuat section:

```json
"PayPalSettings": {
  "ClientId": "",
  "ClientSecret": "",
  "BaseUrl": "https://api-m.sandbox.paypal.com",
  "WebhookId": "",
  "ReturnUrl": "http://localhost:5173/movies/payment/success",
  "CancelUrl": "http://localhost:5173/movies/payment/cancel",
  "Currency": "USD"
}
```

Ghi chu:

- Sandbox dung `https://api-m.sandbox.paypal.com`
- Live dung `https://api-m.paypal.com`

### Can lam trong code

- Tao `PayPalSettings`.
- Tao service lay OAuth token.
- Tao `PayPalPaymentGateway`.
- Tao API create order.
- Tao API capture order neu can.
- Tao webhook endpoint.
- Verify webhook signature.
- Mapping event capture completed -> `ApplyPaymentResultAsync`.
- Luu `orderId`, `captureId`, raw payload.

## Can thiet lap gi tren web Stripe

Nguon tham khao chinh:

- Stripe Developers Dashboard:
  - https://docs.stripe.com/development/dashboard
- Stripe Webhooks:
  - https://docs.stripe.com/webhooks
- Stripe Checkout fulfillment:
  - https://docs.stripe.com/checkout/fulfillment

### Can thiet lap

1. Tao hoac dang nhap Stripe account.
2. Bat `sandbox/test mode`.
3. Lay:
   - `Publishable key`
   - `Secret key`
4. Tao webhook endpoint trong Dashboard.
5. Dang ky URL webhook:
   - local: dung Stripe CLI forward
   - public: vd `https://your-domain/api/payments/webhooks/stripe`
6. Lay `webhook signing secret` (`whsec_...`).
7. Chon event can subscribe:
   - `checkout.session.completed`
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
8. Cau hinh `success_url` va `cancel_url`.
9. Test bang card test cua Stripe:
   - `4242 4242 4242 4242`
   - ngay het han bat ky trong tuong lai
   - CVC bat ky 3 chu so

### Thiet lap local voi Stripe CLI

1. Cai Stripe CLI.
2. Chay:

```bash
stripe listen --forward-to localhost:5000/api/payments/webhooks/stripe
```

3. Lay `whsec_...` CLI in ra.
4. Nap vao `StripeSettings:WebhookSecret`.

## Can thiet lap gi tren web PayPal

Nguon tham khao chinh:

- PayPal Orders API v2:
  - https://developer.paypal.com/docs/api/orders/sdk/v2/
- PayPal Webhooks overview:
  - https://developer.paypal.com/api/rest/webhooks/
- PayPal Webhooks Management:
  - https://developer.paypal.com/docs/api/webhooks/v1/
- PayPal checkout webhooks:
  - https://developer.paypal.com/docs/checkout/apm/reference/subscribe-to-webhooks/

### Can thiet lap

1. Tao hoac dang nhap PayPal Developer account.
2. Tao app trong Dashboard.
3. Lay:
   - `Client ID`
   - `Client Secret`
4. Su dung sandbox truoc.
5. Tao webhook cho app.
6. Dang ky URL webhook:
   - vd `https://your-domain/api/payments/webhooks/paypal`
7. Dang ky event:
   - `CHECKOUT.ORDER.APPROVED`
   - `PAYMENT.CAPTURE.PENDING`
   - `PAYMENT.CAPTURE.COMPLETED`
   - `PAYMENT.CAPTURE.DENIED`
   - `CHECKOUT.PAYMENT-APPROVAL.REVERSED`
8. Lay `Webhook ID`.
9. Nap `Webhook ID` vao `PayPalSettings`.
10. Tao sandbox buyer account de test flow approve/capture.

## De xuat config appsettings

Khong commit secret that vao repo.

Nen luu o:

- `appsettings.Development.json` local only
- Secret Manager
- environment variables
- CI/CD secret store

### Mau config

```json
{
  "StripeSettings": {
    "SecretKey": "",
    "PublishableKey": "",
    "WebhookSecret": "",
    "SuccessUrl": "http://localhost:5173/movies/payment/success",
    "CancelUrl": "http://localhost:5173/movies/payment/cancel",
    "Currency": "USD"
  },
  "PayPalSettings": {
    "ClientId": "",
    "ClientSecret": "",
    "BaseUrl": "https://api-m.sandbox.paypal.com",
    "WebhookId": "",
    "ReturnUrl": "http://localhost:5173/movies/payment/success",
    "CancelUrl": "http://localhost:5173/movies/payment/cancel",
    "Currency": "USD"
  }
}
```

## De xuat roadmap thuc hien

### Phase 1

- Bo mock `applyPaymentResult` tren FE.
- Tich hop Stripe Checkout Session that.
- Tich hop Stripe webhook.
- Xac minh ticket email van hoat dong.

### Phase 2

- Them PayPal vao `PaymentMethod`.
- Tich hop PayPal Orders API.
- Tich hop PayPal webhook.

### Phase 3

- Them trang success/cancel ro rang.
- Them admin logs cho payment callback/webhook.
- Them retry va idempotency key.

## Cac thay doi cu the can lam trong repo nay

### Backend

- Tao `StripeSettings` va `PayPalSettings`
- Register them trong DI
- Tao thu muc:
  - `Application/Services/Payments` hoac
  - `Infrastructure/Services/Payments`
- Them:
  - `IPaymentGateway`
  - `StripePaymentGateway`
  - `PayPalPaymentGateway`
- Them controller endpoint:
  - `POST /api/payments/checkout-session`
  - `POST /api/payments/webhooks/stripe`
  - `POST /api/payments/webhooks/paypal`
- Them logic map webhook -> `ApplyPaymentResultAsync`

### Frontend

- Them `paypal` vao `PaymentMethod`
- Doi `visa` thanh `stripe` cho ro nghia
- Checkout page:
  - goi backend create checkout session/order
  - redirect provider
- Tao page:
  - `/movies/payment/success`
  - `/movies/payment/cancel`

## Luu y quan trong

- Webhook moi la nguon su that de xac nhan payment.
- Frontend redirect success khong du de danh dau booking da thanh toan.
- Can verify chu ky webhook ca Stripe va PayPal.
- Can idempotency:
  - webhook co the gui lai
  - callback co the bi goi nhieu lan
- Can log raw payload de debug.
- Can test sandbox truoc live.

## De xuat uu tien

Nen lam theo thu tu:

1. Stripe truoc
2. PayPal sau

Ly do:

- FE hien da co `visa -> Stripe` nen it thay doi hon
- Stripe Checkout Session nhanh de dong production flow
- Sau khi Stripe on dinh, tai su dung abstraction cho PayPal de tranh code cheo

## Ghi chu ve tai lieu tham khao

Thong tin dashboard va webhook cua Stripe/PayPal co the thay doi theo thoi gian. Khi implement that, doi chieu them voi docs chinh thuc:

- Stripe Developers Dashboard:
  - https://docs.stripe.com/development/dashboard
- Stripe Webhooks:
  - https://docs.stripe.com/webhooks
- Stripe Checkout fulfillment:
  - https://docs.stripe.com/checkout/fulfillment
- PayPal Orders API v2:
  - https://developer.paypal.com/docs/api/orders/sdk/v2/
- PayPal Webhooks overview:
  - https://developer.paypal.com/api/rest/webhooks/
- PayPal Webhooks Management:
  - https://developer.paypal.com/docs/api/webhooks/v1/
- PayPal checkout webhooks:
  - https://developer.paypal.com/docs/checkout/apm/reference/subscribe-to-webhooks/
