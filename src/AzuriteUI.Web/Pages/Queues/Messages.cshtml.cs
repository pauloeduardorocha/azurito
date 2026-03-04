using Azure.Storage.Queues.Models;
using AzuriteUI.Web.Services.Azurite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AzuriteUI.Web.Pages.Queues;

public class MessagesModel : PageModel
{
    private readonly IAzuriteService _azuriteService;
    public MessagesModel(IAzuriteService azuriteService)
    {
        _azuriteService = azuriteService;
    }

    [BindProperty(SupportsGet = true)]
    public string QueueName { get; set; } = string.Empty;

    [BindProperty]
    public string NewMessageText { get; set; } = string.Empty;

    public QueueItem? QueueInfo { get; set; }
    public List<DecodedQueueMessage> DecodedMessages { get; set; } = new();

    public string? StatusMessage => TempData["StatusMessage"] as string;

    public class DecodedQueueMessage
    {
        public PeekedMessage Message { get; set; } = default!;
        public string DecodedText { get; set; } = string.Empty;
    }

    [BindProperty]
    public string NewQueueName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string queueName)
    {
        QueueName = queueName;
        var queues = await _azuriteService.GetQueuesDetailedAsync();
        QueueInfo = queues.FirstOrDefault(q => q.Name == queueName);
        var messages = await _azuriteService.GetQueueMessagesDetailedAsync(queueName);
        DecodedMessages = messages.Select(m => new DecodedQueueMessage
        {
            Message = m,
            DecodedText = TryDecodeBase64(m.MessageText)
        }).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateQueueAsync()
    {
        if (string.IsNullOrWhiteSpace(NewQueueName))
        {
            TempData["StatusMessage"] = "Queue name is required.";
            return RedirectToPage(new { queueName = QueueName });
        }
        await _azuriteService.CreateQueueAsync(NewQueueName);
        TempData["StatusMessage"] = $"Queue '{NewQueueName}' created.";
        return RedirectToPage(new { queueName = NewQueueName });
    }


    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(QueueName)) return BadRequest();
        if (!string.IsNullOrWhiteSpace(NewMessageText))
        {
            await _azuriteService.AddQueueMessageAsync(QueueName, NewMessageText);
            TempData["StatusMessage"] = "Message created.";
        }
        return RedirectToPage(new { queueName = QueueName });
    }


    public async Task<IActionResult> OnPostDequeueAsync()
    {
        if (string.IsNullOrWhiteSpace(QueueName)) return BadRequest();
        var msg = await _azuriteService.DequeueMessageAsync(QueueName);
        TempData["StatusMessage"] = msg is null ? "No messages to dequeue." : $"Dequeued message {msg.MessageId}.";
        return RedirectToPage(new { queueName = QueueName });
    }

    public async Task<IActionResult> OnPostClearQueueAsync()
    {
        if (string.IsNullOrWhiteSpace(QueueName)) return BadRequest();
        await _azuriteService.ClearQueueAsync(QueueName);
        TempData["StatusMessage"] = "Queue cleared.";
        return RedirectToPage(new { queueName = QueueName });
    }

    public async Task<IActionResult> OnPostDeleteQueueAsync()
    {
        if (string.IsNullOrWhiteSpace(QueueName)) return BadRequest();
        await _azuriteService.DeleteQueueAsync(QueueName);
        TempData["StatusMessage"] = "Queue deleted.";
        return RedirectToPage("/Index");
    }

    private static string TryDecodeBase64(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        // Check if input is likely base64
        input = input.Trim();
        if (input.Length % 4 == 0 && System.Text.RegularExpressions.Regex.IsMatch(input, "^[A-Za-z0-9+/=\r\n]+$"))
        {
            try
            {
                var bytes = Convert.FromBase64String(input);
                var decoded = System.Text.Encoding.UTF8.GetString(bytes);
                // Only return decoded if it is printable
                if (decoded.All(c => !char.IsControl(c) || c == '\r' || c == '\n' || c == '\t'))
                    return decoded;
            }
            catch { }
        }
        return input;
    }
}
