using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using ACGCET_Admin.ViewModels.Application;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Linq;

namespace ACGCET_Admin.Views.Application
{
    public partial class ClassInternalMarkReportView : UserControl
    {
        public ClassInternalMarkReportView()
        {
            InitializeComponent();
            
            this.Loaded += (s, e) => 
            {
                WeakReferenceMessenger.Default.Register<PrintReportMessage>(this, OnPrintRequest);
            };
            
            this.Unloaded += (s, e) => 
            {
                WeakReferenceMessenger.Default.Unregister<PrintReportMessage>(this);
            };
        }

        private void OnPrintRequest(object recipient, PrintReportMessage message)
        {
            try
            {
                // Generate FlowDocument
                var doc = CreateFlowDocument(message);

                if (message.IsPreview)
                {
                    // Show Preview Window
                    var window = new Window
                    {
                        Title = "Print Preview - " + message.Title,
                        Width = 800,
                        Height = 600,
                        Content = new FlowDocumentScrollViewer { Document = doc },
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    window.ShowDialog(); // Use ShowDialog to block and keep context
                }
                else
                {
                    // Print
                    var printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        IDocumentPaginatorSource idp = doc;
                        printDialog.PrintDocument(idp.DocumentPaginator, message.Title);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error processing print request: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateFlowDocument(PrintReportMessage message)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Arial"),
                FontSize = 12
            };

            // Header
            var header = new Paragraph(new Run(message.Title))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            // Table
            var table = new Table();
            table.CellSpacing = 0;
            var gridLengthConverter = new GridLengthConverter();
            
            // Columns
            table.Columns.Add(new TableColumn { Width = new GridLength(50) }); // SNo
            table.Columns.Add(new TableColumn { Width = new GridLength(120) }); // RegNo
            table.Columns.Add(new TableColumn { Width = new GridLength(250) }); // Name
            table.Columns.Add(new TableColumn { Width = new GridLength(80) }); // Mark

            // Header Row
            var rowGroup = new TableRowGroup();
            var headerRow = new TableRow();
            headerRow.Cells.Add(CreateHeaderCell("SNo"));
            headerRow.Cells.Add(CreateHeaderCell("Reg No"));
            headerRow.Cells.Add(CreateHeaderCell("Student Name"));
            headerRow.Cells.Add(CreateHeaderCell("Int Mark"));
            rowGroup.Rows.Add(headerRow);
            table.RowGroups.Add(rowGroup);

            // Data Rows
            var dataGroup = new TableRowGroup();
            if (message.ReportItems != null)
            {
                foreach (var item in message.ReportItems)
                {
                    var row = new TableRow();
                    row.Cells.Add(CreateCell(item.SNo.ToString()));
                    row.Cells.Add(CreateCell(item.RegNo));
                    row.Cells.Add(CreateCell(item.StudentName));
                    row.Cells.Add(CreateCell(item.InternalMark));
                    dataGroup.Rows.Add(row);
                }
            }
            table.RowGroups.Add(dataGroup);
            
            doc.Blocks.Add(table);
            return doc;
        }

        private TableCell CreateHeaderCell(string text)
        {
            return new TableCell(new Paragraph(new Run(text)))
            {
                FontWeight = FontWeights.Bold,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(5)
            };
        }

        private TableCell CreateCell(string? text)
        {
             return new TableCell(new Paragraph(new Run(text ?? "")))
            {
                Padding = new Thickness(5)
            };
        }
    }
}
