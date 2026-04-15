export interface Brand {
  id: string;
  name: string;
  slug: string;
  category: string;
  floor: string;
  logoUrl: string;
}

export const mockBrandsData: Brand[] = [
  // Lấy một số mẫu từ MapDataSeeder của bạn
  { id: "1", name: "Uniqlo", slug: "uniqlo", category: "thoi-trang", floor: "Tầng 1", logoUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/UNIQLO_logo.svg/1024px-UNIQLO_logo.svg.png" },
  { id: "2", name: "Adidas", slug: "adidas", category: "thoi-trang", floor: "Tầng 1", logoUrl: "https://upload.wikimedia.org/wikipedia/commons/2/20/Adidas_Logo.svg" },
  { id: "3", name: "PNJ", slug: "pnj", category: "phu-kien", floor: "Tầng 1", logoUrl: "https://i.imgur.com/rW4wNpw.png" }, // Ảnh placeholder
  { id: "4", name: "Starbucks Coffee", slug: "starbucks", category: "am-thuc", floor: "Tầng 1", logoUrl: "https://upload.wikimedia.org/wikipedia/en/thumb/d/d3/Starbucks_Corporation_Logo_2011.svg/1200px-Starbucks_Corporation_Logo_2011.svg.png" },
  { id: "5", name: "Phương Nam Book City", slug: "phuong-nam", category: "giao-duc", floor: "Tầng 2", logoUrl: "https://i.imgur.com/H1z4x9N.png" },
  { id: "6", name: "Gogi House", slug: "gogi-house", category: "am-thuc", floor: "Tầng 3", logoUrl: "https://i.imgur.com/z4bY4jH.png" },
  { id: "7", name: "ABCD Cinemas", slug: "abcd-cinemas", category: "giai-tri", floor: "Tầng 4", logoUrl: "https://i.imgur.com/X4yXW1b.png" },
];