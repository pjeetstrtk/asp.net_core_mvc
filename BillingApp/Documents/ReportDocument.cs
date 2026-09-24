using BillingApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BillingApp.Documents
{
    public static class ReportDocument
    {
        public static byte[] Generate(List<Billing> billings, DateTime from, DateTime to)
        {
            decimal totalBilled = billings.Sum(b => b.TotalAmount);
            decimal totalPaid = billings.Sum(b => b.PaidAmount);
            decimal totalBalance = billings.Sum(b => b.BalanceAmount);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Sales Report").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Period: {from:dd MMM yyyy} to {to:dd MMM yyyy}")
                            .FontSize(11).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text("Total Billed").FontSize(9).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"Rs. {totalBilled:N2}").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text("Total Paid").FontSize(9).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"Rs. {totalPaid:N2}").FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Background(Colors.Red.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text("Balance").FontSize(9).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"Rs. {totalBalance:N2}").FontSize(14).Bold().FontColor(Colors.Red.Darken2);
                            });
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(1.5f);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Invoice").FontColor(Colors.White).Bold();
                                h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Date").FontColor(Colors.White).Bold();
                                h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Customer").FontColor(Colors.White).Bold();
                                h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                                h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight().Text("Paid").FontColor(Colors.White).Bold();
                                h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight().Text("Balance").FontColor(Colors.White).Bold();
                                h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter().Text("Status").FontColor(Colors.White).Bold();
                            });

                            int i = 0;
                            foreach (var b in billings)
                            {
                                var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                table.Cell().Background(bg).Padding(5).Text(b.InvoiceNo);
                                table.Cell().Background(bg).Padding(5).Text(b.BillDate.ToString("dd MMM yyyy"));
                                table.Cell().Background(bg).Padding(5).Text(b.Customer?.Name ?? "-");
                                table.Cell().Background(bg).Padding(5).AlignRight().Text($"Rs. {b.TotalAmount:N2}");
                                table.Cell().Background(bg).Padding(5).AlignRight().Text($"Rs. {b.PaidAmount:N2}");
                                table.Cell().Background(bg).Padding(5).AlignRight().Text($"Rs. {b.BalanceAmount:N2}");
                                table.Cell().Background(bg).Padding(5).AlignCenter().Text(b.Status);
                                i++;
                            }
                        });

                        col.Item().PaddingTop(10).AlignRight().Text($"Total records: {billings.Count}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    page.Footer().Row(r =>
                    {
                        r.RelativeItem().Text($"Generated on {DateTime.Now:dd MMM yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        r.ConstantItem(150).AlignRight().Text(t =>
                        {
                            t.Span("Page ").FontSize(9);
                            t.CurrentPageNumber().FontSize(9);
                            t.Span(" / ").FontSize(9);
                            t.TotalPages().FontSize(9);
                        });
                    });
                });
            });

            return doc.GeneratePdf();
        }
    }
}