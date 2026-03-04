using AzuriteUI.Web.Pages.Queues;
using AzuriteUI.Web.Services.Azurite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using Xunit;  

namespace AzuriteUI.Web.UnitTests.Pages.Queues;

public class MessagesModel_Tests
{
    [Fact]
    public async Task OnPostCreateAsync_CallsAddQueueMessageAndRedirects()
    {
        // Arrange
        var service = Substitute.For<IAzuriteService>();
        var model = new MessagesModel(service);
        model.PageContext = new PageContext { HttpContext = new DefaultHttpContext() };
        model.QueueName = "testqueue";
        model.NewMessageText = "hello";

        // Act
        var result = await model.OnPostCreateAsync();

        // Assert
        await service.Received(1).AddQueueMessageAsync("testqueue", "hello");
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostDequeueAsync_CallsDequeueAndRedirects()
    {
        // Arrange
        var service = Substitute.For<IAzuriteService>();
        var model = new MessagesModel(service);
        model.PageContext = new PageContext { HttpContext = new DefaultHttpContext() };
        model.QueueName = "testqueue";

        // Act
        var result = await model.OnPostDequeueAsync();

        // Assert
        await service.Received(1).DequeueMessageAsync("testqueue");
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostClearQueueAsync_CallsClearAndRedirects()
    {
        // Arrange
        var service = Substitute.For<IAzuriteService>();
        var model = new MessagesModel(service);
        model.PageContext = new PageContext { HttpContext = new DefaultHttpContext() };
        model.QueueName = "testqueue";

        // Act
        var result = await model.OnPostClearQueueAsync();

        // Assert
        await service.Received(1).ClearQueueAsync("testqueue");
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostDeleteQueueAsync_CallsDeleteAndRedirectsToIndex()
    {
        // Arrange
        var service = Substitute.For<IAzuriteService>();
        var model = new MessagesModel(service);
        model.PageContext = new PageContext { HttpContext = new DefaultHttpContext() };
        model.QueueName = "testqueue";

        // Act
        var result = await model.OnPostDeleteQueueAsync();

        // Assert
        await service.Received(1).DeleteQueueAsync("testqueue");
        Assert.IsType<RedirectToPageResult>(result);
        var redirect = (RedirectToPageResult)result;
        Assert.Equal("/Index", redirect.PageName);
    }

    [Fact]
    public async Task OnPostCreateQueueAsync_CreatesQueueAndRedirects()
    {
        // Arrange
        var service = Substitute.For<IAzuriteService>();
        var model = new MessagesModel(service);
        model.PageContext = new PageContext { HttpContext = new DefaultHttpContext() };
        model.NewQueueName = "newqueue";

        // Act
        var result = await model.OnPostCreateQueueAsync();

        // Assert
        await service.Received(1).CreateQueueAsync("newqueue");
        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("newqueue", ((RedirectToPageResult)result).RouteValues["queueName"]);
    }
}
