using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tgs.Report.Aggregate.Client.Models;

namespace Tgs.Report.Aggregate.Client
{
    public class ReportAggregateClient : IReportAggregateClient
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public ReportAggregateClient(HttpClient client)
        {
            _client = client;
        }

        public async ValueTask<MessageResponse> CreatePlayerAggregateAsync(
            CreateUpdatePlayerAggregateModel request)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/playerAggregate")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<MessageResponse>(await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions)!;
        }

        public async ValueTask<MessageResponse> CreateRoundAggregateAsync(
            CreateRoundAggregateModel request)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/roundAggregate")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<MessageResponse>(await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions)!;
        }

        public async ValueTask<MessageResponse> FillPlayerBucketMinutelyAsync(
            FillPlayerBucketMinutelyModel request)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/roundAggregate/fillPlayerBucketMinutely")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<MessageResponse>(await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions)!;
        }

        public async ValueTask<MessageResponse> ManualCreatePlayerAggregateAsync(
            ManualCreateUpdatePlayerAggregateModel request)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/playerAggregate/manual")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<MessageResponse>(await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions)!;
        }

        public async ValueTask<MessageResponse> ManualCreateRoundAggregateAsync(
            ManualCreateUpdateRoundAggregateModel request)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/roundAggregate/manual")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<MessageResponse>(await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions)!;
        }

        public async ValueTask<MessageResponse> ManualFillPlayerBucketMinutelyAsync(
            ManualFillPlayerBucketMinutelyModel request)
        {
            var httpRequest =
                new HttpRequestMessage(HttpMethod.Post, "/api/roundAggregate/manualFillPlayerBucketMinutely")
                {
                    Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                };
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<MessageResponse>(await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions)!;
        }

        public async ValueTask<MessageResponse> RemoveBrandGameProviderAggregateAsync(
            RemoveBrandGameProviderAggregateRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/brandGameProviderAggregate?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<MessageResponse>(
                await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions)!;
        }

        public async ValueTask<MessageResponse> RemoveGameProviderAggregateAsync(
            RemoveGameProviderAggregateRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/gameProviderAggregate?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<MessageResponse>(
                await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions)!;
        }
    }
}
