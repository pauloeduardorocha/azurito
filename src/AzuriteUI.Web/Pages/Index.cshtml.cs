using AzuriteUI.Web.Controllers.Models;
using AzuriteUI.Web.Services.Repositories;
using AzuriteUI.Web.Services.Azurite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AzuriteUI.Web.Pages;

/// <summary>
/// Page model for the dashboard page.
/// </summary>
/// <param name="repository">The storage repository for accessing data.</param>
/// <param name="logger">The logger for diagnostics.</param>
public class IndexModel : PageModel
{
    private readonly IStorageRepository repository;
    private readonly IAzuriteService azuriteService;
    private readonly ILogger<IndexModel> logger;

    public IndexModel(IStorageRepository repository, IAzuriteService azuriteService, ILogger<IndexModel> logger)
    {
        this.repository = repository;
        this.azuriteService = azuriteService;
        this.logger = logger;
    }
    /// <summary>
    /// Gets the dashboard data.
    /// </summary>
    public DashboardResponse? Dashboard { get; private set; }
    public IEnumerable<string>? Queues { get; private set; }

    /// <summary>
    /// Handles GET requests to the dashboard page.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Dashboard = await repository.GetDashboardDataAsync(HttpContext.RequestAborted);
            Queues = await azuriteService.GetQueuesAsync(HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load dashboard data");
            Dashboard = new DashboardResponse
            {
                Stats = new DashboardStats
                {
                    Containers = 0,
                    Blobs = 0,
                    TotalBlobSize = 0,
                    TotalImageSize = 0
                },
                RecentContainers = [],
                RecentBlobs = []
            };
            Queues = Enumerable.Empty<string>();
        }
        return Page();
    }

    [BindProperty]
    public string NewQueueName { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostCreateQueueAsync()
    {
        if (string.IsNullOrWhiteSpace(NewQueueName))
        {
            TempData["StatusMessage"] = "Queue name is required.";
            return RedirectToPage();
        }
        await azuriteService.CreateQueueAsync(NewQueueName);
        TempData["StatusMessage"] = $"Queue '{NewQueueName}' created.";
        return RedirectToPage();
    }
}
