using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskManagement.Models.Entities;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportService _svc;
    private readonly UserManager<ApplicationUser> _users;
    public ReportsController(IReportService s, UserManager<ApplicationUser> u) { _svc = s; _users = u; QuestPDF.Settings.License = LicenseType.Community; }

    public async Task<IActionResult> Index()
    {
        var uid = User.IsInRole("Admin") ? null : _users.GetUserId(User);
        return View(await _svc.BuildAsync(uid));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var uid = User.IsInRole("Admin") ? null : _users.GetUserId(User);
        var data = await _svc.BuildAsync(uid);
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Tasks");
        ws.Cell(1,1).Value = "ID"; ws.Cell(1,2).Value = "Title"; ws.Cell(1,3).Value = "Project";
        ws.Cell(1,4).Value = "Status"; ws.Cell(1,5).Value = "Priority"; ws.Cell(1,6).Value = "Due";
        int r = 2;
        foreach (var t in data.Tasks)
        {
            ws.Cell(r,1).Value = t.Id; ws.Cell(r,2).Value = t.Title;
            ws.Cell(r,3).Value = t.Project?.Name ?? ""; ws.Cell(r,4).Value = t.Status.ToString();
            ws.Cell(r,5).Value = t.Priority.ToString(); ws.Cell(r,6).Value = t.DueDate; r++;
        }
        using var ms = new MemoryStream(); wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
    }

    public async Task<IActionResult> ExportPdf()
    {
        var uid = User.IsInRole("Admin") ? null : _users.GetUserId(User);
        var data = await _svc.BuildAsync(uid);
        var bytes = Document.Create(c =>
        {
            c.Page(p =>
            {
                p.Margin(30);
                p.Header().Text("Report").FontSize(20).Bold();
                p.Content().Column(col =>
                {
                    col.Item().Text($"Total tasks: {data.TotalTasks}");
                    col.Item().Text($"Completed: {data.CompletedTasks}");
                    col.Item().Text($"Pending: {data.PendingTasks}");
                    col.Item().Text($"In Progress: {data.InProgressTasks}");
                    col.Item().Text($"Overdue: {data.OverdueTasks}");
                    col.Item().Text($"Projects: {data.TotalProjects}");
                    col.Item().PaddingTop(10).Table(t =>
                    {
                        t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); });
                        t.Header(h => { h.Cell().Text("Title"); h.Cell().Text("Project"); h.Cell().Text("Status"); h.Cell().Text("Due"); });
                        foreach (var ti in data.Tasks.Take(50))
                        {
                            t.Cell().Text(ti.Title); t.Cell().Text(ti.Project?.Name ?? "");
                            t.Cell().Text(ti.Status.ToString()); t.Cell().Text(ti.DueDate.ToShortDateString());
                        }
                    });
                });
                p.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
            });
        }).GeneratePdf();
        return File(bytes, "application/pdf", "report.pdf");
    }
}
