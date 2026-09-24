using BillingApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BillingApp.Documents
{
    public static class InvoiceDocument
    {
        public static byte[] Generate(Billing billing)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("BillingApp Pvt Ltd").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                                c.Item().Text("123 Business Street, Dhaka 1207").FontSize(9).FontColor(Colors.Grey.Darken1);
                                c.Item().Text("Phone: +880 1700-000000 | Email: info@billingapp.com").FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(170).AlignRight().Column(c =>
                            {
                                c.Item().Text("INVOICE").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                                c.Item().Text($"# {billing.InvoiceNo}").FontSize(10);
                                c.Item().Text($"Date: {billing.BillDate:dd MMM yyyy}").FontSize(10);
                                c.Item().Text($"Status: {billing.Status}").FontSize(10).Bold();
                            });
                        });
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Column(c =>
                        {
                            c.Item().Text("BILL TO").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(billing.Customer?.Name ?? "-").FontSize(12).Bold();
                            if (!string.IsNullOrEmpty(billing.Customer?.Email))
                                c.Item().Text(billing.Customer.Email).FontSize(10);
                            if (!string.IsNullOrEmpty(billing.Customer?.Phone))
                                c.Item().Text(billing.Customer.Phone).FontSize(10);
                            if (!string.IsNullOrEmpty(billing.Customer?.Address))
                                c.Item().Text(billing.Customer.Address).FontSize(10);
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(30);
                                cols.RelativeColumn(4);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                    .Text("#").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                    .Text("Product").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight()
                                    .Text("Unit Price").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter()
                                    .Text("Qty").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight()
                                    .Text("Total").FontColor(Colors.White).Bold();
                            });

                            int i = 1;
                            foreach (var item in billing.Items)
                            {
                                var bg = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                table.Cell().Background(bg).Padding(6).Text(i.ToString());
                                table.Cell().Background(bg).Padding(6).Text(item.Product?.Name ?? "-");
                                table.Cell().Background(bg).Padding(6).AlignRight().Text($"Rs. {item.UnitPrice:N2}");
                                table.Cell().Background(bg).Padding(6).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().Background(bg).Padding(6).AlignRight().Text($"Rs. {item.LineTotal:N2}");
                                i++;
                            }
                        });

                        col.Item().AlignRight().Width(250).Column(c =>
                        {
                            c.Spacing(4);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Subtotal").Bold();
                                r.ConstantItem(120).AlignRight().Text($"Rs. {billing.TotalAmount:N2}");
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Paid").Bold();
                                r.ConstantItem(120).AlignRight().Text($"Rs. {billing.PaidAmount:N2}");
                            });
                            c.Item().PaddingTop(4).BorderTop(1).BorderColor(Colors.Grey.Medium)
                                .PaddingTop(4).Row(r =>
                            {
                                r.RelativeItem().Text("Balance Due").Bold().FontColor(Colors.Red.Darken2);
                                r.ConstantItem(120).AlignRight().Text($"Rs. {billing.BalanceAmount:N2}")
                                    .Bold().FontColor(Colors.Red.Darken2);
                            });
                        });

                        if (billing.Payments.Any())
                        {
                            col.Item().PaddingTop(15).Text("Payment History").FontSize(11).Bold();
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(3);
                                    cols.RelativeColumn(2);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date").Bold();
                                    h.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Method").Bold();
                                    h.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Reference").Bold();
                                    h.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Amount").Bold();
                                });
                                foreach (var p in billing.Payments)
                                {
                                    table.Cell().Padding(5).Text(p.PaymentDate.ToString("dd MMM yyyy"));
                                    table.Cell().Padding(5).Text(p.Method);
                                    table.Cell().Padding(5).Text(p.Reference ?? "-");
                                    table.Cell().Padding(5).AlignRight().Text($"Rs. {p.Amount:N2}");
                                }
                            });
                        }
                    });

                    page.Footer().Column(c =>
                    {
                        c.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        c.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Thank you for your business!").FontSize(9).Italic();
                            r.ConstantItem(120).AlignRight().Text(t =>
                            {
                                t.Span("Page ").FontSize(9);
                                t.CurrentPageNumber().FontSize(9);
                                t.Span(" / ").FontSize(9);
                                t.TotalPages().FontSize(9);
                            });
                        });
                    });
                });
            });

            return doc.GeneratePdf();
        }
    }
}