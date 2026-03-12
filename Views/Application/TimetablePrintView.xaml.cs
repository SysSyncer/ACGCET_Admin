using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using ACGCET_Admin.ViewModels.Application;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Linq;

namespace ACGCET_Admin.Views.Application
{
    public partial class TimetablePrintView : UserControl
    {
        public TimetablePrintView()
        {
            InitializeComponent();
            InitializeComponent();
            
            this.Loaded += (s, e) => 
            {
                WeakReferenceMessenger.Default.Register<PrintTimetableMessage>(this, OnPrintRequest);
            };
            
            this.Unloaded += (s, e) => 
            {
                WeakReferenceMessenger.Default.Unregister<PrintTimetableMessage>(this);
            };
        }

        private void OnPrintRequest(object recipient, PrintTimetableMessage message)
        {
            try
            {
                var doc = CreateFlowDocument(message);

                if (message.IsPreview)
                {
                    var window = new Window
                    {
                        Title = "Timetable Preview",
                        Width = 900,
                        Height = 800,
                        Content = new FlowDocumentScrollViewer { Document = doc },
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    window.ShowDialog();
                }
                else
                {
                    var printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        IDocumentPaginatorSource idp = doc;
                        printDialog.PrintDocument(idp.DocumentPaginator, $"Timetable_{message.ExamName}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error processing print request: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateFlowDocument(PrintTimetableMessage message)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new FontFamily("Arial"),
                FontSize = 12
            };

            // Header
            var headerParams = new Paragraph();
            headerParams.TextAlignment = TextAlignment.Center;
            headerParams.Inlines.Add(new Run("ALAGAPPA CHETTIAR GOVERNMENT COLLEGE OF ENGINEERING AND TECHNOLOGY\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
            headerParams.Inlines.Add(new Run("(Autonomous)\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
            headerParams.Inlines.Add(new Run("Karaikudi - 630 003\n") { FontSize=14 });
            headerParams.Inlines.Add(new Run("OFFICE OF THE CONTROLLER OF EXAMINATIONS\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
            headerParams.Inlines.Add(new Run($"{message.ExamName} EXAMINATIONS\n") { FontWeight = FontWeights.Bold, FontSize = 16, TextDecorations = TextDecorations.Underline });
            doc.Blocks.Add(headerParams);
            
            // Sub Header
            var subHeader = new Paragraph();
            subHeader.Inlines.Add(new Run($"Programme: {message.Program}") { FontWeight = FontWeights.Bold });
            subHeader.Inlines.Add(new Run($"\tRegulation: {message.Regulation}") { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(subHeader);

            // Table
            var table = new Table();
            table.CellSpacing = 0;
            table.BorderThickness = new Thickness(1);
            table.BorderBrush = Brushes.Black;
            
            table.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Date
            table.Columns.Add(new TableColumn { Width = new GridLength(80) }); // Session
            table.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Code
            table.Columns.Add(new TableColumn { Width = new GridLength(300) }); // Title
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });  // Sem

            // Header Row
            var headerRow = new TableRow();
            headerRow.Cells.Add(CreateHeaderCell("Date"));
            headerRow.Cells.Add(CreateHeaderCell("Session"));
            headerRow.Cells.Add(CreateHeaderCell("Course Code"));
            headerRow.Cells.Add(CreateHeaderCell("Course Title"));
            headerRow.Cells.Add(CreateHeaderCell("Sem"));
            var headerGroup = new TableRowGroup();
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            // Data
            var dataGroup = new TableRowGroup();
            if (message.Items != null)
            {
                foreach (var item in message.Items)
                {
                     var row = new TableRow();
                     row.Cells.Add(CreateCell(item.Date));
                     row.Cells.Add(CreateCell(item.Session));
                     row.Cells.Add(CreateCell(item.CourseCode));
                     row.Cells.Add(CreateCell(item.CourseTitle));
                     row.Cells.Add(CreateCell(item.Semester.ToString()));
                     dataGroup.Rows.Add(row);
                }
            }
            table.RowGroups.Add(dataGroup);
            doc.Blocks.Add(table);

            // Footer
            var footer = new Paragraph();
            footer.Margin = new Thickness(0, 50, 0, 0);
            footer.TextAlignment = TextAlignment.Right;
            footer.Inlines.Add(new Run("Controller of Examinations") { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(footer);

            return doc;
        }

       private TableCell CreateHeaderCell(string text)
        {
            return new TableCell(new Paragraph(new Run(text)))
            {
                FontWeight = FontWeights.Bold,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Background = Brushes.LightGray
            };
        }

        private TableCell CreateCell(string text)
        {
             return new TableCell(new Paragraph(new Run(text)))
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5)
            };
        }
    }
}
