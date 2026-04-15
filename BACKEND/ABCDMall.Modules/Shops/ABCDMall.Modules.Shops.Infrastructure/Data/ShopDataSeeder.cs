using ABCDMall.Modules.Shops.Domain.Entities;
using ABCDMall.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Shops.Infrastructure.Data;

public static class ShopDataSeeder
{
    // Bắt buộc truyền AppDbContext (nằm trong Shared)
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<Shop>();

        if (await dbSet.AnyAsync())
        {
            return;
        }

        var shops = new List<Shop>
        {
           // ==========================================
    // TẦNG 1: THỜI TRANG & TRANG SỨC (9 CỬA HÀNG)
    // ==========================================

    // 1. UNIQLO
    new Shop
    {
        Name = "Uniqlo", Slug = "uniqlo", Floor = "Tầng 1", LocationSlot = "1-01",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/9/92/UNIQLO_logo.svg",
        CoverImageUrl = "https://im.uniqlo.com/global-cms/spa/res3ed3924df508b9cdb36ed77daae56891fr.jpg",
        Description = "Thương hiệu thời trang bán lẻ đến từ Nhật Bản, nổi tiếng với triết lý LifeWear.",
        Slogan = "LifeWear - Simple Made Better", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Áo Thun Cổ Tròn Ngắn Tay Uniqlo U", ImageUrl = "https://image.uniqlo.com/UQ/ST3/vn/imagesgoods/455359/item/vngoods_00_455359.jpg", Price = 249000, IsFeatured = true },
            new Product { Name = "Áo Khoác Chống Nắng UV Protection", ImageUrl = "https://image.uniqlo.com/UQ/ST3/vn/imagesgoods/453773/item/vngoods_69_453773.jpg", Price = 799000, IsFeatured = true },
            new Product { Name = "Quần Smart Ankle Pants", ImageUrl = "https://image.uniqlo.com/UQ/ST3/AsianCommon/imagesgoods/450251/item/goods_09_450251.jpg", Price = 799000, IsFeatured = true },
            new Product { Name = "Áo Sơ Mi Vải Linen", ImageUrl = "https://image.uniqlo.com/UQ/ST3/vn/imagesgoods/464936/item/vngoods_00_464936.jpg", Price = 599000, OldPrice = 799000, DiscountPercent = 25, IsDiscounted = true }
        },
        Vouchers = new List<Voucher> { new Voucher { Code = "UQWELCOME", Title = "Giảm 100K", Description = "Hóa đơn từ 1.000.000đ khi tải app", ValidUntil = "31/12/2026" } }
    },

