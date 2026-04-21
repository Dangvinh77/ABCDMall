import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

// ========== User Management ==========

export interface UserSummary {
  id: string;
  email: string;
  fullName: string;
  role: string;
  createdAt: string;
}

export interface RegisterManagerRequest {
  email: string;
  password: string;
  fullName: string;
  shopName: string;
  cccd: string;
  mapLocationId?: number;
}

export interface RegisterManagerResponse {
  message: string;
  shopId: string;
}

export interface UpdateUserAccountRequest {
  email?: string;
  fullName?: string;
  // Add other fields as needed
}

/**
 * Get list of all users (admin only)
 */
export const getUsers = async (token: string): Promise<UserSummary[]> => {
  const response = await axios.get<UserSummary[]>(`${API_BASE_URL}/auth/users`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return response.data;
};

/**
 * Create a new manager account (admin only)
 */
export const registerManager = async (
  token: string,
  dto: RegisterManagerRequest
): Promise<RegisterManagerResponse> => {
  const response = await axios.post<RegisterManagerResponse>(
    `${API_BASE_URL}/auth/register`,
    dto,
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  return response.data;
};

/**
 * Update user account details (admin only)
 */
export const updateUserAccount = async (
  token: string,
  userId: string,
  dto: UpdateUserAccountRequest
): Promise<{ message: string; emailSent: boolean }> => {
  const response = await axios.put(
    `${API_BASE_URL}/auth/users/${userId}`,
    dto,
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  return response.data;
};

/**
 * Delete user account (admin only)
 */
export const deleteUserAccount = async (
  token: string,
  userId: string
): Promise<{ message: string; emailSent: boolean }> => {
  const response = await axios.delete(`${API_BASE_URL}/auth/users/${userId}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return response.data;
};

// ========== Map/Slot Management ==========

export interface MapLocationAdmin {
  id: number;
  shopName: string;
  locationSlot: string;
  shopUrl: string;
  x: number;
  y: number;
  storefrontImageUrl: string;
  status: 'Available' | 'Reserved' | 'ComingSoon' | 'Active';
  shopInfoId?: string;
}

export interface FloorPlanAdmin {
  id: number;
  floorLevel: string;
  description: string;
  blueprintImageUrl: string;
  locations: MapLocationAdmin[];
}

export interface AssignSlotRequest {
  shopInfoId: string;
}

/**
 * Get all floor plans for admin (includes available slots)
 */
export const getAdminFloorPlans = async (token: string): Promise<FloorPlanAdmin[]> => {
  const response = await axios.get<FloorPlanAdmin[]>(
    `${API_BASE_URL}/map/floors/admin`,
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  return response.data;
};

/**
 * Get specific floor plan for admin
 */
export const getAdminFloorPlan = async (
  token: string,
  floorLevel: string
): Promise<FloorPlanAdmin> => {
  const response = await axios.get<FloorPlanAdmin>(
    `${API_BASE_URL}/map/floors/admin/${floorLevel}`,
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  return response.data;
};

/**
 * Assign a slot to a manager
 */
export const assignSlot = async (
  token: string,
  locationId: number,
  shopInfoId: string
): Promise<{ message: string }> => {
  const response = await axios.put(
    `${API_BASE_URL}/map/floors/locations/${locationId}/reserve`,
    { shopInfoId },
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  return response.data;
};

/**
 * Release a slot from a manager
 */
export const releaseSlot = async (
  token: string,
  locationId: number
): Promise<{ message: string }> => {
  const response = await axios.put(
    `${API_BASE_URL}/map/floors/locations/${locationId}/release`,
    {},
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  return response.data;
};
