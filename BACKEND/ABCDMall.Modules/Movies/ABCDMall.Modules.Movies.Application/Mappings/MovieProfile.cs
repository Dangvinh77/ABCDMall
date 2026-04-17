using ABCDMall.Modules.Movies.Application.Contracts;
using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Mappings;

public sealed class MovieProfile : AutoMapper.Profile
{
    public MovieProfile()
    {
        CreateMap<Movie, MovieListItemResponseDto>()
            .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src =>
                src.MovieGenres
                    .Where(movieGenre => movieGenre.Genre != null)
                    .Select(movieGenre => movieGenre.Genre!.Name)));

        CreateMap<Movie, MovieDetailResponseDto>()
            .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src =>
                src.MovieGenres
                    .Where(movieGenre => movieGenre.Genre != null)
                    .Select(movieGenre => movieGenre.Genre!.Name)));

        CreateMap<Showtime, ShowtimeResponseDto>()
            .ForMember(dest => dest.ShowtimeId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.HallType, opt => opt.MapFrom(src => src.Hall != null
                ? MoviesContractValueMapper.ToContractValue(src.Hall.HallType)
                : string.Empty))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => MoviesContractValueMapper.ToContractValue(src.Language)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
