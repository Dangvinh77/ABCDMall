import axios, { AxiosError } from "axios";

// API FETCH NOTE:
// Vite exposes env vars that start with VITE_. If you create FRONTEND/.env with
// VITE_API_BASE_URL=http://localhost:5184/api, the frontend will call that API.
// The fallback keeps local testing simple when no .env file exists.
export const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5184/api";

export const http = axios.create({
  baseURL: BASE_URL,
});

function mapApiError(error: unknown): never {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<{ detail?: string; title?: string }>;
    // API FETCH NOTE:
    // ASP.NET Core often returns ProblemDetails with "detail" or "title".
    // We convert that backend response into a normal Error for pages to display.
    const message =
      axiosError.response?.data?.detail ??
      axiosError.response?.data?.title ??
      axiosError.message ??
      "Request failed.";

    throw new Error(message);
  }

  throw error;
}

export const api = {
  get: async <T>(url: string, params?: Record<string, unknown>): Promise<T> => {
    try {
      // API FETCH NOTE:
      // All GET requests return response.data only, so pages do not need to know Axios internals.
      const response = await http.get<T>(url, { params });
      return response.data;
    } catch (error) {
      return mapApiError(error);
    }
  },

  post: async <TResponse, TRequest = unknown>(url: string, data?: TRequest): Promise<TResponse> => {
    try {
      // API FETCH NOTE:
      // POST sends JSON by default. moviesApi.quoteBooking uses this for POST /api/bookings/quote.
      const response = await http.post<TResponse>(url, data);
      return response.data;
    } catch (error) {
      return mapApiError(error);
    }
  },

  delete: async <TResponse = void>(url: string): Promise<TResponse> => {
    try {
      const response = await http.delete<TResponse>(url);
      return response.data;
    } catch (error) {
      return mapApiError(error);
    }
  },
};

export const uploadImage = async (file: File) => {
  const formData = new FormData();
  formData.append("file", file);

  const response = await http.post("/food/upload", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return response.data;
};
