namespace ABCDMall.Modules.Movies.Domain.Entities;

public class Person
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? Biography { get; set; }

    public ICollection<MovieCredit> MovieCredits { get; set; } = new List<MovieCredit>();
}
