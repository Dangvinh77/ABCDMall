import axios, { AxiosError } from "axios";

type ApiError = Error & {
  data?: unknown;
  status?: number;
};

// API FETCH NOTE:
// Vite exposes env vars that start with VITE_. If you create FRONTEND/.env with
// VITE_API_BASE_URL=http://localhost:5184/api, the frontend will call that API.
// The fallback keeps local testing simple when no .env file exists.
export const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5184/api";

export const http = axios.create({
  baseURL: BASE_URL,
});

http.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");

  if (token) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

http.interceptors.response.use(
  (response) => response,
  async (error) => {
    const requestUrl = error.config?.url ?? "";
    const isPublicAuthRequest =
      requestUrl.includes("/Auth/login") ||
      requestUrl.includes("/Auth/forgotpassword/request-otp") ||
      requestUrl.includes("/Auth/forgotpassword/confirm-otp");

    if (error.response?.status === 401 && !isPublicAuthRequest) {
      const refreshToken = localStorage.getItem("refreshToken");

      if (!refreshToken) {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("role");
        window.location.href = "/login";
        return Promise.reject(error);
      }

      try {
        const refreshResponse = await axios.post<{ accessToken: string }>(
          `${BASE_URL}/Auth/refresh`,
          { refreshToken },
        );

        const newAccessToken = refreshResponse.data.accessToken;
        localStorage.setItem("token", newAccessToken);

        error.config.headers = error.config.headers ?? {};
        error.config.headers.Authorization = `Bearer ${newAccessToken}`;
        return axios(error.config);
      } catch {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("role");
        window.location.href = "/login";
      }
    }

    return Promise.reject(error);
  },
);

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

    const mappedError = new Error(message) as ApiError;
    mappedError.data = axiosError.response?.data;
    mappedError.status = axiosError.response?.status;
    throw mappedError;
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

  put: async <TResponse, TRequest = unknown>(
    url: string,
    data?: TRequest,
    config?: { headers?: Record<string, string> },
  ): Promise<TResponse> => {
    try {
      const response = await http.put<TResponse>(url, data, config);
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

export default api;
