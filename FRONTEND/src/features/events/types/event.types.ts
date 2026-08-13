export type EventTypeId = 1 | 2; 
export type EventStatusId = 1 | 2 | 3; 


export interface EventDto {
  id: string;
  title: string;
  description: string;
  coverImageUrl: string;
  startDate: string;
  endDate: string;
  location: string;
  
  eventType: string;    
  eventTypeId: number;  
  
  shopId?: string;     
  shopName?: string;
  
  isHot: boolean;
  
  status: string;       
  statusId: number;      
  
  createdAt: string;
}