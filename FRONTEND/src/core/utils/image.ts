// export const getImageUrl = (url: string) => {
//   if (!url) return "";

//   if (url.startsWith("http")) {
//     return url;
//   }

//   return `http://localhost:5184${url}`;
// };

export const getImageUrl = (url: string) => {
  if (!url) return "/placeholder.png"; // thêm fallback

  if (url.startsWith("http")) {
    return url;
  }

  // 👉 thêm logic fix thiếu "/"
  let fixedUrl = url.startsWith("/") ? url : `/${url}`;

  // 👉 thêm logic nếu thiếu /img (backend của bạn đang dùng wwwroot/img)
  if (!fixedUrl.startsWith("/img")) {
    fixedUrl = `/img${fixedUrl}`;
  }

  return `http://localhost:5184${fixedUrl}`;
};