export interface Brand {
  id: string;
  name: string;
  slug: string;
  category: string;
  floor: string;
  logoUrl: string;
}

export const mockBrandsData: Brand[] = [
   { id: "1", name: "Uniqlo", slug: "uniqlo", category: "thoi-trang", floor: "Tầng 1", logoUrl: "/img/uniqlo/logo.png" },
  { id: "2", name: "Adidas", slug: "adidas", category: "thoi-trang", floor: "Tầng 1", logoUrl: "/img/adidas/logo.png" },
  { id: "3", name: "Levi's", slug: "levis", category: "thoi-trang", floor: "Tầng 1", logoUrl: "/img/levi/logo.png" },
  { id: "4", name: "PNJ", slug: "pnj", category: "phu-kien", floor: "Tầng 1", logoUrl: "/img/pnj/logo.jpg" },
  { id: "5", name: "Starbucks Coffee", slug: "starbucks", category: "am-thuc", floor: "Tầng 1", logoUrl: "/img/starbuck/logo.png" },
  { id: "6", name: "Nike", slug: "nike", category: "thoi-trang", floor: "Tầng 1", logoUrl: "/img/nike/logo.webp" },
  { id: "7", name: "Charles & Keith", slug: "charles-keith", category: "thoi-trang", floor: "Tầng 1", logoUrl: "/img/C&K/logo.jpg" },
  { id: "8", name: "Pedro", slug: "pedro", category: "thoi-trang", floor: "Tầng 1", logoUrl: "/img/pedro/logo.png" },
  { id: "9", name: "Ecco", slug: "ecco", category: "thoi-trang", floor: "Tầng 1", logoUrl: "/img/ecco/logo.png" },

  // ===== TẦNG 2 =====
  { id: "10", name: "Phương Nam Book City", slug: "phuong-nam", category: "giao-duc", floor: "Tầng 2", logoUrl: "/img/phuongnam/logo.jpg" },
  { id: "11", name: "Miniso", slug: "miniso", category: "thoi-trang", floor: "Tầng 2", logoUrl: "/img/miniso/logo.jpg" },
  { id: "12", name: "Boo", slug: "boo", category: "thoi-trang", floor: "Tầng 2", logoUrl: "/img/boo/logo.png" },
  { id: "13", name: "Levents", slug: "levents", category: "thoi-trang", floor: "Tầng 2", logoUrl: "/img/levents/logo.png" },
  { id: "14", name: "Belluni", slug: "belluni", category: "thoi-trang", floor: "Tầng 2", logoUrl: "/img/Belluni/logo.png" },

  // ===== TẦNG 3 (FOOD) =====
  { id: "15", name: "Gogi House", slug: "gogi-house", category: "foods", floor: "Tầng 3", logoUrl: "/img/gogi/logo.png" },
  { id: "16", name: "Kichi Kichi", slug: "kichi-kichi", category: "am-thuc", floor: "Tầng 3", logoUrl: "/img/kichi/logo1.png" },
  { id: "17", name: "Texas Chicken", slug: "texas-chicken", category: "am-thuc", floor: "Tầng 3", logoUrl: "/img/texaschicken/logo.jpg" },
  { id: "18", name: "Sushi X", slug: "sushix", category: "am-thuc", floor: "Tầng 3", logoUrl: "/img/sushix/logo.jpg" },

  // ===== TẦNG 4 =====
  { id: "19", name: "ABCD Cinemas", slug: "abcd-cinemas", category: "giai-tri", floor: "Tầng 4", logoUrl: "/img/ABCDMall/logo.png" },
  { id: "20", name: "Dookki", slug: "dookki", category: "am-thuc", floor: "Tầng 4", logoUrl: "/img/Dookki/logo.png" },
  { id: "21", name: "The Pizza Company", slug: "pizza-company", category: "am-thuc", floor: "Tầng 4", logoUrl: "/img/pizzaCompany/logo.webp" },
  { id: "22", name: "Dairy Queen", slug: "dairy-queen", category: "am-thuc", floor: "Tầng 4", logoUrl: "/img/dairyqueen/logo.jpg" },
];