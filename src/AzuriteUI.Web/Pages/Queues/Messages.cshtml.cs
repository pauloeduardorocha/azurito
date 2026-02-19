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
    public QueueItem? QueueInfo { get; set; }
    public IEnumerable<QueueMessage>? Messages { get; set; }
    public List<DecodedQueueMessage> DecodedMessages { get; set; } = new();

    public class DecodedQueueMessage
    {
        public QueueMessage Message { get; set; } = default!;
        public string DecodedText { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(string queueName)
    {
        QueueName = queueName;
        var queues = await _azuriteService.GetQueuesDetailedAsync();
        QueueInfo = queues.FirstOrDefault(q => q.Name == queueName);
        Messages = await _azuriteService.GetQueueMessagesDetailedAsync(queueName);
        DecodedMessages = Messages?.Select(m => new DecodedQueueMessage
        {
            Message = m,
            DecodedText = TryDecodeBase64(m.MessageText)
        }).ToList() ?? new List<DecodedQueueMessage>();
        return Page();
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
