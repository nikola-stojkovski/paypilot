using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace PayPilot.Api.Controllers;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private const string SystemPrompt = "/no_think\nYou are PayPilot, a payroll assistant. Answer briefly.";
    private readonly IChatClient _chatClient;

    public ChatController(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    [HttpPost]
    public IResult Post(ChatRequest request, CancellationToken ct)
        => TypedResults.ServerSentEvents(StreamAsync(request.Message, ct));

    private async IAsyncEnumerable<ChatChunk> StreamAsync(string message, [EnumeratorCancellation] CancellationToken ct)
    {
        List<ChatMessage> messages =
        [
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, message)
        ];

        await foreach (var update in _chatClient.GetStreamingResponseAsync(messages, cancellationToken: ct))
        {
            if (!string.IsNullOrEmpty(update.Text))
                yield return new ChatChunk(update.Text);
        }
    }
}

public record ChatRequest(string Message);
public record ChatChunk(string Text);
