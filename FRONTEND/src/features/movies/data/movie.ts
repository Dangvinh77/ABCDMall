export interface Cinema {
  id: string;
  name: string;
  address: string;
  showtimes: string[];
  hallTypes: string[];
}

export interface Movie {
  id: string;
  title: string;
  description: string;
  genre: string;
  rating: number;
  duration: string;
  director: string;
  cast: string[];
  language: string;
  releaseDate?: string;
  imageUrl: string;
  backdropUrl?: string;
  isComingSoon?: boolean;
  ageRating: string;
  cinemas?: Cinema[];
}

export const cinemasList: Cinema[] = [
  {
    id: 'abcd-mall',
    name: 'ABCD Cinema - ABCD Mall',
    address: 'Tầng 5, ABCD Mall, 123 Đường Lê Văn Việt, TP. HCM',
    showtimes: ['09:30', '12:00', '14:30', '17:00', '19:30', '22:00'],
    hallTypes: ['2D', '3D', 'IMAX'],
  },
  {
    id: 'quan-1',
    name: 'ABCD Cinema - Quận 1',
    address: '456 Nguyễn Huệ, Quận 1, TP. HCM',
    showtimes: ['10:00', '13:15', '16:00', '19:00', '21:45'],
    hallTypes: ['2D', '3D'],
  },
  {
    id: 'binh-duong',
    name: 'ABCD Cinema - Bình Dương',
    address: 'Tầng 3, Aeon Mall Bình Dương, Thuận An, Bình Dương',
    showtimes: ['08:45', '11:30', '14:00', '16:45', '19:15', '21:30'],
    hallTypes: ['2D', '4DX'],
  },
  {
    id: 'go-vap',
    name: 'ABCD Cinema - Gò Vấp',
    address: '789 Phan Văn Trị, Gò Vấp, TP. HCM',
    showtimes: ['10:30', '13:00', '15:30', '18:00', '20:30'],
    hallTypes: ['2D', '3D'],
  },
];

export const nowShowingMovies: Movie[] = [
  {
    id: 'hanh-trinh-vu-tru',
    title: 'Hành Trình Vũ Trụ',
    description:
      'Một cuộc phiêu lưu sử thi vào vũ trụ bao la khi đội phi hành gia dũng cảm nhận nhiệm vụ khám phá một hành tinh bí ẩn ngoài dải Ngân Hà. Họ phải đối mặt với những thử thách chưa từng có trong lịch sử loài người – từ lỗ đen hút thời gian đến những sinh vật ngoài hành tinh kỳ lạ.',
    genre: 'Sci-Fi, Phiêu lưu',
    rating: 8.5,
    duration: '148 phút',
    director: 'Christopher Wright',
    cast: ['Tom Holland', 'Zoe Saldana', 'Oscar Isaac'],
    language: 'Tiếng Anh (Phụ đề Việt)',
    ageRating: 'T13',
    imageUrl:
      'https://images.unsplash.com/photo-1767048264833-5b65aacd1039?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzY2ktZmklMjBtb3ZpZSUyMGF0bW9zcGhlcmV8ZW58MXx8fHwxNzc0Njg5MzQ5fDA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: cinemasList,
  },
  {
    id: 'bi-an-toa-nha-cu',
    title: 'Bí Ẩn Tòa Nhà Cũ',
    description:
      'Khi nhóm thám tử trẻ quyết định điều tra tòa nhà bỏ hoang 30 năm tại trung tâm thành phố, họ không ngờ mình đang bước vào một mê cung kinh hoàng đầy bí mật và ám ảnh. Mỗi tầng lầu ẩn chứa một sự thật đáng sợ chờ được khám phá.',
    genre: 'Kinh dị, Thriller',
    rating: 7.8,
    duration: '112 phút',
    director: 'James Wan Jr.',
    cast: ['Florence Pugh', 'Jacob Elordi', 'Lupita Nyong\'o'],
    language: 'Tiếng Anh (Phụ đề Việt)',
    ageRating: 'T18',
    imageUrl:
      'https://images.unsplash.com/photo-1595171694538-beb81da39d3e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx0aHJpbGxlciUyMG1vdmllJTIwZGFya3xlbnwxfHx8fDE3NzQ1NzkyNTB8MA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: [cinemasList[0], cinemasList[1], cinemasList[3]],
  },
  {
    id: 'nhiem-vu-bat-kha-thi',
    title: 'Nhiệm Vụ Bất Khả Thi',
    description:
      'Điệp viên siêu đẳng Ethan Cross nhận nhiệm vụ nguy hiểm nhất trong sự nghiệp – phá giải âm mưu khủng bố quy mô toàn cầu trong vòng 48 giờ. Một mình đối đầu với tổ chức bí ẩn có vũ khí hủy diệt hàng loạt, anh không có chỗ để sai lầm.',
    genre: 'Hành động, Phiêu lưu',
    rating: 8.9,
    duration: '135 phút',
    director: 'David Leitch',
    cast: ['Tom Cruise', 'Rebecca Ferguson', 'Henry Cavill'],
    language: 'Tiếng Anh (Phụ đề Việt)',
    ageRating: 'T16',
    imageUrl:
      'https://images.unsplash.com/photo-1765510296004-614b6cc204da?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxhY3Rpb24lMjBtb3ZpZSUyMHBvc3RlcnxlbnwxfHx8fDE3NzQ1NjY1NjF8MA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: cinemasList,
  },
  {
    id: 'chuyen-tinh-hoang-hon',
    title: 'Chuyện Tình Hoàng Hôn',
    description:
      'Trong chuyến du lịch Đà Lạt, hai tâm hồn cô đơn tình cờ gặp nhau bên bờ hồ lúc hoàng hôn. Một câu chuyện tình yêu đẹp đẽ, lãng mạn nhưng đầy trắc trở giữa nhịp sống hối hả của hiện đại và khát vọng sống thật với trái tim mình.',
    genre: 'Tình cảm, Drama',
    rating: 7.5,
    duration: '105 phút',
    director: 'Nguyễn Minh Châu',
    cast: ['Kaity Nguyễn', 'Isaac', 'Nhung Kate'],
    language: 'Tiếng Việt',
    ageRating: 'P',
    imageUrl:
      'https://images.unsplash.com/photo-1759643509991-0b0ec261e395?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxyb21hbmNlJTIwbW92aWUlMjBzdW5zZXR8ZW58MXx8fHwxNzc0Njg5MzUwfDA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: [cinemasList[0], cinemasList[2]],
  },
];

