const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5184/api";
const API_ORIGIN = (() => {
  try {
    return new URL(API_BASE_URL).origin;
  } catch {
    return "http://localhost:5184";
  }
})();

export const getImageUrl = (url: string) => {
  if (!url) return "";

  if (url.startsWith("http")) {
    return url;
  }

  return `${API_ORIGIN}${url}`;
};
