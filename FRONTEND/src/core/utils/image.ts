export const getImageUrl = (url: string) => {
  if (!url) return "";

  if (url.startsWith("http")) {
    return url;
  }

  return `http://localhost:5184${url}`;
};