    // 2. ADIDAS
    new Shop
    {
        Name = "Adidas", Slug = "adidas", Floor = "Tầng 1", LocationSlot = "1-11",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/2/20/Adidas_Logo.svg",
        CoverImageUrl = "https://brand.assets.adidas.com/image/upload/f_auto,q_auto,fl_lossy/if_w_gt_1920,w_1920/enUS/Images/running-ss24-supernova-launch-hp-mh-large-d_tcm221-1111666.jpg",
        Description = "Thương hiệu thể thao hàng đầu thế giới với các dòng Originals, Performance.",
        Slogan = "Impossible is Nothing", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Giày Thể Thao Nam Samba OG", ImageUrl = "https://assets.adidas.com/images/h_840,f_auto,q_auto,fl_lossy,c_fill,g_auto/3bbecbdf584e40398446a8bf0117cf62_9366/Giay_Samba_OG_trang_B75806_01_standard.jpg", Price = 2700000, IsFeatured = true },
            new Product { Name = "Giày Chạy Bộ Nam Ultraboost 1.0", ImageUrl = "https://assets.adidas.com/images/h_840,f_auto,q_auto,fl_lossy,c_fill,g_auto/119bc1694f2945d8ab93af6600bf8a96_9366/Giay_Ultraboost_1.0_trang_HQ4199_01_standard.jpg", Price = 5000000, IsFeatured = true },
            new Product { Name = "Giày Stan Smith Trắng", ImageUrl = "https://assets.adidas.com/images/h_840,f_auto,q_auto,fl_lossy,c_fill,g_auto/68ae7ea7849b43eca70aac1e00f5146d_9366/Giay_Stan_Smith_trang_FX5502_01_standard.jpg", Price = 1750000, OldPrice = 2500000, DiscountPercent = 30, IsDiscounted = true }
        },
        Vouchers = new List<Voucher> { new Voucher { Code = "ADICLUB15", Title = "Giảm 15% Hàng Nguyên Giá", Description = "Dành riêng cho adiClub Members", ValidUntil = "30/06/2026" } }
    },

    // 3. NIKE
    new Shop
    {
        Name = "Nike", Slug = "nike", Floor = "Tầng 1", LocationSlot = "1-07",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a6/Logo_NIKE.svg",
        CoverImageUrl = "https://c.static-nike.com/a/images/f_auto/dpr_1.0,cs_srgb/w_1824,c_limit/d2757270-ec18-4cc3-9b25-b88a91a9c3fb/nike-just-do-it.jpg",
        Description = "Thương hiệu giày và thời trang thể thao lớn nhất thế giới, truyền cảm hứng cho mọi vận động viên.",
        Slogan = "Just Do It", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Nike Air Force 1 '07", ImageUrl = "https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/b7d9211c-26e7-431a-ac24-b0540fb3c00f/air-force-1-07-mens-shoes-jBrhbr.png", Price = 2939000, IsFeatured = true },
            new Product { Name = "Nike Air Zoom Pegasus 40", ImageUrl = "https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/8cbafcf5-3bf5-40b9-87a1-d87b3b4bc50d/pegasus-40-mens-road-running-shoes-MCVWzK.png", Price = 3519000, IsFeatured = true },
            new Product { Name = "Áo Thun Thể Thao Dri-FIT", ImageUrl = "https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/e970a2f1-6a3f-42e1-884c-cb6104e4604e/dri-fit-mens-training-t-shirt-1JvTzR.png", Price = 650000, OldPrice = 850000, DiscountPercent = 23, IsDiscounted = true }
        }
    },

    // 4. LEVI'S
    new Shop
    {
        Name = "Levi's", Slug = "levis", Floor = "Tầng 1", LocationSlot = "1-02",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/1/1b/Levi%27s_logo.svg",
        CoverImageUrl = "https://www.levi.com.vn/wp-content/uploads/2023/09/Levis-VN-Banner-Desktop.jpg",
        Description = "Biểu tượng của thời trang Denim toàn cầu. Những chiếc quần jeans Levi's là minh chứng cho sự tự do và cá tính.",
        Slogan = "Quality Never Goes Out of Style", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Quần Jeans Nam 501® Original", ImageUrl = "https://lsco.scene7.com/is/image/lsco/005010115-front-pdp?fmt=jpeg&qlt=70,1&op_sharpen=0&resMode=sharp2&op_usm=0.8,1,10,0&fit=crop,0&wid=450&hei=600", Price = 2299000, IsFeatured = true },
            new Product { Name = "Áo Khoác Denim Trucker Jacket", ImageUrl = "https://lsco.scene7.com/is/image/lsco/723340146-front-pdp?fmt=jpeg&qlt=70,1&op_sharpen=0&resMode=sharp2&op_usm=0.8,1,10,0&fit=crop,0&wid=450&hei=600", Price = 2599000, IsFeatured = true },
            new Product { Name = "Áo Thun Levi's® Logo Tee", ImageUrl = "https://lsco.scene7.com/is/image/lsco/224910815-front-pdp?fmt=jpeg&qlt=70,1&op_sharpen=0&resMode=sharp2&op_usm=0.8,1,10,0&fit=crop,0&wid=450&hei=600", Price = 499000, OldPrice = 799000, DiscountPercent = 37, IsDiscounted = true }
        }
    },

    // 5. CHARLES & KEITH
    new Shop
    {
        Name = "Charles & Keith", Slug = "charles-keith", Floor = "Tầng 1", LocationSlot = "1-06",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/e/ee/Charles_and_Keith_Logo.svg",
        CoverImageUrl = "https://www.charleskeith.vn/on/demandware.static/-/Library-Sites-CharlesKeithVN/default/dw18b84e0c/images/campaign/2024/spring/spring-2024-campaign-desktop.jpg",
        Description = "Thương hiệu túi xách, giày dép và phụ kiện cao cấp với thiết kế hiện đại, tinh tế dành riêng cho phái đẹp.",
        Slogan = "Fashion forward footwear and accessories", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Túi Xách Tay Gabine Saddle", ImageUrl = "https://www.charleskeith.vn/dw/image/v2/BCWJ_PRD/on/demandware.static/-/Sites-vn-products/default/dw5b078020/images/hi-res/2021-L3-CK2-80781610-1-01-1.jpg?sw=1152&sh=1536", Price = 2150000, IsFeatured = true },
            new Product { Name = "Giày Cao Gót Mũi Nhọn Classic", ImageUrl = "https://www.charleskeith.vn/dw/image/v2/BCWJ_PRD/on/demandware.static/-/Sites-vn-products/default/dw47b2c5dc/images/hi-res/2023-L6-CK1-60280385-01-1.jpg?sw=1152&sh=1536", Price = 1390000, IsFeatured = true },
            new Product { Name = "Ví Ngắn Gập Đôi Đính Nơ", ImageUrl = "https://www.charleskeith.vn/dw/image/v2/BCWJ_PRD/on/demandware.static/-/Sites-vn-products/default/dw9026402f/images/hi-res/2023-L4-CK6-10770578-01-1.jpg?sw=1152&sh=1536", Price = 950000, OldPrice = 1250000, DiscountPercent = 24, IsDiscounted = true }
        }
    },

    // 6. BEAUTY BOX
    new Shop
    {
        Name = "Beauty Box", Slug = "beauty-box", Floor = "Tầng 1", LocationSlot = "SB-01",
        LogoUrl = "https://beautybox.com.vn/web/logo/logo-beautybox.png", // Dùng tạm text logo hoặc icon phù hợp nếu lỗi link
        CoverImageUrl = "https://beautybox.com.vn/media/wysiwyg/BB-KV-Web-Banner-1920x640_1.jpg",
        Description = "Thiên đường mỹ phẩm chính hãng với hàng ngàn sản phẩm làm đẹp từ các thương hiệu đình đám Hàn Quốc, Âu Mỹ.",
        Slogan = "All Beauty Trends", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Phấn Nước CLIO Kill Cover The New Founwear", ImageUrl = "https://product.hstatic.net/200000000133/product/clio_kill_cover_the_new_founwear_cushion_03_linen_1_fd3a928c0c1f4e199990141cc131100b_master.jpg", Price = 850000, IsFeatured = true },
            new Product { Name = "Son Kem Lì MAC Powder Kiss Liquid", ImageUrl = "https://product.hstatic.net/200000000133/product/son_kem_mac_powder_kiss_liquid_lipcolour_991_devoted_to_chili_1_18bc79fb665d4b85bd8ff8a5d3fdb983_master.jpg", Price = 780000, IsFeatured = true },
            new Product { Name = "Son Tint Bóng Romand Juicy Lasting Tint", ImageUrl = "https://product.hstatic.net/200000000133/product/son_tint_romand_juicy_lasting_tint_25_bare_grape_1_8f17bc3dbb3f46f4b3069b47e5b22108_master.jpg", Price = 209000, OldPrice = 280000, DiscountPercent = 25, IsDiscounted = true }
        },
        Vouchers = new List<Voucher> { new Voucher { Code = "BBNEW20", Title = "Giảm 20% Đơn Đầu Tiên", Description = "Tối đa 100k cho khách hàng đăng ký mới", ValidUntil = "30/06/2026" } }
    },

    // 7. PNJ
    new Shop
    {
        Name = "PNJ", Slug = "pnj", Floor = "Tầng 1", LocationSlot = "1-27",
        LogoUrl = "https://cdn.pnj.io/images/logo/pnj.com.vn.png",
        CoverImageUrl = "https://cdn.pnj.io/images/promo/193/top-banner-pc.jpg",
        Description = "Thương hiệu trang sức hàng đầu Việt Nam, mang lại niềm tự hào cho khách hàng bằng các sản phẩm tinh tế, chất lượng.",
        Slogan = "Niềm tin và Phong cách", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Bông tai Vàng 18K đính đá CZ PNJ", ImageUrl = "https://cdn.pnj.io/images/detailed/141/gbxmxmy001099-bong-tai-vang-18k-dinh-da-cz-pnj.png", Price = 4250000, IsFeatured = true },
            new Product { Name = "Dây chuyền Bạc đính đá PNJSilver", ImageUrl = "https://cdn.pnj.io/images/detailed/146/gnsmmxw000305-day-chuyen-bac-dinh-da-pnjsilver.png", Price = 850000, IsFeatured = true },
            new Product { Name = "Nhẫn Kim cương Vàng 14K PNJ", ImageUrl = "https://cdn.pnj.io/images/detailed/189/gnddddw004543-nhan-kim-cuong-vang-14k-pnj.png", Price = 18500000, OldPrice = 19500000, DiscountPercent = 5, IsDiscounted = true }
        }
    },

    // 8. PEDRO
    new Shop
    {
        Name = "Pedro", Slug = "pedro", Floor = "Tầng 1", LocationSlot = "1-09",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/4/49/Pedro_Logo.png", // Dùng icon phụ nếu không hiển thị
        CoverImageUrl = "https://media.maisononline.vn/sys_master/images/h6a/h7d/9431326490654/campaign-desktop-banner-pedro.jpg",
        Description = "Thương hiệu thời trang quốc tế chuyên về giày dép và phụ kiện cao cấp dành cho cả nam và nữ với phong cách lịch lãm, hiện đại.",
        Slogan = "Effortless Essentials", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Giày Lười Nam Leather Loafers", ImageUrl = "https://media.maisononline.vn/sys_master/images/hea/h19/9274577879070/PM1-40940172-1_01.jpg", Price = 2590000, IsFeatured = true },
            new Product { Name = "Túi Đeo Chéo Nam Nylon Messenger", ImageUrl = "https://media.maisononline.vn/sys_master/images/h5a/hf0/9248742514718/PM2-26320141_01.jpg", Price = 1890000, IsFeatured = true },
            new Product { Name = "Túi Xách Nữ Studio Leather Top Handle", ImageUrl = "https://media.maisononline.vn/sys_master/images/h04/h27/9385078554654/PW2-56610098_01.jpg", Price = 1690000, OldPrice = 2190000, DiscountPercent = 22, IsDiscounted = true }
        }
    },

    // 9. CASIO
    new Shop
    {
        Name = "Casio", Slug = "casio", Floor = "Tầng 1", LocationSlot = "K1-11",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/e/eb/Casio_logo.svg",
        CoverImageUrl = "https://www.casio.com/content/casio/locales/vn/vi/products/watches/gshock/_jcr_content/root/responsivegrid/container_1027170154/teaser.casiocoreimg.jpeg/1684305886915/g-shock-banner-pc.jpeg",
        Description = "Đại lý phân phối đồng hồ chính hãng Casio Nhật Bản. Chuyên các dòng G-Shock chống sốc, Baby-G năng động và Edifice thanh lịch.",
        Slogan = "Toughness & Technology", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Đồng hồ G-Shock GA-2100-1A1DR (CasiOak)", ImageUrl = "https://www.casio.com/content/dam/casio/product-info/locales/vn/vi/timepiece/product/watch/G/GA/GA-2/GA-2100-1A1/assets/GA-2100-1A1_1.png.transform/main-visual-pc/image.png", Price = 3280000, IsFeatured = true },
            new Product { Name = "Đồng hồ Nữ Vintage LA680WA", ImageUrl = "https://www.casio.com/content/dam/casio/product-info/locales/vn/vi/timepiece/product/watch/L/LA/LA6/LA680WA-7/assets/LA680WA-7_1.png.transform/main-visual-pc/image.png", Price = 1150000, IsFeatured = true },
            new Product { Name = "Đồng hồ Nam Edifice EFV-550L", ImageUrl = "https://www.casio.com/content/dam/casio/product-info/locales/vn/vi/timepiece/product/watch/E/EF/EFV/EFV-550L-1A/assets/EFV-550L-1AV_1.png.transform/main-visual-pc/image.png", Price = 2500000, OldPrice = 3100000, DiscountPercent = 19, IsDiscounted = true }
        }
    },

          // ==========================================
    // TẦNG 2: NHÀ SÁCH, ĐỒ CHƠI & THỜI TRANG NAM (9 CỬA HÀNG)
    // ==========================================

    // 10. PHƯƠNG NAM BOOK CITY
    new Shop
    {
        Name = "Phương Nam Book City", Slug = "phuong-nam", Floor = "Tầng 2", LocationSlot = "3-01",
        LogoUrl = "https://nhasachphuongnam.com/images/logos/256/logo-pnc.png",
        CoverImageUrl = "https://bizweb.dktcdn.net/100/363/355/files/hinh-anh-nha-sach-phuong-nam-khong-gian-van-hoa-doc-dep-mat.jpg",
        Description = "Không gian văn hóa kết hợp giữa sách, văn phòng phẩm cao cấp và đồ chơi giáo dục. Điểm đến lý tưởng cho người yêu sách.",
        Slogan = "Hành trình tri thức, Gắn kết yêu thương", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Sách Muôn Kiếp Nhân Sinh - Nguyên Phong", ImageUrl = "https://nhasachphuongnam.com/images/detailed/164/muon-kiep-nhan-sinh-b2.jpg", Price = 168000, IsFeatured = true },
            new Product { Name = "Sách Đắc Nhân Tâm (Khổ Lớn)", ImageUrl = "https://nhasachphuongnam.com/images/detailed/161/dac-nhan-tam.jpg", Price = 88000, IsFeatured = true },
            new Product { Name = "Bút Máy Lamy Safari M", ImageUrl = "https://nhasachphuongnam.com/images/detailed/160/lamy-safari.jpg", Price = 690000, IsFeatured = true },
            new Product { Name = "Sách Cây Cam Ngọt Của Tôi", ImageUrl = "https://nhasachphuongnam.com/images/detailed/209/cay-cam-ngot-cua-toi.jpg", Price = 86400, OldPrice = 108000, DiscountPercent = 20, IsDiscounted = true }
        },
        Vouchers = new List<Voucher> { new Voucher { Code = "PNBCOCT", Title = "Tặng Bookmark Đồng", Description = "Cho hóa đơn mua sách Văn học trên 500k", ValidUntil = "31/12/2026" } }
    },

    // 11. POP MART
    new Shop
    {
        Name = "Pop Mart", Slug = "pop-mart", Floor = "Tầng 2", LocationSlot = "3-15",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/f/fb/Pop_Mart_logo.svg",
        CoverImageUrl = "https://cdn.tuoitre.vn/471584752817336320/2024/5/17/img-8386-17159392176461974121287.jpg",
        Description = "Thương hiệu đồ chơi nghệ thuật (Art Toy) hàng đầu thế giới với trào lưu Blind Box đình đám. Sở hữu các IP độc quyền hot nhất.",
        Slogan = "To Light Up Passion and Bring Joy", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Hộp mù THE MONSTERS - Tasty Macarons (Labubu)", ImageUrl = "https://media.karousell.com/media/photos/products/2023/12/3/pop_mart_labubu_macaron_1701584742_91a0c02c_progressive.jpg", Price = 450000, IsFeatured = true },
            new Product { Name = "Hộp mù SKULLPANDA Image of Reality", ImageUrl = "https://global.popmart.com/cdn/shop/files/7_7b84a9e5-9f5b-4357-9db0-5e3e14fb5e74.jpg", Price = 280000, IsFeatured = true },
            new Product { Name = "Mô hình MEGA SPACE MOLLY 400%", ImageUrl = "https://global.popmart.com/cdn/shop/files/1_dbca665c-3f9c-4ebc-88e5-eb65ea4e4f8d.jpg", Price = 5500000, IsFeatured = true },
            new Product { Name = "Hộp mù DIMOO Retro Paperboy", ImageUrl = "https://global.popmart.com/cdn/shop/files/1_f3f1ed1a-3e5e-49b2-a42e-13b30366a6b5.jpg", Price = 224000, OldPrice = 280000, DiscountPercent = 20, IsDiscounted = true }
        }
    },

    // 12. MINISO
    new Shop
    {
        Name = "Miniso", Slug = "miniso", Floor = "Tầng 2", LocationSlot = "3-03A",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/e/ea/Miniso_logo.svg",
        CoverImageUrl = "https://minisovietnam.com.vn/wp-content/uploads/2023/10/banner-web-1.jpg",
        Description = "Thương hiệu bán lẻ đa quốc gia cung cấp các mặt hàng gia dụng, đồ chơi, mỹ phẩm và đồ dùng tiện ích với thiết kế sáng tạo, giá rẻ.",
        Slogan = "Life is for fun", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Gấu bông Capybara ôm gà Miniso", ImageUrl = "https://minisovietnam.com.vn/wp-content/uploads/2024/01/6936735398282-1.jpg", Price = 199000, IsFeatured = true },
            new Product { Name = "Bình giữ nhiệt Miniso Lotso 500ml", ImageUrl = "https://minisovietnam.com.vn/wp-content/uploads/2023/08/6936735368384-1.jpg", Price = 259000, IsFeatured = true },
            new Product { Name = "Tai nghe Bluetooth Miniso Sanrio", ImageUrl = "https://minisovietnam.com.vn/wp-content/uploads/2023/07/6936735352321-1.jpg", Price = 399000, OldPrice = 499000, DiscountPercent = 20, IsDiscounted = true }
        }
    },

    // 13. NINOMAXX
    new Shop
    {
        Name = "Ninomaxx", Slug = "ninomaxx", Floor = "Tầng 2", LocationSlot = "3-31",
        LogoUrl = "https://ninomaxxconcept.com/wp-content/uploads/2023/02/Logo-Ninomaxx-Concept.png",
        CoverImageUrl = "https://ninomaxxconcept.com/wp-content/uploads/2023/11/Banner-Web-1920x800-1.jpg",
        Description = "Thương hiệu thời trang ứng dụng (Casual Wear) Việt Nam, tiên phong mang đến phong cách năng động, trẻ trung cho mọi lứa tuổi.",
        Slogan = "Beyond Casual", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Áo Thun Polo Nam Phối Bo Cổ", ImageUrl = "https://ninomaxxconcept.com/wp-content/uploads/2023/11/2311025-1.jpg", Price = 459000, IsFeatured = true },
            new Product { Name = "Quần Khaki Nam Dáng Slimfit", ImageUrl = "https://ninomaxxconcept.com/wp-content/uploads/2023/10/2310118-1.jpg", Price = 599000, IsFeatured = true },
            new Product { Name = "Áo Khoác Gió Nam Cản Nước", ImageUrl = "https://ninomaxxconcept.com/wp-content/uploads/2023/11/2311054-1.jpg", Price = 489000, OldPrice = 699000, DiscountPercent = 30, IsDiscounted = true }
        }
    },

    // 14. LEVENTS (Local Brand)
    new Shop
    {
        Name = "Levents", Slug = "levents", Floor = "Tầng 2", LocationSlot = "3-07",
        LogoUrl = "https://levents.asia/wp-content/uploads/2021/10/logo.png",
        CoverImageUrl = "https://file.hstatic.net/1000312752/file/111_02c770c068304ddfa0c0ab58da5b81cc.jpg",
        Description = "Thương hiệu thời trang Local Brand dành cho giới trẻ, đi đầu về chất lượng và xu hướng thiết kế Streetwear hiện đại.",
        Slogan = "Levents® - Make Everything Become Popular", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Levents® Classic Zipper Hoodie", ImageUrl = "https://product.hstatic.net/1000312752/product/ao_khoac_ni_hoodie_zip_local_brand_levents_classic_zipper_hoodie_black_2_0c3bf310b1a04d2ab9c704f0f089f268_master.jpg", Price = 650000, IsFeatured = true },
            new Product { Name = "Levents® Basic Tee", ImageUrl = "https://product.hstatic.net/1000312752/product/ao_thun_local_brand_levents_basic_tee_white_2_d15f2122607e4d8fb74f260bc34ba0bb_master.jpg", Price = 350000, IsFeatured = true },
            new Product { Name = "Levents® Mini Logo Backpack", ImageUrl = "https://product.hstatic.net/1000312752/product/balo_local_brand_levents_mini_logo_backpack_black_1_bb481878b27c4b4d8ec8891d1eeb2c0b_master.jpg", Price = 490000, OldPrice = 690000, DiscountPercent = 28, IsDiscounted = true }
        },
        Vouchers = new List<Voucher> { new Voucher { Code = "LVTFREE", Title = "Freeship Đơn 500K", Description = "Áp dụng cho mọi hình thức thanh toán", ValidUntil = "Không thời hạn" } }
    },

    // 15. RABITY
    new Shop
    {
        Name = "Rabity", Slug = "rabity", Floor = "Tầng 2", LocationSlot = "3-03",
        LogoUrl = "https://rabity.vn/cdn/shop/files/logo_rabity_1_180x.png",
        CoverImageUrl = "https://rabity.vn/cdn/shop/files/Banner_Web_2000x800_14_1920x.png",
        Description = "Thương hiệu thời trang trẻ em cao cấp hàng đầu Việt Nam. Chất liệu tự nhiên, an toàn, thoải mái cho bé vui chơi cả ngày.",
        Slogan = "Tự do khám phá", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Áo Thun Bé Trai In Hình Marvel", ImageUrl = "https://rabity.vn/cdn/shop/files/92408_1_540x.png", Price = 159000, IsFeatured = true },
            new Product { Name = "Đầm Váy Bé Gái Công Chúa Elsa", ImageUrl = "https://rabity.vn/cdn/shop/files/92398_1_540x.png", Price = 329000, IsFeatured = true },
            new Product { Name = "Bộ Ngắn Tay Bé Trai Mickey", ImageUrl = "https://rabity.vn/cdn/shop/files/92405_1_540x.png", Price = 175000, OldPrice = 250000, DiscountPercent = 30, IsDiscounted = true }
        }
    },

    // 16. BOO
    new Shop
    {
        Name = "Boo", Slug = "boo", Floor = "Tầng 2", LocationSlot = "3-08",
        LogoUrl = "https://boo.vn/wp-content/uploads/2021/04/logo-boo.svg", // Giả sử icon Boo
        CoverImageUrl = "https://boo.vn/wp-content/uploads/2023/12/BOO-Marvel-Banner-PC.jpg",
        Description = "Thương hiệu thời trang đường phố tiên phong tại Việt Nam, mang tinh thần trẻ trung, phá cách và luôn cập nhật xu hướng.",
        Slogan = "BOO - Bò Sữa", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Áo Phông Nam BOO in Marvel", ImageUrl = "https://boo.vn/wp-content/uploads/2023/12/1-4-600x800.jpg", Price = 349000, IsFeatured = true },
            new Product { Name = "Quần Jeans Nam Ống Rộng Tôn Dáng", ImageUrl = "https://boo.vn/wp-content/uploads/2023/11/1-600x800.jpg", Price = 599000, IsFeatured = true },
            new Product { Name = "Áo Nỉ Có Mũ BOOZilla", ImageUrl = "https://boo.vn/wp-content/uploads/2023/10/1-1-600x800.jpg", Price = 489000, OldPrice = 699000, DiscountPercent = 30, IsDiscounted = true }
        }
    },

    // 17. JOHN HENRY
    new Shop
    {
        Name = "John Henry", Slug = "john-henry", Floor = "Tầng 2", LocationSlot = "3-09",
        LogoUrl = "https://johnhenry.vn/cdn/shop/files/Logo_JH_White_160x.png",
        CoverImageUrl = "https://johnhenry.vn/cdn/shop/files/Banner_Web_PC_1_1920x.jpg",
        Description = "Thời trang nam phong cách Mỹ, mang đến vẻ ngoài lịch lãm, nam tính nhưng vẫn thoải mái cho dân công sở.",
        Slogan = "American Classic", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Áo Sơ Mi Nam Tay Dài Trắng Chống Nhăn", ImageUrl = "https://johnhenry.vn/cdn/shop/files/SM2301_1_540x.jpg", Price = 750000, IsFeatured = true },
            new Product { Name = "Quần Tây Nam Form Regular Fit", ImageUrl = "https://johnhenry.vn/cdn/shop/files/QT2305_1_540x.jpg", Price = 890000, IsFeatured = true },
            new Product { Name = "Áo Polo Nam Sợi Bamboo", ImageUrl = "https://johnhenry.vn/cdn/shop/files/PL2310_1_540x.jpg", Price = 450000, OldPrice = 590000, DiscountPercent = 23, IsDiscounted = true }
        }
    },

    // 18. LUG.VN (Đẩy lên Tầng 2 theo sơ đồ thực tế hoặc rải đều)
    new Shop
    {
        Name = "Lug.vn", Slug = "lugvn", Floor = "Tầng 2", LocationSlot = "3-12",
        LogoUrl = "https://lug.vn/v2/img/logo.png",
        CoverImageUrl = "https://bizweb.dktcdn.net/100/363/355/files/lug-vn-cua-hang-vali-chinh-hang.jpg",
        Description = "Hệ thống phân phối các thương hiệu vali, balo, túi xách, phụ kiện chính hãng quốc tế lớn nhất Việt Nam.",
        Slogan = "Gói trọn mọi hành trình", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Vali kéo nhựa Echolac", ImageUrl = "https://lug.vn/images/products/2023/11/17/large/vali-nhua-echolac-pc185sa-20-do_1700201633.jpg", Price = 2500000, IsFeatured = true },
            new Product { Name = "Balo chống nước Lusetti", ImageUrl = "https://lug.vn/images/products/2023/08/11/large/balo-laptop-lusetti-lsb1150-den_1691740924.jpg", Price = 499000, OldPrice = 990000, DiscountPercent = 50, IsDiscounted = true }
        },
        Vouchers = new List<Voucher> { new Voucher { Code = "LUGTRIP", Title = "Giảm 10% Vali Size L", Description = "Không áp dụng cùng CTKM khác", ValidUntil = "Hết hạn tuần này" } }
    },

     // ==========================================
    // TẦNG 3: THỂ THAO ĐƯỜNG PHỐ & GIẢI TRÍ (3 CỬA HÀNG)
    // ==========================================

    // 19. POWERBOWL 388 (Khu giải trí theo sơ đồ Map)
    new Shop
    {
        Name = "Powerbowl 388", Slug = "powerbowl", Floor = "Tầng 3", LocationSlot = "5-01",
        LogoUrl = "https://powerbowl.vn/wp-content/uploads/2020/09/logo-powerbowl-01.png", // Text logo thay thế
        CoverImageUrl = "https://powerbowl.vn/wp-content/uploads/2020/12/127116810_1079374095844426_8373307559135062638_o.jpg",
        Description = "Trung tâm giải trí Powerbowl mang đến không gian Bowling hiện đại, khu vui chơi Arcade Games và Billiards đẳng cấp.",
        Slogan = "Entertainment Center", OpenTime = "09:30", CloseTime = "23:00",
        Products = new List<Product>
        {
            new Product { Name = "Vé Chơi Bowling (1 Game / Người)", ImageUrl = "https://powerbowl.vn/wp-content/uploads/2020/12/127116810_1079374095844426_8373307559135062638_o.jpg", Price = 70000, IsFeatured = true },
            new Product { Name = "Combo 50 Xu Chơi Game Arcade", ImageUrl = "https://media.timeout.com/images/105304123/image.jpg", Price = 250000, OldPrice = 300000, DiscountPercent = 16, IsDiscounted = true }
        }
    },

    // 20. VANS
    new Shop
    {
        Name = "Vans", Slug = "vans", Floor = "Tầng 3", LocationSlot = "5-04",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/9/9eb/Vans_logo.svg",
        CoverImageUrl = "https://media.maisononline.vn/sys_master/images/h29/he1/9483162746910/VANS-D1-1920X720.jpg",
        Description = "Vans là thương hiệu giày trượt ván và thời trang đường phố biểu tượng toàn cầu, đại diện cho tinh thần sáng tạo và văn hóa giới trẻ.",
        Slogan = "Off The Wall", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Giày Sneaker Unisex Vans Old Skool", ImageUrl = "https://media.maisononline.vn/sys_master/images/h64/h5b/9239846387742/VN000D3HY28_01.jpg", Price = 1750000, IsFeatured = true },
            new Product { Name = "Giày Sneaker Unisex Vans Slip-On Checkerboard", ImageUrl = "https://media.maisononline.vn/sys_master/images/h81/hf1/9239845765150/VN000EYEBWW_01.jpg", Price = 1450000, IsFeatured = true },
            new Product { Name = "Balo Unisex Vans Old Skool Drop V", ImageUrl = "https://media.maisononline.vn/sys_master/images/hab/hf6/9341499547678/VN0A5KHPY28_01.jpg", Price = 850000, OldPrice = 1100000, DiscountPercent = 22, IsDiscounted = true }
        },
        Vouchers = new List<Voucher> { new Voucher { Code = "VANS10", Title = "Giảm 10% Hàng Mới", Description = "Áp dụng cho Bộ sưu tập Knu Skool", ValidUntil = "30/06/2026" } }
    },

    // 21. CONVERSE
    new Shop
    {
        Name = "Converse", Slug = "converse", Floor = "Tầng 3", LocationSlot = "5-05",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/3/30/Converse_logo.svg",
        CoverImageUrl = "https://media.maisononline.vn/sys_master/images/h70/h53/9463567794206/CONVERSE-D1-1920X720.jpg",
        Description = "Converse - Biểu tượng của thời trang đường phố từ năm 1908. Khẳng định cá tính với những thiết kế Chuck Taylor huyền thoại.",
        Slogan = "Create Next", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Giày Sneaker Unisex Chuck Taylor All Star Classic", ImageUrl = "https://media.maisononline.vn/sys_master/images/hdf/hec/9253457199134/M9160C_01.jpg", Price = 1500000, IsFeatured = true },
            new Product { Name = "Giày Sneaker Unisex Chuck 70 Vintage Canvas", ImageUrl = "https://media.maisononline.vn/sys_master/images/hf5/hc5/9280453308446/162058C_01.jpg", Price = 2200000, IsFeatured = true },
            new Product { Name = "Áo Thun Unisex Converse Go-To", ImageUrl = "https://media.maisononline.vn/sys_master/images/h1d/hce/9385078554654/10023883-A01_01.jpg", Price = 450000, OldPrice = 650000, DiscountPercent = 30, IsDiscounted = true }
        }
    },

    // ==========================================
    // TẦNG 4: CÔNG NGHỆ & LIFESTYLE (2 CỬA HÀNG)
    // ==========================================

    // 22. SONY CENTER
    new Shop
    {
        Name = "Sony Center", Slug = "sony-center", Floor = "Tầng 4", LocationSlot = "6-06",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c4/Sony_logo.svg",
        CoverImageUrl = "https://sonycenter.sony.com.vn/Data/Sites/1/media/sony-center-vietnam.jpg",
        Description = "Trải nghiệm thế giới công nghệ đỉnh cao từ Sony. Phân phối chính hãng các dòng Tivi Bravia, máy chơi game PlayStation, máy ảnh Alpha và tai nghe.",
        Slogan = "Be Moved", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "Máy chơi game PlayStation®5 (PS5)", ImageUrl = "https://sonycenter.sony.com.vn/Data/Sites/1/Product/3729/ps5_01.png", Price = 14990000, IsFeatured = true },
            new Product { Name = "Tai nghe chống ồn WH-1000XM5", ImageUrl = "https://sonycenter.sony.com.vn/Data/Sites/1/Product/3932/wh-1000xm5_01.png", Price = 7990000, IsFeatured = true },
            new Product { Name = "Loa Bluetooth Không Dây SRS-XB13", ImageUrl = "https://sonycenter.sony.com.vn/Data/Sites/1/Product/3602/srs-xb13_01.png", Price = 990000, OldPrice = 1490000, DiscountPercent = 33, IsDiscounted = true }
        },
        Vouchers = new List<Voucher> { new Voucher { Code = "SONYPS5", Title = "Tặng Tay Cầm DualSense", Description = "Khi mua máy PS5 bản ổ đĩa", ValidUntil = "31/05/2026" } }
    },

    // 23. LEGO STORE
    new Shop
    {
        Name = "LEGO", Slug = "lego", Floor = "Tầng 4", LocationSlot = "6-02",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/2/24/LEGO_logo.svg",
        CoverImageUrl = "https://theme.hstatic.net/1000287106/1000984815/14/banner_aboutus.jpg",
        Description = "Cửa hàng đồ chơi lắp ráp LEGO chính hãng. Nơi khơi nguồn sáng tạo cho mọi lứa tuổi với hàng ngàn bộ mô hình đa dạng.",
        Slogan = "Play On", OpenTime = "09:30", CloseTime = "22:00",
        Products = new List<Product>
        {
            new Product { Name = "LEGO Technic 42151 Bugatti Bolide", ImageUrl = "https://product.hstatic.net/1000287106/product/42151_4a8d0b5e9f1947b18fc4f794b68e98a3_master.jpg", Price = 1599000, IsFeatured = true },
            new Product { Name = "LEGO Creator Hoa Lan (Orchid)", ImageUrl = "https://product.hstatic.net/1000287106/product/10311_1_25b04c868eb34421b9cfca784578b871_master.jpg", Price = 1699000, IsFeatured = true },
            new Product { Name = "LEGO City Xe Cảnh Sát Chở Tù Nhân", ImageUrl = "https://product.hstatic.net/1000287106/product/60312_1_c8b417e3f84f494fb2a3c75d3121d5a7_master.jpg", Price = 599000, OldPrice = 759000, DiscountPercent = 21, IsDiscounted = true }
        }
    }
        };

        await dbSet.AddRangeAsync(shops);
        await context.SaveChangesAsync();
    }

    public static async Task AutoFillMissingShopsAsync(AppDbContext context)
    {
        // 1. Lấy danh sách 23 shop "Thật" đã có đầy đủ Products/Vouchers để làm mẫu (Template)
        var templates = await context.Shops
            .Include(s => s.Products)
            .Include(s => s.Vouchers)
            .ToListAsync();

        if (!templates.Any()) return;

        // 2. Lấy tất cả vị trí từ Map có URL bắt đầu bằng /shops/ 
        // (Điều này tự động loại trừ /food/ và /movies theo luật của bạn)
        var allMapLocations = await context.MapLocations
            .Where(m => m.ShopUrl.StartsWith("/shops/"))
            .ToListAsync();

        var random = new Random();

        foreach (var location in allMapLocations)
        {
            // Trích xuất slug từ URL (VD: /shops/ecco -> ecco)
            var slug = location.ShopUrl.Replace("/shops/", "").ToLower();

            // Kiểm tra xem Shop này đã tồn tại trong bảng dữ liệu chi tiết chưa
            var exists = await context.Shops.AnyAsync(s => s.Slug == slug);

            if (!exists)
            {
                // 3. Bốc ngẫu nhiên 1 shop mẫu từ danh sách 23 shop thật
                var randomTemplate = templates[random.Next(templates.Count)];

                // 4. Tạo Shop mới với tên và slug của Map nhưng lấy "ruột" của Template
                var filledShop = new Shop
                {
                    Name = location.ShopName,
                    Slug = slug,
                    Floor = randomTemplate.Floor,
                    LocationSlot = location.LocationSlot,
                    LogoUrl = randomTemplate.LogoUrl,
                    CoverImageUrl = randomTemplate.CoverImageUrl,
                    Description = randomTemplate.Description,
                    Slogan = randomTemplate.Slogan,
                    OpenTime = randomTemplate.OpenTime,
                    CloseTime = randomTemplate.CloseTime,

                    // Sao chép danh sách sản phẩm từ mẫu
                    Products = randomTemplate.Products.Select(p => new Product
                    {
                        Name = p.Name,
                        ImageUrl = p.ImageUrl,
                        Price = p.Price,
                        OldPrice = p.OldPrice,
                        DiscountPercent = p.DiscountPercent,
                        IsFeatured = p.IsFeatured,
                        IsDiscounted = p.IsDiscounted
                    }).ToList(),

                    // Sao chép Voucher từ mẫu
                    Vouchers = randomTemplate.Vouchers.Select(v => new Voucher
                    {
                        Code = v.Code,
                        Title = v.Title,
                        Description = v.Description,
                        ValidUntil = v.ValidUntil
                    }).ToList()
                };

                context.Shops.Add(filledShop);
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Đã lấp đầy dữ liệu cho các shop còn trống bằng dữ liệu ngẫu nhiên!");
    }

}

