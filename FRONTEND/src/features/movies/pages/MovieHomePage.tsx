import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Film,
  Popcorn,
  Ticket,
  Clock,
  TrendingUp,
  Sparkles,
  ChevronRight,
} from 'lucide-react'
import { Button } from '../component/ui/button'
import { Badge } from '../component/ui/badge'
import { Tabs, TabsList, TabsTrigger } from '../component/ui/tabs'
import { MovieCard } from './MovieCard'
import { PromoCard } from './PromoCard'
import { nowShowingMovies, comingSoonMovies } from '../data/movie'

const promos = [
  {
    title: 'Ưu đãi cuối tuần',
    description: 'Giảm giá cho tất cả suất chiếu T7-CN',
    discount: '30% OFF',
    color: 'bg-gradient-to-br from-purple-600 to-purple-800',
  },
  {
    title: 'Combo bắp nước',
    description: 'Mua combo tiết kiệm 50.000đ',
    discount: '50K',
    color: 'bg-gradient-to-br from-pink-600 to-pink-800',
  },
  {
    title: 'Member đặc biệt',
    description: 'Đăng ký thành viên nhận voucher',
    discount: 'FREE',
    color: 'bg-gradient-to-br from-cyan-600 to-cyan-800',
  },
]

export function MovieHomePage() {
  const [selectedCategory, setSelectedCategory] = useState('all')
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-950 via-gray-700 to-gray-900">
      {/* Header */}
      <header className="sticky top-0 z-20 border-b border-white/10 bg-[#040816]/35 backdrop-blur-md">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
          <div className="flex items-center gap-3">
            <div className="flex size-11 items-center justify-center rounded-full bg-gradient-to-br from-violet-700 via-fuchsia-600 to-pink-500 shadow-lg shadow-fuchsia-950/35">
              <Film className="size-5 text-white" />
            </div>

            <div>
              <h1 className="text-xl font-bold leading-none tracking-tight text-white sm:text-[1.65rem]">
                ABCD Cinema
              </h1>
              <p className="mt-1 text-xs text-gray-400 sm:text-sm">
                Đặt vé online - Nhanh chóng và tiện lợi
              </p>
            </div>
          </div>

          <nav className="flex items-center gap-2 sm:gap-4">
            <Button
              variant="ghost"
              className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
            >
              <Clock className="mr-2 size-4" />
              Lịch chiếu
            </Button>
            <Button
              variant="ghost"
              className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
            >
              <Popcorn className="mr-2 size-4" />
              Đồ ăn
            </Button>
            <Button className="h-10 rounded-xl bg-gradient-to-r from-violet-700 via-fuchsia-600 to-pink-500 px-4 text-sm font-bold text-white shadow-lg shadow-fuchsia-950/35 hover:from-violet-600 hover:via-fuchsia-500 hover:to-pink-400 sm:h-11 sm:px-6 sm:text-base">
              <Ticket className="mr-2 size-4" />
              <span className="hidden sm:inline">Vé của tôi</span>
              <span className="sm:hidden">Vé</span>
            </Button>
          </nav>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-r from-purple-900/50 to-pink-900/50" />
        <img
          src="https://images.unsplash.com/photo-1515100235140-6cb3498e8031?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjaW5lbWElMjB0aGVhdGVyJTIwaW50ZXJpb3J8ZW58MXx8fHwxNzc0Njg5MzQ4fDA&ixlib=rb-4.1.0&q=80&w=1080"
          alt="Cinema"
          className="h-72 w-full object-cover opacity-50 sm:h-80 md:h-96"
        />
        <div className="absolute inset-0 flex items-center">
          <div className="container mx-auto px-4">
            <div className="max-w-2xl space-y-4 sm:space-y-6">
              <Badge className="bg-gradient-to-r from-yellow-500 to-orange-500 text-black">
                <Sparkles className="mr-1 size-3" />
                Trải nghiệm điện ảnh đỉnh cao
              </Badge>
              <h2 className="text-3xl font-bold text-white sm:text-4xl md:text-5xl">
                Đặt vé xem phim
                <br />
                <span className="bg-gradient-to-r from-purple-400 to-pink-400 bg-clip-text text-transparent">
                  Cực kỳ dễ dàng
                </span>
              </h2>
              <p className="text-base text-gray-300 sm:text-lg">
                Hệ thống đặt vé online tại ABCD Mall - Chọn phim, chọn suất, chọn ghế chỉ trong vài
                giây
              </p>
              <div className="flex flex-wrap gap-3 sm:gap-4">
                <Button
                  size="lg"
                  className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
                >
                  Khám phá ngay
                  <ChevronRight className="ml-2 size-4" />
                </Button>
                <Button size="lg" variant="outline" className="border-gray-600 text-white">
                  Xem lịch chiếu
                </Button>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Promotions Section */}
      <section className="py-10 sm:py-12">
        <div className="container mx-auto px-4">
          <div className="mb-6 flex items-center justify-between sm:mb-8">
            <div>
              <h2 className="text-2xl font-bold text-white sm:text-3xl">Ưu đãi dành cho bạn</h2>
              <p className="text-sm text-gray-400 sm:text-base">
                Tiết kiệm ngay với các chương trình khuyến mãi
              </p>
            </div>
            <Button variant="ghost" className="text-purple-400 hover:text-purple-300">
              Xem tất cả  
              <ChevronRight className="ml-1 size-4" />
            </Button>
          </div>

          <div className="grid gap-4 sm:gap-6 md:grid-cols-3">
            {promos.map((promo, index) => (
              <PromoCard key={index} {...promo} />
            ))}
          </div>
        </div>
      </section>

      {/* Now Showing Section */}
      <section className="py-10 sm:py-12">
        <div className="container mx-auto px-4">
          <div className="mb-6 flex flex-col gap-4 sm:mb-8 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
              <TrendingUp className="size-7 text-purple-500 sm:size-8" />
              <div>
                <h2 className="text-2xl font-bold text-white sm:text-3xl">Đang chiếu</h2>
                <p className="text-sm text-gray-400">Những bộ phim hot nhất hiện nay</p>
              </div>
            </div>

            <div className="overflow-x-auto pb-1 text-gray-200">
              <Tabs value={selectedCategory} onValueChange={setSelectedCategory}>
                <TabsList>
                  <TabsTrigger value="all">Tất cả</TabsTrigger>
                  <TabsTrigger value="action">Hành động</TabsTrigger>
                  <TabsTrigger value="scifi">Sci-Fi</TabsTrigger>
                  <TabsTrigger value="horror">Kinh dị</TabsTrigger>
                </TabsList>
              </Tabs>
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2 sm:gap-6 lg:grid-cols-4">
            {nowShowingMovies.map((movie) => (
              <MovieCard key={movie.id} {...movie} onClick={() => navigate(`/movie/${movie.id}`)} />
            ))}
          </div>
        </div>
      </section>

      {/* Coming Soon Section */}
      <section className="py-10 sm:py-12">
        <div className="container mx-auto px-4">
          <div className="mb-6 flex items-center justify-between sm:mb-8">
            <div className="flex items-center gap-3">
              <Sparkles className="size-7 text-cyan-500 sm:size-8" />
              <div>
                <h2 className="text-2xl font-bold text-white sm:text-3xl">Sắp chiếu</h2>
                <p className="text-sm text-gray-400">Đừng bỏ lỡ những bom tấn sắp ra mắt</p>
              </div>
            </div>
            <Button variant="ghost" className="text-cyan-400 hover:text-cyan-300">
              Xem thêm
              <ChevronRight className="ml-1 size-4" />
            </Button>
          </div>

          <div className="grid gap-4 sm:grid-cols-2 sm:gap-6 lg:grid-cols-3">
            {comingSoonMovies.map((movie) => (
              <MovieCard key={movie.id} {...movie} onClick={() => navigate(`/movie/${movie.id}`)} />
            ))}
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t border-gray-800 bg-gray-950 py-8">
        <div className="container mx-auto px-4">
          <div className="grid gap-8 sm:grid-cols-2 md:grid-cols-4">
            <div>
              <h3 className="mb-4 font-semibold text-white">Về ABCD Cinema</h3>
              <p className="text-sm text-gray-400">
                Hệ thống đặt vé online tại ABCD Mall - Mang đến trải nghiệm xem phim tuyệt vời nhất
              </p>
            </div>
            <div>
              <h3 className="mb-4 font-semibold text-white">Liên kết nhanh</h3>
              <ul className="space-y-2 text-sm text-gray-400">
                <li>Lịch chiếu</li>
                <li>Giá vé</li>
                <li>Khuyến mãi</li>
                <li>Tin tức</li>
              </ul>
            </div>
            <div>
              <h3 className="mb-4 font-semibold text-white">Hỗ trợ</h3>
              <ul className="space-y-2 text-sm text-gray-400">
                <li>Câu hỏi thường gặp</li>
                <li>Chính sách</li>
                <li>Điều khoản</li>
                <li>Liên hệ</li>
              </ul>
            </div>
            <div>
              <h3 className="mb-4 font-semibold text-white">Liên hệ</h3>
              <ul className="space-y-2 text-sm text-gray-400">
                <li>Email: info@abcdcinema.vn</li>
                <li>Hotline: 1900 xxxx</li>
                <li>ABCD Mall, TP.HCM</li>
              </ul>
            </div>
          </div>
          <div className="mt-8 border-t border-gray-800 pt-8 text-center text-sm text-gray-500">
            © 2026 ABCD Cinema. Đặt vé online - Nhanh chóng và tiện lợi.
          </div>
        </div>
      </footer>
    </div>
  )
}
