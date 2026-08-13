export interface Cinema {
  id: string;
  name: string;
  address: string;
  showtimes: string[];
  hallTypes: string[];
}

export interface Movie {
  id: string;
  apiId?: string;
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

function expandMovies(source: Movie[], total: number, suffix: string): Movie[] {
  return Array.from({ length: total }, (_, index) => {
    const movie = source[index % source.length];
    return {
      ...movie,
      id: `${movie.id}-${suffix}-${index + 1}`,
    };
  });
}

export const cinemasList: Cinema[] = [
  {
    id: 'abcd-mall',
    name: 'ABCD Cinema - ABCD Mall',
    address: 'Level 5, ABCD Mall, 123 Le Van Viet Street, Ho Chi Minh City',
    showtimes: ['09:30', '12:00', '14:30', '17:00', '19:30', '22:00'],
    hallTypes: ['2D', '3D', 'IMAX'],
  },
  {
    id: 'quan-1',
    name: 'ABCD Cinema - District 1',
    address: '456 Nguyen Hue Boulevard, District 1, Ho Chi Minh City',
    showtimes: ['10:00', '13:15', '16:00', '19:00', '21:45'],
    hallTypes: ['2D', '3D'],
  },
  {
    id: 'binh-duong',
    name: 'ABCD Cinema - Binh Duong',
    address: 'Level 3, Aeon Mall Binh Duong, Thuan An, Binh Duong',
    showtimes: ['08:45', '11:30', '14:00', '16:45', '19:15', '21:30'],
    hallTypes: ['2D', '4DX'],
  },
  {
    id: 'go-vap',
    name: 'ABCD Cinema - Go Vap',
    address: '789 Phan Van Tri Street, Go Vap, Ho Chi Minh City',
    showtimes: ['10:30', '13:00', '15:30', '18:00', '20:30'],
    hallTypes: ['2D', '3D'],
  },
];

const nowShowingSeed: Movie[] = [
  {
    id: 'cosmic-odyssey',
    title: 'Cosmic Odyssey',
    description:
      'A daring crew of astronauts embarks on a breathtaking mission beyond the Milky Way, where black holes, alien ecosystems, and impossible choices test the limits of human courage.',
    genre: 'Sci-Fi, Adventure',
    rating: 8.5,
    duration: '148 min',
    director: 'Christopher Wright',
    cast: ['Tom Holland', 'Zoe Saldana', 'Oscar Isaac'],
    language: 'English with Vietnamese subtitles',
    ageRating: 'T13',
    imageUrl:
      'https://images.unsplash.com/photo-1767048264833-5b65aacd1039?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzY2ktZmklMjBtb3ZpZSUyMGF0bW9zcGhlcmV8ZW58MXx8fHwxNzc0Njg5MzQ5fDA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: cinemasList,
  },
  {
    id: 'the-old-building-secret',
    title: 'The Old Building Secret',
    description:
      'A young team of investigators explores an abandoned downtown tower and uncovers a maze of terrifying secrets, hidden floors, and a presence that has been waiting in the dark.',
    genre: 'Horror, Thriller',
    rating: 7.8,
    duration: '112 min',
    director: 'James Wan Jr.',
    cast: ['Florence Pugh', "Jacob Elordi", "Lupita Nyong'o"],
    language: 'English with Vietnamese subtitles',
    ageRating: 'T18',
    imageUrl:
      'https://images.unsplash.com/photo-1595171694538-beb81da39d3e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx0aHJpbGxlciUyMG1vdmllJTIwZGFya3xlbnwxfHx8fDE3NzQ1NzkyNTB8MA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: [cinemasList[0], cinemasList[1], cinemasList[3]],
  },
  {
    id: 'impossible-target',
    title: 'Impossible Target',
    description:
      'Elite agent Ethan Cross has 48 hours to stop a global conspiracy armed with catastrophic technology. With every lead compromised, one mistake could change the world forever.',
    genre: 'Action, Adventure',
    rating: 8.9,
    duration: '135 min',
    director: 'David Leitch',
    cast: ['Tom Cruise', 'Rebecca Ferguson', 'Henry Cavill'],
    language: 'English with Vietnamese subtitles',
    ageRating: 'T16',
    imageUrl:
      'https://images.unsplash.com/photo-1765510296004-614b6cc204da?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxhY3Rpb24lMjBtb3ZpZSUyMHBvc3RlcnxlbnwxfHx8fDE3NzQ1NjY1NjF8MA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: cinemasList,
  },
  {
    id: 'sunset-serenade',
    title: 'Sunset Serenade',
    description:
      'During a quiet getaway in Da Lat, two lonely hearts meet by the lake at sunset and discover a tender romance shaped by modern pressures, missed chances, and the courage to be honest.',
    genre: 'Romance, Drama',
    rating: 7.5,
    duration: '105 min',
    director: 'Nguyen Minh Chau',
    cast: ['Kaity Nguyen', 'Isaac', 'Nhung Kate'],
    language: 'Vietnamese',
    ageRating: 'P',
    imageUrl:
      'https://images.unsplash.com/photo-1759643509991-0b0ec261e395?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxyb21hbmNlJTIwbW92aWUlMjBzdW5zZXR8ZW58MXx8fHwxNzc0Njg5MzUwfDA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: [cinemasList[0], cinemasList[2]],
  },
  {
    id: 'cuoc-chien-ngam',
    title: 'Cuoc Chien Ngam',
    description:
      'Detective Marcus Black takes a murder case that pulls him into a covert war between global crime syndicates and the institution that once trained him.',
    genre: 'Action, Crime',
    rating: 8.1,
    duration: '128 min',
    director: 'Matt Reeves',
    cast: ['Ryan Gosling', 'Ana de Armas', 'John David Washington'],
    language: 'English with Vietnamese subtitles',
    ageRating: 'T16',
    imageUrl:
      'https://images.unsplash.com/photo-1765510296004-614b6cc204da?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxhY3Rpb24lMjBtb3ZpZSUyMHBvc3RlcnxlbnwxfHx8fDE3NzQ1NjY1NjF8MA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: [cinemasList[0], cinemasList[1], cinemasList[3]],
  },
  {
    id: 'ngu-hanh-son',
    title: 'Ngu Hanh Son',
    description:
      'Five young guardians inherit the power of sacred stones and must learn to wield them before an ancient darkness returns.',
    genre: 'Fantasy, Action',
    rating: 7.9,
    duration: '143 min',
    director: 'Ly Hai',
    cast: ['Tran Thanh', 'Ngo Thanh Van', 'Kieu Minh Tuan'],
    language: 'Vietnamese',
    ageRating: 'T13',
    imageUrl:
      'https://images.unsplash.com/photo-1761948245703-cbf27a3e7502?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxmYW50YXN5JTIwYWR2ZW50dXJlJTIwbW92aWV8ZW58MXx8fHwxNzc0NTk4NzcwfDA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: [cinemasList[0], cinemasList[2], cinemasList[3]],
  },
];

const comingSoonSeed: Movie[] = [
  {
    id: 'kingdom-of-legends',
    title: 'Kingdom of Legends',
    description:
      'A young ruler begins a mythical quest to lift an ancient curse and save his kingdom, traveling through enchanted lands filled with monsters, magic, and a legendary blade.',
    genre: 'Fantasy, Adventure',
    rating: 8.2,
    duration: '156 min',
    releaseDate: '15/04/2026',
    director: 'Peter Jackson Jr.',
    cast: ['Timothee Chalamet', 'Anya Taylor-Joy', 'Idris Elba'],
    language: 'English with Vietnamese subtitles',
    ageRating: 'T13',
    isComingSoon: true,
    imageUrl:
      'https://images.unsplash.com/photo-1761948245703-cbf27a3e7502?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxmYW50YXN5JTIwYWR2ZW50dXJlJTIwbW92aWV8ZW58MXx8fHwxNzc0NTk4NzcwfDA&ixlib=rb-4.1.0&q=80&w=1080',
  },
  {
    id: 'midnight-nightmare',
    title: 'Midnight Nightmare',
    description:
      'When nightmares begin to bleed into reality, a group of students discovers that something from the shadows has marked them, and the line between dream and death starts to vanish.',
    genre: 'Horror, Mystery',
    rating: 7.9,
    duration: '118 min',
    releaseDate: '22/04/2026',
    director: 'Mike Flanagan',
    cast: ['Sydney Sweeney', 'Barry Keoghan', 'Cate Blanchett'],
    language: 'English with Vietnamese subtitles',
    ageRating: 'T18',
    isComingSoon: true,
    imageUrl:
      'https://images.unsplash.com/photo-1699631596984-cfb063c5d968?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxob3Jyb3IlMjBtb3ZpZSUyMGNyZWVweXxlbnwxfHx8fDE3NzQ2ODkzNDl8MA&ixlib=rb-4.1.0&q=80&w=1080',
  },
  {
    id: 'unexpected-happiness',
    title: 'Unexpected Happiness',
    description:
      'A chaotic family vacation in a coastal village turns into a heartfelt summer comedy, where old misunderstandings, warm reunions, and small joys reveal what happiness really means.',
    genre: 'Comedy, Family',
    rating: 7.3,
    duration: '98 min',
    releaseDate: '05/05/2026',
    director: 'Tran Thanh',
    cast: ['Kaity Nguyen', 'Tran Thanh', 'Tuan Tran'],
    language: 'Vietnamese',
    ageRating: 'P',
    isComingSoon: true,
    imageUrl:
      'https://images.unsplash.com/photo-1758525862263-af89b090fb56?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjb21lZHklMjBtb3ZpZSUyMGhhcHB5fGVufDF8fHx8MTc3NDY4OTM1MHww&ixlib=rb-4.1.0&q=80&w=1080',
  },
];

export const nowShowingMovies: Movie[] = expandMovies(nowShowingSeed, 6, 'now');

export const comingSoonMovies: Movie[] = expandMovies(comingSoonSeed, 6, 'soon');

const scheduleDetailMovies = nowShowingSeed.filter((movie) =>
  movie.id === 'cuoc-chien-ngam' || movie.id === 'ngu-hanh-son',
);

export const allMovies: Movie[] = [...nowShowingMovies, ...comingSoonMovies, ...scheduleDetailMovies];

export function getMovieById(id: string): Movie | undefined {
  return allMovies.find((m) => m.id === id);
}
