using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;

public sealed class QuestPdfTicketPdfRenderer : ITicketPdfRenderer
{
    public byte[] Render(TicketDocumentModel model)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var qrSvg = BuildQrSvg(model.QrCodePayload);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(32);
                page.PageColor(Colors.Grey.Lighten5);
                page.DefaultTextStyle(text => text.FontFamily("Arial").FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Content().Element(content => ComposeContent(content, model, qrSvg));
            });
        }).GeneratePdf();
    }

    private static void ComposeContent(IContainer container, TicketDocumentModel model, string qrSvg)
    {
        container
            .Border(2)
            .BorderColor(Colors.Grey.Darken4)
            .Background(Colors.White)
            .Padding(24)
            .Column(column =>
            {
                column.Spacing(18);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(header =>
                    {
                        header.Item().Text("ABCD CINEMA")
                            .FontSize(22)
                            .Bold()
                            .FontColor(Colors.Red.Darken3);
                        header.Item().Text("Payment successful - your movie ticket is ready")
                            .FontSize(13)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(190).AlignRight().Column(meta =>
                    {
                        meta.Item().AlignRight().Text(model.BookingCode).FontSize(16).Bold();
                        meta.Item().AlignRight().Text("Booking code").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });

                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Column(left =>
                    {
                        left.Spacing(10);
                        left.Item().Text(model.MovieTitle).FontSize(24).Bold().FontColor(Colors.Grey.Darken4);
                        left.Item().Text($"{model.CinemaName} - {model.HallName}").FontSize(12);
                        left.Item().Text(model.ShowtimeText).FontSize(15).Bold().FontColor(Colors.Red.Darken3);

                        left.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            table.Header(header =>
                            {
                                HeaderCell(header.Cell(), "Seat");
                                HeaderCell(header.Cell(), "Ticket code");
                                HeaderCell(header.Cell(), "Description");
                            });

                            foreach (var seat in model.Seats)
                            {
                                BodyCell(table.Cell(), seat.SeatCode, true);
                                BodyCell(table.Cell(), seat.TicketCode, false);
                                BodyCell(table.Cell(), seat.Description, false);
                            }
                        });

                        if (model.Combos.Count > 0)
                        {
                            left.Item().PaddingTop(6).Text("Snack combos").Bold();
                            foreach (var combo in model.Combos)
                            {
                                left.Item().Text($"{combo.Description} x {combo.Quantity}");
                            }
                        }
                    });

                    row.ConstantItem(220).Column(right =>
                    {
                        right.Spacing(10);
                        right.Item().AlignCenter().Width(150).Height(150).Svg(qrSvg);
                        right.Item().AlignCenter().Text("Scan at counter").FontSize(10).FontColor(Colors.Grey.Darken1);

                        InfoLine(right, "Customer", model.CustomerName);
                        InfoLine(right, "Email", model.CustomerEmail);
                        InfoLine(right, "Payment", model.PaymentProvider);
                        InfoLine(right, "Paid at", model.PaymentTimeText);
                        InfoLine(right, "Total", model.TotalText, true);
                    });
                });

                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                column.Item().Text("Please arrive 15 minutes before showtime. Keep this PDF ready for ticket validation.")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });
    }

    private static void HeaderCell(IContainer container, string text)
    {
        container
            .Background(Colors.Grey.Darken4)
            .PaddingVertical(6)
            .PaddingHorizontal(8)
            .Text(text)
            .Bold()
            .FontColor(Colors.White);
    }

    private static void BodyCell(IContainer container, string text, bool emphasize)
    {
        var descriptor = container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(8)
            .Text(text);

        if (emphasize)
        {
            descriptor.Bold().FontColor(Colors.Red.Darken3);
        }
    }

    private static void InfoLine(ColumnDescriptor column, string label, string value, bool emphasize = false)
    {
        column.Item().Column(item =>
        {
            item.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
            var text = item.Item().Text(value).FontSize(emphasize ? 14 : 10);
            if (emphasize)
            {
                text.Bold().FontColor(Colors.Red.Darken3);
            }
        });
    }

    private static string BuildQrSvg(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new SvgQRCode(data);
        return qrCode.GetGraphic(6);
    }
}
