using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using ACGCET_Admin.ViewModels.EntryReport;
using ACGCET_Admin.ViewModels.Application;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ACGCET_Admin.Services
{
    public class PrintService
    {
        public PrintService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ── Logo Helpers ────────────────────────────────────────────────────
        private static byte[] LoadImageFromFile(string relativePath)
        {
            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var fullPath = Path.Combine(basePath, relativePath);
                if (File.Exists(fullPath))
                    return File.ReadAllBytes(fullPath);
            }
            catch { }
            return Array.Empty<byte>();
        }

        private byte[] GetAcgcetLogo() =>
            LoadImageFromFile(Path.Combine("Resources", "Images", "acgcet_logo.png"));

        private byte[] GetTNLogo() =>
            LoadImageFromFile(Path.Combine("Resources", "Images", "accet_logo_1.png"));

        // ── Header ──────────────────────────────────────────────────────────
        private void ComposeHeader(IContainer container, string reportTitle)
        {
            container.Column(col =>
            {
                // Row 1: ACGCET logo | College info | TN logo
                col.Item().Row(row =>
                {
                    // ACGCET logo — left
                    var acgcetLogo = GetAcgcetLogo();
                    if (acgcetLogo.Length > 0)
                        row.ConstantItem(72).Image(acgcetLogo).FitArea();
                    else
                        row.ConstantItem(72).AlignMiddle().AlignCenter()
                           .Text("ACGCET").FontSize(9).Bold().FontColor(Colors.Grey.Medium);

                    // College name — center
                    row.RelativeItem().PaddingHorizontal(8).AlignMiddle().Column(info =>
                    {
                        info.Item().AlignCenter()
                            .Text("Alagappa Chettiar Government College of")
                            .FontSize(14).Bold();
                        info.Item().AlignCenter()
                            .Text("Engineering and Technology (Autonomous)")
                            .FontSize(14).Bold();
                        info.Item().AlignCenter()
                            .Text("Karaikudi - 630 003")
                            .FontSize(12).Bold();
                        info.Item().AlignCenter()
                            .Text("Office of the Controller of Examinations")
                            .FontSize(12).Bold();
                    });

                    // TN logo — right
                    var tnLogo = GetTNLogo();
                    if (tnLogo.Length > 0)
                        row.ConstantItem(72).Image(tnLogo).FitArea();
                    else
                        row.ConstantItem(72).AlignMiddle().AlignCenter()
                           .Text("TN GOVT").FontSize(9).Bold().FontColor(Colors.Grey.Medium);
                });

                // Divider
                col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);

                // Report Title
                if (!string.IsNullOrWhiteSpace(reportTitle))
                {
                    col.Item().AlignCenter().PaddingBottom(4)
                       .Text(reportTitle).FontSize(13).Bold().Underline();
                }
            });
        }

        private void ComposeMetadata(IContainer container, string degree, string program, string batch, string section)
        {
            container.PaddingVertical(8).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Degree: {degree}").FontSize(11).Bold();
                    row.RelativeItem().Text($"Programme: {program}").FontSize(11).Bold();
                });
                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text($"Batch: {batch}").FontSize(11).Bold();
                    row.RelativeItem().Text($"Section: {section}").FontSize(11).Bold();
                });
            });
        }

        private string GenerateAndOpenPdf(Action<IDocumentContainer> buildDocument, string fileTitle)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{fileTitle}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            Document.Create(buildDocument).GeneratePdf(filePath);
            try
            {
                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open PDF automatically.\nFile saved at:\n{filePath}\nError: {ex.Message}",
                                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return filePath;
        }

        // ── Table Cell Helpers ───────────────────────────────────────────────
        private static IContainer HeaderCell(IContainer c) =>
            c.Background(Color.FromHex("#4A148C"))
             .Border(0.5f).BorderColor(Colors.White)
             .Padding(5);

        private static IContainer DataCell(IContainer c, bool isAlt) =>
            c.Background(isAlt ? Color.FromHex("#F3E5F5") : Colors.White)
             .Border(0.5f).BorderColor(Color.FromHex("#E0E0E0"))
             .Padding(4);

        // ── Reports ─────────────────────────────────────────────────────────
        public void GenerateStudentReport(
            IEnumerable<StudentEntryItem> data,
            string degree, string program, string batch, string section,
            string reportTitle = "Student Entry Report")
        {
            GenerateAndOpenPdf(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36, Unit.Point);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        ComposeHeader(col.Item(), reportTitle);
                        ComposeMetadata(col.Item(), degree, program, batch, section);
                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(32);  // SNo
                            cols.RelativeColumn(2);   // Admission No
                            cols.RelativeColumn(2);   // Roll No
                            cols.RelativeColumn(3);   // Reg No
                            cols.RelativeColumn(5);   // Name
                            cols.RelativeColumn(2);   // Regulation
                        });

                        table.Header(header =>
                        {
                            foreach (var h in new[] { "S.No", "Adm. No", "Roll No", "Reg. No", "Student Name", "Regulation" })
                                header.Cell().Element(c => HeaderCell(c))
                                      .Text(h).FontColor(Colors.White).Bold().FontSize(10);
                        });

                        int sno = 1;
                        foreach (var item in data)
                        {
                            bool alt = sno % 2 == 0;
                            table.Cell().Element(c => DataCell(c, alt)).AlignRight().Text(sno.ToString());
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.AdmissionNo ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.RollNo ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.RegNo ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.StudentName ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.RegulationName ?? "-");
                            sno++;
                        }
                    });

                    ComposeFooter(page.Footer());
                });
            }, "StudentReport");
        }

        public void GenerateInternalMarkReport(
            IEnumerable<InternalMarkEntryItem> data,
            string degree, string program, string batch, string section,
            string reportTitle = "Internal Mark Entry Report")
        {
            GenerateAndOpenPdf(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36, Unit.Point);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        ComposeHeader(col.Item(), reportTitle);
                        ComposeMetadata(col.Item(), degree, program, batch, section);
                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(32); // SNo
                            cols.RelativeColumn(3);  // Reg
                            cols.RelativeColumn(5);  // Name
                            cols.RelativeColumn(2);  // Paper
                            cols.RelativeColumn(2);  // Test
                            cols.RelativeColumn(2);  // Mark
                        });

                        table.Header(header =>
                        {
                            foreach (var h in new[] { "S.No", "Reg. No", "Student Name", "Paper Code", "Test", "Mark" })
                                header.Cell().Element(c => HeaderCell(c))
                                      .Text(h).FontColor(Colors.White).Bold().FontSize(10);
                        });

                        int sno = 1;
                        foreach (var item in data)
                        {
                            bool alt = sno % 2 == 0;
                            table.Cell().Element(c => DataCell(c, alt)).AlignRight().Text(sno.ToString());
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.RegNo ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.StudentName ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.PaperCode ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.Test ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).AlignRight().Text(item.Mark ?? "-");
                            sno++;
                        }
                    });

                    ComposeFooter(page.Footer());
                });
            }, "InternalMarkReport");
        }

        public void GenerateResultReport(
            IEnumerable<ResultEntryItem> data,
            string degree, string program, string batch, string section,
            string reportTitle = "Result Entry Report")
        {
            GenerateAndOpenPdf(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36, Unit.Point);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        ComposeHeader(col.Item(), reportTitle);
                        ComposeMetadata(col.Item(), degree, program, batch, section);
                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(32); // SNo
                            cols.RelativeColumn(3);  // Reg
                            cols.RelativeColumn(5);  // Name
                            cols.RelativeColumn(3);  // Paper
                            cols.RelativeColumn(2);  // Grade
                        });

                        table.Header(header =>
                        {
                            foreach (var h in new[] { "S.No", "Reg. No", "Student Name", "Paper Code", "Grade" })
                                header.Cell().Element(c => HeaderCell(c))
                                      .Text(h).FontColor(Colors.White).Bold().FontSize(10);
                        });

                        int sno = 1;
                        foreach (var item in data)
                        {
                            bool alt = sno % 2 == 0;
                            table.Cell().Element(c => DataCell(c, alt)).AlignRight().Text(sno.ToString());
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.RegNo ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.StudentName ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.PaperCode ?? "-");
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.Grade ?? "-");
                            sno++;
                        }
                    });

                    ComposeFooter(page.Footer());
                });
            }, "ResultReport");
        }

        public void GenerateSubjectReport(
            IEnumerable<SubjectReportItem> data,
            string regulation, string program,
            string reportTitle = "Class Wise Subject Code Report")
        {
            GenerateAndOpenPdf(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(28, Unit.Point);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        ComposeHeader(col.Item(), reportTitle);
                        col.Item().PaddingVertical(6).Row(row =>
                        {
                            row.RelativeItem().Text($"Regulation: {regulation}").FontSize(11).Bold();
                            row.RelativeItem().Text($"Program: {program}").FontSize(11).Bold();
                        });
                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(28); // Sem
                            cols.RelativeColumn(2);  // Code
                            cols.RelativeColumn(6);  // Name
                            cols.ConstantColumn(30); // Crd
                            cols.ConstantColumn(28); // Int
                            cols.ConstantColumn(28); // Ext
                            cols.ConstantColumn(28); // Tot
                            cols.ConstantColumn(28); // P.I
                            cols.ConstantColumn(28); // P.E
                            cols.ConstantColumn(28); // P.T
                            cols.RelativeColumn(1);  // Type
                        });

                        table.Header(header =>
                        {
                            foreach (var h in new[] { "Sem", "Code", "Paper Name", "Crd", "Int", "Ext", "Tot", "P.I", "P.E", "P.T", "Type" })
                                header.Cell().Element(c => HeaderCell(c))
                                      .Text(h).FontColor(Colors.White).Bold().FontSize(9);
                        });

                        int sno = 1;
                        foreach (var item in data)
                        {
                            bool alt = sno % 2 == 0;
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.Semester);
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.PaperCode);
                            table.Cell().Element(c => DataCell(c, alt)).Text(item.PaperName);
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.Credits);
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.IntMax);
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.ExtMax);
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.TotalMax);
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.IntMin);
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.ExtMin);
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.TotalMin);
                            table.Cell().Element(c => DataCell(c, alt)).AlignCenter().Text(item.Type);
                            sno++;
                        }
                    });

                    ComposeFooter(page.Footer());
                });
            }, "SubjectReport");
        }

        public void GenerateDynamicReport<T>(
            IEnumerable<T> data,
            List<PrintColumnDefinition> columns,
            string degree, string program, string batch, string section,
            string reportTitle = "Class Wise Student Report")
        {
            GenerateAndOpenPdf(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(28, Unit.Point);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        ComposeHeader(col.Item(), reportTitle);
                        ComposeMetadata(col.Item(), degree, program, batch, section);
                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(colDef =>
                        {
                            foreach (var col in columns)
                            {
                                if (col.Width < 50) colDef.ConstantColumn((float)col.Width);
                                else colDef.RelativeColumn((float)(col.Width / 50.0));
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var col in columns)
                                header.Cell().Element(c => HeaderCell(c))
                                      .Text(col.Header).FontColor(Colors.White).Bold().FontSize(10);
                        });

                        int sno = 1;
                        var properties = typeof(T).GetProperties();
                        foreach (var row in data)
                        {
                            bool alt = sno % 2 == 0;
                            foreach (var col in columns)
                            {
                                string text = "-";
                                if (col.Header == "SNo" || col.Header == "S.No")
                                {
                                    text = sno.ToString();
                                }
                                else
                                {
                                    var prop = properties.FirstOrDefault(p => p.Name == col.BindingPath);
                                    if (prop != null)
                                        text = prop.GetValue(row)?.ToString() ?? "-";
                                }
                                table.Cell().Element(c => DataCell(c, alt)).Text(text);
                            }
                            sno++;
                        }
                    });

                    ComposeFooter(page.Footer());
                });
            }, "DynamicReport");
        }

        private static void ComposeFooter(IContainer footer)
        {
            footer.PaddingTop(4).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2)
                  .Row(row =>
                  {
                      row.RelativeItem().AlignLeft()
                         .Text("Alagappa Chettiar Government College of Engineering and Technology")
                         .FontSize(8).FontColor(Colors.Grey.Medium);
                      row.AutoItem().AlignRight().Text(x =>
                      {
                          x.Span("Printed: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "  |  Page ")
                           .FontSize(8).FontColor(Colors.Grey.Medium);
                          x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                          x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                          x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                      });
                  });
        }
    }

    public class PrintColumnDefinition
    {
        public string Header      { get; set; } = string.Empty;
        public string BindingPath { get; set; } = string.Empty;
        public double Width       { get; set; } = 100;
    }
}
