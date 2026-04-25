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
  hasGiftRegistration: boolean;
  giftDescription?: string | null;
  isOngoing: boolean;
  isUpcoming: boolean;
  createdAt: string;
}

export interface RegisterEventRequest {
  customerName: string;
  customerEmail: string;
  customerPhone: string;
}