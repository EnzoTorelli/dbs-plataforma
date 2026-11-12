using System.Net.Http.Json;
using System.Text.Json;

namespace DbsErpMobile.Services;

public sealed class ApiService
{
    public const string BaseUrl =
        "https://crispy-space-fortnight-wvg6r9pgqr3g4qp-5175.app.github.dev/";

    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> GetAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            endpoint,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"A API retornou HTTP {(int)response.StatusCode}.");
        }

        var result = await response.Content.ReadFromJsonAsync<T>(
            cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException(
            "A API retornou uma resposta vazia.");
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            endpoint,
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"A API retornou HTTP {(int)response.StatusCode}.");
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(
            cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException(
            "A API retornou uma resposta vazia.");
    }
}
