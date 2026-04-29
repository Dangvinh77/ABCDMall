// const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5184/api";
// const API_ORIGIN = (() => {
//   try {
//     return new URL(API_BASE_URL).origin;
//   } catch {
//     return "http://localhost:5184";
//   }
// })();

// export const getImageUrl = (url: string) => {
//   if (!url) return "";

//   if (url.startsWith("http")) {
//     return url;
//   }

//   return `${API_ORIGIN}${url}`;
// };

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5184/api";
const API_ORIGIN = (() => {
  try {
    return new URL(API_BASE_URL).origin;
  } catch {
    return "http://localhost:5184";
  }
})();

// chỉ giữ các prefix thật sự là FE local
const FRONTEND_LOCAL_PREFIXES = ["/assets/foodcourt/", "/images/local/"];

export const getImageUrl = (url: string) => {
  if (!url) return "";

  if (url.startsWith("http") || url.startsWith("data:") || url.startsWith("blob:")) {
    return url;
  }

  // các path upload từ BE như /images/foodcourt/... phải đi qua BE origin
  if (FRONTEND_LOCAL_PREFIXES.some((prefix) => url.startsWith(prefix))) {
    return url;
  }

  return `${API_ORIGIN}${url.startsWith("/") ? url : `/${url}`}`;
};
