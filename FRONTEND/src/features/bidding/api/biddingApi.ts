import { api } from "../../../core/api/api";

export type TemplateType = "ShopAd" | "DiscountAd" | "EventAd";

export interface SubmitCarouselBidPayload {
  bidAmount: number;
  templateType: TemplateType;
  shopImageFile?: File | null;
  message?: string;
  productImageFile?: File | null;
  originalPrice?: number;
  discountPrice?: number;
  eventImageFile?: File | null;
  startDate?: string;
  startTime?: string;
}

export interface ManagerCarouselBid {
  id: string;
  shopId: string;
  shopName?: string | null;
  templateType: string;
  bidAmount: number;
  status: string;
  targetMondayDate: string;
  createdAt: string;
  imageUrl?: string | null;
  message?: string | null;
  originalPrice?: number | null;
  discountPrice?: number | null;
  eventStartDate?: string | null;
  startTime?: string | null;
}

export interface AdminCarouselBid {
  id: string;
  shopId: string;
  shopName: string;
  templateType: string;
  bidAmount: number;
  status: string;
  targetMondayDate: string;
  createdAt: string;
}

export interface MovieCarouselAdPayload {
  imageUrl: string;
  description: string;
}

export interface MovieCarouselAd {
  id: string;
  imageUrl: string;
  description: string;
  targetMondayDate: string;
  isActive: boolean;
}

export interface BidPaymentCheckoutSession {
  bidId: string;
  sessionId: string;
  checkoutUrl: string;
  expiresAtUtc: string;
}

export interface BidResolutionSummary {
  targetMondayDate: string;
  totalPending: number;
  wonCount: number;
  lostCount: number;
}

export interface BidPublishSummary {
  targetMondayDate: string;
  activeBidCount: number;
  movieAdIncluded: boolean;
  totalSlots: number;
}

export interface PublicCarouselItem {
  id: string;
  slotType: "ShopAd" | "DiscountAd" | "EventAd" | "MovieAd";
  targetMondayDate: string;
  shopId?: string | null;
  shopName?: string | null;
  shopSlug?: string | null;
  imageUrl: string;
  message?: string | null;
  description?: string | null;
  originalPrice?: number | null;
  discountPrice?: number | null;
  eventStartDate?: string | null;
  startTime?: string | null;
  linkUrl?: string | null;
}

export const biddingApi = {
  getManagerBids: () => api.get<ManagerCarouselBid[]>("/manager/bids"),
  submitBid: (payload: SubmitCarouselBidPayload) => {
    const formData = new FormData();
    formData.append("bidAmount", String(payload.bidAmount));
    formData.append("templateType", payload.templateType);

    if (payload.message) {
      formData.append("message", payload.message);
    }

    if (typeof payload.originalPrice === "number") {
      formData.append("originalPrice", String(payload.originalPrice));
    }

    if (typeof payload.discountPrice === "number") {
      formData.append("discountPrice", String(payload.discountPrice));
    }

    if (payload.startDate) {
      formData.append("startDate", payload.startDate);
    }

    if (payload.startTime) {
      formData.append("startTime", payload.startTime);
    }

    if (payload.shopImageFile) {
      formData.append("shopImageFile", payload.shopImageFile);
    }

    if (payload.productImageFile) {
      formData.append("productImageFile", payload.productImageFile);
    }

    if (payload.eventImageFile) {
      formData.append("eventImageFile", payload.eventImageFile);
    }

    return api.post<ManagerCarouselBid, FormData>("/manager/bids", formData);
  },
  payBid: (bidId: string) =>
    api.post<BidPaymentCheckoutSession, undefined>(`/manager/bids/${bidId}/pay`),

  getAdminBids: () => api.get<AdminCarouselBid[]>("/admin/bids"),
  upsertMovieAd: (payload: MovieCarouselAdPayload) =>
    api.post<MovieCarouselAd, MovieCarouselAdPayload>("/admin/bids/movie-ad", payload),
  resolveUpcomingWeek: () =>
    api.post<BidResolutionSummary, undefined>("/admin/bids/trigger-saturday-resolution"),
  publishUpcomingWeek: () =>
    api.post<BidPublishSummary, undefined>("/admin/bids/trigger-monday-publish"),

  getPublicCarousel: () => api.get<PublicCarouselItem[]>("/public/carousel"),
};
