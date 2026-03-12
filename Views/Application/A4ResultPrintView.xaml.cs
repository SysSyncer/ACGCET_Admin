using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using ACGCET_Admin.ViewModels.Application;
using CommunityToolkit.Mvvm.Messaging;
using System.Linq;

namespace ACGCET_Admin.Views.Application
{
    /// <summary>
    /// Interaction logic for A4ResultPrintView.xaml
    /// </summary>
    public partial class A4ResultPrintView : UserControl
    {
        public A4ResultPrintView()
        {
            InitializeComponent();
            
            this.Loaded += (s, e) => 
            {
                WeakReferenceMessenger.Default.Register<PrintA4ResultMessage>(this, OnPrintRequest);
            };
            
            this.Unloaded += (s, e) => 
            {
                WeakReferenceMessenger.Default.Unregister<PrintA4ResultMessage>(this);
            };
        }

        private void OnPrintRequest(object recipient, PrintA4ResultMessage message)
        {
            try
            {
                if (message.Students == null || !message.Students.Any()) return;

                var doc = GenerateResultDocument(message);

                if (message.IsPreview)
                {
                    var window = new Window
                    {
                        Title = "A4 Result Print - Preview",
                        Content = new DocumentViewer { Document = doc },
                        Width = 800,
                        Height = 1000,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    window.ShowDialog();
                }
                else
                {
                    var printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "A4 Result Print");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error processing print request: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument GenerateResultDocument(PrintA4ResultMessage msg)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 12
            };

            // Header
            var header = new Paragraph(new Run("A4 RESULT PRINT"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            doc.Blocks.Add(header);

            // Info
            doc.Blocks.Add(new Paragraph(new Run($"Examination: {msg.ExamName}")) { Margin = new Thickness(0, 5, 0, 0) });
            doc.Blocks.Add(new Paragraph(new Run($"Program: {msg.Program}")) { Margin = new Thickness(0, 0, 0, 0) });
            doc.Blocks.Add(new Paragraph(new Run($"Regulation: {msg.Regulation}")) { Margin = new Thickness(0, 0, 0, 0) });
            doc.Blocks.Add(new Paragraph(new Run($"Section: {msg.Section}")) { Margin = new Thickness(0, 0, 0, 10) });

            // Table
            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });
            table.Columns.Add(new TableColumn { Width = new GridLength(250) });

            var headerGroup = new TableRowGroup();
            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("S.No"))) { FontWeight = FontWeights.Bold, Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1) });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Reg. No"))) { FontWeight = FontWeights.Bold, Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1) });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Roll No"))) { FontWeight = FontWeights.Bold, Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1) });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Student Name"))) { FontWeight = FontWeights.Bold, Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1) });
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            var bodyGroup = new TableRowGroup();
            foreach (var student in msg.Students)
            {
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(student.SNo.ToString()))) { Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(student.RegistrationNumber))) { Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(student.RollNumber))) { Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(student.StudentName))) { Padding = new Thickness(5), BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1) });
                bodyGroup.Rows.Add(row);
            }
            table.RowGroups.Add(bodyGroup);

            doc.Blocks.Add(table);
            return doc;
        }
    }
}
