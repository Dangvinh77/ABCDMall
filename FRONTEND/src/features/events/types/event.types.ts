export interface EventDto {
  id: string;
  title: string;
  description: string;
  imageUrl: string;
  startDateTime: string;
  endDateTime: string;
  locationType: string;
  shopId?: string | null;
  createdByName: string;
  approvalStatus: string;
  rejectionReason?: string | null;
  hasGiftRegistration: boolean;
  giftDescription?: string | null;
  isOngoing: boolean;
  isUpcoming: boolean;
  createdAt: string;
  approvedAt?: string | null;
}

export interface CreateEventRequest {
  title: string;
  description?: string;
  imageUrl?: string;
  startDateTime: string;
  endDateTime: string;
  locationType: number;
  shopId?: string | null;
  hasGiftRegistration: boolean;
  giftDescription?: string | null;
}

export interface RegisterEventRequest {
  customerName: string;
  customerEmail: string;
  customerPhone: string;
}

export interface EventRegistrationResult {
  registrationId: string;
  redeemCode: string;
  registeredAt: string;
}