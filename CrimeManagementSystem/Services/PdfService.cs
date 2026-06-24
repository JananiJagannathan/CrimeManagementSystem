using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CrimeManagementSystem.Services
{
    public class PdfService : IPdfService
    {
        static PdfService()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        public byte[] GenerateIncidentReport(IncidentResponseDTO incident)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // HEADER
                    page.Header().Column(column =>
                    {
                        column.Item().Text("CRIME MANAGEMENT SYSTEM")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        column.Item().Text("Incident Report")
                            .FontSize(14).FontColor(Colors.Grey.Darken1);
                        column.Item().PaddingTop(5).LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten1);
                    });

                    // CONTENT 
                    page.Content().PaddingTop(20).Column(column =>
                    {
                        column.Spacing(8);

                        AddRow(column, "Incident Code:", incident.IncidentCode);
                        AddRow(column, "Type:", incident.Type);
                        AddRow(column, "Status:", incident.Status);
                        AddRow(column, "Description:", incident.Description);
                        AddRow(column, "Location:", incident.Location);
                        AddRow(column, "Incident Date:", incident.IncidentDate.ToString("dd-MM-yyyy"));
                        AddRow(column, "Reported At:", incident.ReportedAt.ToString("dd-MM-yyyy HH:mm"));
                        AddRow(column, "Reported By:", incident.ReportedByName);
                        AddRow(column, "Assigned Officer:",
                            string.IsNullOrEmpty(incident.AssignedOfficerName)
                                ? "Not yet assigned" : incident.AssignedOfficerName);

                        if (!string.IsNullOrEmpty(incident.PropertyDescription))
                            AddRow(column, "Property Description:", incident.PropertyDescription);

                        if (incident.EstimatedValue.HasValue)
                            AddRow(column, "Estimated Value:", $"₹{incident.EstimatedValue.Value}");

                        if (!string.IsNullOrEmpty(incident.SuspectDescription))
                            AddRow(column, "Suspect Description:", incident.SuspectDescription);

                        column.Item().PaddingTop(20).LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);

                        column.Item().PaddingTop(10).Text(
                            "This is a system-generated report from the Crime Management System.")
                            .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                    });

                    // FOOTER
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generated on: ");
                        text.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm")).Bold();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void AddRow(QuestPDF.Fluent.ColumnDescriptor column, string label, string value)
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(150).Text(label).Bold();
                row.RelativeItem().Text(value);
            });
        }
    }
}