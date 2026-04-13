namespace ABCDMall.Modules.Movies.Domain.Entities;

public class MovieCredit
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid PersonId { get; set; }
    public string CreditType { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Movie? Movie { get; set; }
    public Person? Person { get; set; }
}
