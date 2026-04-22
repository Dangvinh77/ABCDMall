namespace ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;

public interface ITicketPdfRenderer
{
    byte[] Render(TicketDocumentModel model);
}
