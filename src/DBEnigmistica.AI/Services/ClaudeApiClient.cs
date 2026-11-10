using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DBEnigmistica.AI.Services;

/// <summary>
/// Client per interagire con l'API di Claude (Anthropic)
/// </summary>
public class ClaudeApiClient
{
    private readonly ILogger<ClaudeApiClient> _logger;
    private readonly AnthropicClient _client;
    private readonly string _model;
    private readonly int _maxTokens;

    public ClaudeApiClient(
        ILogger<ClaudeApiClient> logger,
        IConfiguration configuration)
    {
        _logger = logger;

        var apiKey = configuration["Anthropic:ApiKey"] 
            ?? throw new InvalidOperationException("Anthropic API key not configured");
        
        _model = configuration["Anthropic:Model"] ?? "claude-sonnet-5-5";
        _maxTokens = int.Parse(configuration["Anthropic:MaxTokens"] ?? "4096");

        _client = new AnthropicClient(new APIAuthentication(apiKey));
        
        _logger.LogInformation("Claude API client initialized with model: {Model}", _model);
    }

    /// <summary>
    /// Invia un prompt a Claude e ottiene una risposta testuale
    /// </summary>
    public async Task<string> SendPromptAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sending prompt to Claude. System prompt length: {SystemLength}, User prompt length: {UserLength}",
            systemPrompt.Length, userPrompt.Length);

        try
        {
            var messages = new List<Message>
            {
                new Message(RoleType.User, userPrompt)
            };

            var parameters = new MessageParameters
            {
                Messages = messages,
                MaxTokens = _maxTokens,
                Model = _model,
                Stream = false,
                Temperature = 0.3m, // Più deterministico per analisi strutturata
                System = new List<SystemMessage>
                {
                    new SystemMessage(systemPrompt)
                }
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            if (response.Content == null || !response.Content.Any())
            {
                throw new InvalidOperationException("Claude returned empty response");
            }

            var textContent = response.Content.FirstOrDefault(c => c is TextContent) as TextContent;
            var responseText = textContent?.Text ?? string.Empty;

            _logger.LogDebug("Received response from Claude. Length: {Length}, Stop reason: {StopReason}",
                responseText.Length, response.StopReason);

            return responseText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            throw;
        }
    }

    /// <summary>
    /// Invia un prompt e deserializza la risposta JSON
    /// </summary>
    public async Task<T> SendPromptForJsonAsync<T>(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var response = await SendPromptAsync(systemPrompt, userPrompt, cancellationToken);

        try
        {
            // Estrai JSON dalla risposta (potrebbe essere wrappato in markdown)
            var json = ExtractJson(response);
            
            var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize Claude response");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON response from Claude: {Response}", response);
            throw new InvalidOperationException("Claude returned invalid JSON", ex);
        }
    }

    private string ExtractJson(string response)
    {
        // Rimuovi eventuali markdown code blocks
        var trimmed = response.Trim();
        
        if (trimmed.StartsWith("```json"))
        {
            trimmed = trimmed.Substring(7);
        }
        else if (trimmed.StartsWith("```"))
        {
            trimmed = trimmed.Substring(3);
        }

        if (trimmed.EndsWith("```"))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 3);
        }

        return trimmed.Trim();
    }
}
