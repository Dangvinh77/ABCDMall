import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';

const slides = [
    { id: 1, title: "Khai trương Zara Store", desc: "Giảm giá 50% toàn bộ mặt hàng trong tuần đầu tiên.", image: "https://images.unsplash.com/photo-1441984904996-e0b6ba687e08?q=80&w=2000&auto=format&fit=crop", link: "/shops/zara", cta: "Khám phá ngay" },
    { id: 2, title: "Lễ hội Ẩm thực Châu Á", desc: "Thưởng thức hàng trăm món ngon tại Food Court.", image: "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?q=80&w=2000&auto=format&fit=crop", link: "/food-court", cta: "Xem thực đơn" },
    { id: 3, title: "Ra mắt Avengers: Doomsday", desc: "Đặt vé sớm nhận ngay combo Popcorn giới hạn.", image: "https://images.unsplash.com/photo-1536440136628-849c177e76a1?q=80&w=2000&auto=format&fit=crop", link: "/movies", cta: "Đặt vé ngay" },
    { id: 4, title: "Tuần lễ Thời trang 2026", desc: "Sự kiện catwalk với các thương hiệu đình đám.", image: "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?q=80&w=2000&auto=format&fit=crop", link: "/gallery", cta: "Xem chi tiết" },
    { id: 5, title: "Nike Air Max Mới", desc: "Độc quyền tại ABCD Mall. Số lượng có hạn.", image: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?q=80&w=2000&auto=format&fit=crop", link: "/shops/nike", cta: "Thử ngay" },
    { id: 6, title: "Buffet Hải Sản Mua 3 Tặng 1", desc: "Ưu đãi cuối tuần tại nhà hàng Ocean Blue.", image: "https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?q=80&w=2000&auto=format&fit=crop", link: "/food-court/ocean-blue", cta: "Nhận ưu đãi" },
    { id: 7, title: "Triển lãm Nghệ thuật Đương đại", desc: "Mở cửa miễn phí tại sảnh chính Tầng 1.", image: "https://images.unsplash.com/photo-1518998053901-5348d3961a04?q=80&w=2000&auto=format&fit=crop", link: "/gallery", cta: "Tham gia ngay" },
    { id: 8, title: "Midnight Sale - Up to 70%", desc: "Duy nhất ngày 20/04. Săn sale xuyên màn đêm.", image: "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?q=80&w=2000&auto=format&fit=crop", link: "/shops", cta: "Săn deal ngay" },
];

const slideTranslations: Record<number, { title: string; desc: string; cta: string }> = {
    1: { title: "Zara Store Grand Opening", desc: "Enjoy 50% off all items during the first week.", cta: "Explore Now" },
    2: { title: "Asian Food Festival", desc: "Enjoy hundreds of delicious dishes at the Food Court.", cta: "View Menu" },
    3: { title: "Avengers: Doomsday Premiere", desc: "Book early and get a limited popcorn combo.", cta: "Book Now" },
    4: { title: "Fashion Week 2026", desc: "A catwalk event featuring standout fashion brands.", cta: "View Details" },
    5: { title: "New Nike Air Max", desc: "Exclusive at ABCD Mall. Limited stock available.", cta: "Try It Now" },
    6: { title: "Seafood Buffet Buy 3 Get 1", desc: "Weekend promotion at Ocean Blue restaurant.", cta: "Get Offer" },
    7: { title: "Contemporary Art Exhibition", desc: "Free entry at the main hall on Floor 1.", cta: "Join Now" },
    8: { title: "Midnight Sale - Up to 70%", desc: "One day only on 20/04. Hunt deals all night long.", cta: "Shop Deals" },
};

export const PromoCarousel = () => {
    const [current, setCurrent] = useState(0);

    useEffect(() => {
        const timer = setInterval(() => {
            setCurrent((prev) => (prev === slides.length - 1 ? 0 : prev + 1));
        }, 5000);
        return () => clearInterval(timer);
    }, []);

    return (
        // THẺ CHA: padding-bottom 12 để tạo chỗ trống cho các nút tròn
        <div className="relative w-full max-w-[96%] xl:max-w-[90%] mx-auto pt-8 pb-12">

            {/* KHUNG ẢNH: Chiều cao tăng lên 85vh, bo góc lớn hơn */}
            <div className="relative w-full h-[75vh] md:h-[85vh] rounded-[2.5rem] overflow-hidden shadow-2xl">
                <div
                    className="flex w-full h-full transition-transform duration-700 ease-in-out"
                    style={{ transform: `translateX(-${current * 100}%)` }}
                >
                    {slides.map((slide) => {
                        const translated = slideTranslations[slide.id];
                        return (
                        <div key={slide.id} className="min-w-full h-full relative group">
                            <img src={slide.image} alt={translated.title} className="w-full h-full object-cover" />
                            {/* Lớp phủ tối để chữ dễ đọc */}
                            <div className="absolute inset-0 bg-gradient-to-t from-mall-dark/90 via-mall-dark/20 to-transparent" />

                            <div className="absolute bottom-16 left-8 md:left-16 right-8 text-white">
                                <h2 className="text-4xl md:text-6xl font-extrabold mb-4 drop-shadow-lg animate-fade-in-up">{translated.title}</h2>
                                <p className="text-lg md:text-2xl mb-8 text-gray-200 max-w-2xl drop-shadow-md">{translated.desc}</p>
                                <Link
                                    to={slide.link}
                                    className="inline-block bg-mall-primary hover:bg-mall-secondary text-white font-bold text-lg py-4 px-10 rounded-full shadow-[0_0_15px_rgba(255,65,108,0.5)] hover:shadow-[0_0_25px_rgba(255,65,108,0.8)] hover:scale-105 transition-all duration-300"
                                >
                                    {translated.cta}
                                </Link>
                            </div>
                        </div>
                    )})}
                </div>
            </div>

            {/* CÁC NÚT TRÒN: Đặt ở thẻ cha, tuyệt đối an toàn không bị cắt mất */}
            <div className="absolute bottom-2 left-0 right-0 flex justify-center gap-3">
                {slides.map((_, index) => (
                    <button
                        key={index}
                        onClick={() => setCurrent(index)}
                        className={`h-3 rounded-full transition-all duration-300 ${current === index ? "w-12 bg-mall-primary shadow-lg" : "w-3 bg-gray-300 hover:bg-gray-400"
                            }`}
                        aria-label={`Go to slide ${index + 1}`}
                    />
                ))}
            </div>
        </div>
    );
};
