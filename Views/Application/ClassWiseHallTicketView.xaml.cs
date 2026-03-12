using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using ACGCET_Admin.ViewModels.Application;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Linq;

namespace ACGCET_Admin.Views.Application
{
    public partial class ClassWiseHallTicketView : UserControl
    {
        public ClassWiseHallTicketView()
        {
            InitializeComponent();
            InitializeComponent();
            
            this.Loaded += (s, e) => 
            {
                WeakReferenceMessenger.Default.Register<PrintHallTicketMessage>(this, OnPrintRequest);
            };
            
            this.Unloaded += (s, e) => 
            {
                WeakReferenceMessenger.Default.Unregister<PrintHallTicketMessage>(this);
            };
        }

        private void OnPrintRequest(object recipient, PrintHallTicketMessage message)
        {
            try
            {
                var doc = CreateFlowDocument(message);

                if (message.IsPreview)
                {
                    var window = new Window
                    {
                        Title = "Hall Ticket Preview",
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
                        printDialog.PrintDocument(idp.DocumentPaginator, "Hall Tickets");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error processing print request: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateFlowDocument(PrintHallTicketMessage message)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new FontFamily("Arial"),
                FontSize = 12,
                ColumnWidth = 999999 // Prevents columns
            };

            foreach (var student in message.Students)
            {
                // Create Ticket Section
                var ticketSection = new Section();
                ticketSection.BreakPageBefore = true; // Each ticket on new page

                // Header
                var headerParams = new Paragraph();
                headerParams.TextAlignment = TextAlignment.Center;
                headerParams.Inlines.Add(new Run("ALAGAPPA CHETTIAR GOVERNMENT COLLEGE OF ENGINEERING AND TECHNOLOGY\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
                headerParams.Inlines.Add(new Run("(Autonomous)\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
                headerParams.Inlines.Add(new Run("Karaikudi - 630 003\n") { FontSize=14 });
                headerParams.Inlines.Add(new Run("HALL TICKET") { FontWeight = FontWeights.Bold, FontSize = 18, TextDecorations = TextDecorations.Underline });
                ticketSection.Blocks.Add(headerParams);

                // Student Info Table
                var infoTable = new Table();
                infoTable.Columns.Add(new TableColumn { Width = new GridLength(120) });
                infoTable.Columns.Add(new TableColumn { Width = new GridLength(300) });
                infoTable.Columns.Add(new TableColumn { Width = new GridLength(150) }); // For Photo placeholder potentially

                var infoRowGroup = new TableRowGroup();
                
                infoRowGroup.Rows.Add(CreateRow("Register Number:", student.RegNo));
                infoRowGroup.Rows.Add(CreateRow("Name of the Candidate:", student.StudentName));
                infoRowGroup.Rows.Add(CreateRow("Degree & Branch:", message.Program));
                infoRowGroup.Rows.Add(CreateRow("Regulation:", message.Regulation));
                infoRowGroup.Rows.Add(CreateRow("Month & Year:", message.ExamName));
                
                infoTable.RowGroups.Add(infoRowGroup);
                ticketSection.Blocks.Add(infoTable);
                
                // Spacing
                ticketSection.Blocks.Add(new Paragraph(new Run(" ")));

                // Schedule Table
                var scheduleTable = new Table();
                scheduleTable.CellSpacing = 0;
                scheduleTable.BorderThickness = new Thickness(1);
                scheduleTable.BorderBrush = Brushes.Black;
                
                scheduleTable.Columns.Add(new TableColumn { Width = new GridLength(80) }); // Code
                scheduleTable.Columns.Add(new TableColumn { Width = new GridLength(300) }); // Title
                scheduleTable.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Date
                scheduleTable.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Session
                scheduleTable.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Signature

                // Header
                var headerRow = new TableRow();
                headerRow.Cells.Add(CreateHeaderCell("Sub Code"));
                headerRow.Cells.Add(CreateHeaderCell("Subject Title"));
                headerRow.Cells.Add(CreateHeaderCell("Date"));
                headerRow.Cells.Add(CreateHeaderCell("Session"));
                headerRow.Cells.Add(CreateHeaderCell("Invigilator Sign"));
                var headerGroup = new TableRowGroup();
                headerGroup.Rows.Add(headerRow);
                scheduleTable.RowGroups.Add(headerGroup);

                // Data
                var dataGroup = new TableRowGroup();
                if (message.Schedule != null)
                {
                    foreach (var exam in message.Schedule)
                    {
                         var row = new TableRow();
                         row.Cells.Add(CreateCell(exam.SubjectCode));
                         row.Cells.Add(CreateCell(exam.SubjectName));
                         row.Cells.Add(CreateCell(exam.ExamDate));
                         row.Cells.Add(CreateCell(exam.Session));
                         row.Cells.Add(CreateCell("")); // Signature placeholder
                         dataGroup.Rows.Add(row);
                    }
                }
                scheduleTable.RowGroups.Add(dataGroup);
                ticketSection.Blocks.Add(scheduleTable);

                // Footer
                var footer = new Paragraph();
                footer.Margin = new Thickness(0, 50, 0, 0);
                footer.Inlines.Add(new Run("Signature of the Student") { FontWeight = FontWeights.Bold });
                footer.Inlines.Add(new Run("\t\t\t\t\t\t\t\t"));
                footer.Inlines.Add(new Run("Controller of Examinations") { FontWeight = FontWeights.Bold });
                ticketSection.Blocks.Add(footer);

                doc.Blocks.Add(ticketSection);
            }
            
            return doc;
        }

        private TableRow CreateRow(string label, string value)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontWeight = FontWeights.Bold }));
            row.Cells.Add(new TableCell(new Paragraph(new Run(value))));
            return row;
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