export const comingSoonMovies: Movie[] = [
  {
    id: 'vuong-quoc-than-thoai',
    title: 'Vương Quốc Thần Thoại',
    description:
      'Một vị vua trẻ phải bước vào hành trình huyền bí để giải cứu vương quốc khỏi lời nguyền cổ xưa. Cùng nhóm dũng sĩ đa tài, anh đi qua những vùng đất thần thoại đầy quái vật và phép thuật để tìm lại thanh kiếm linh thiêng.',
    genre: 'Fantasy, Phiêu lưu',
    rating: 8.2,
    duration: '156 phút',
    releaseDate: '15/04/2026',
    director: 'Peter Jackson Jr.',
    cast: ['Timothée Chalamet', 'Anya Taylor-Joy', 'Idris Elba'],
    language: 'Tiếng Anh (Phụ đề Việt)',
    ageRating: 'T13',
    isComingSoon: true,
    imageUrl:
      'https://images.unsplash.com/photo-1761948245703-cbf27a3e7502?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxmYW50YXN5JTIwYWR2ZW50dXJlJTIwbW92aWV8ZW58MXx8fHwxNzc0NTk4NzcwfDA&ixlib=rb-4.1.0&q=80&w=1080',
  },
  {
    id: 'ac-mong-dem-khuya',
    title: 'Ác Mộng Đêm Khuya',
    description:
      'Khi những giấc mơ trở thành hiện thực đáng sợ, một nhóm bạn sinh viên phát hiện rằng có thứ gì đó đang theo dõi họ từ thế giới bóng tối. Ranh giới giữa thực và ảo dần tan biến, và cái chết rình rập ở khắp nơi.',
    genre: 'Kinh dị, Bí ẩn',
    rating: 7.9,
    duration: '118 phút',
    releaseDate: '22/04/2026',
    director: 'Mike Flanagan',
    cast: ['Sydney Sweeney', 'Barry Keoghan', 'Cate Blanchett'],
    language: 'Tiếng Anh (Phụ đề Việt)',
    ageRating: 'T18',
    isComingSoon: true,
    imageUrl:
      'https://images.unsplash.com/photo-1699631596984-cfb063c5d968?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxob3Jyb3IlMjBtb3ZpZSUyMGNyZWVweXxlbnwxfHx8fDE3NzQ2ODkzNDl8MA&ixlib=rb-4.1.0&q=80&w=1080',
  },
  {
    id: 'hanh-phuc-bat-ngo',
    title: 'Hạnh Phúc Bất Ngờ',
    description:
      'Một gia đình hỗn loạn vui nhộn cùng nhau trải qua kỳ nghỉ hè đầy biến cố tại một ngôi làng nhỏ miền Trung. Từ những hiểu lầm buồn cười đến những khoảnh khắc ấm lòng, họ nhận ra rằng hạnh phúc thật ra rất đơn giản.',
    genre: 'Hài kịch, Gia đình',
    rating: 7.3,
    duration: '98 phút',
    releaseDate: '05/05/2026',
    director: 'Trấn Thành',
    cast: ['Kaity Nguyễn', 'Trấn Thành', 'Tuấn Trần'],
    language: 'Tiếng Việt',
    ageRating: 'P',
    isComingSoon: true,
    imageUrl:
      'https://images.unsplash.com/photo-1758525862263-af89b090fb56?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjb21lZHklMjBtb3ZpZSUyMGhhcHB5fGVufDF8fHx8MTc3NDY4OTM1MHww&ixlib=rb-4.1.0&q=80&w=1080',
  },
];

export const allMovies: Movie[] = [...nowShowingMovies, ...comingSoonMovies];

export function getMovieById(id: string): Movie | undefined {
  return allMovies.find((m) => m.id === id);
}
