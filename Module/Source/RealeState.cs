using System;
using System.Web;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace PropertyTool.Client
{
    public class RealeState
    {

        private readonly HttpClient _client;
        public RealeState()
        {
            _client = new HttpClient();
        }

        public async Task GetProperty()
        {
            var query = JsonSerializer.Serialize(new
            {
                channel = "sold",
                filters = new
                {
                    soldDateRange = new
                    {
                        minimum = "2019-5-29",
                        maximum = "2020-5-29"
                    },
                    surroundingSuburbs = "true",
                    excludeTier2 = "true",
                    geoPrecision = "address",
                    excludeAddressHidden = "true"
                },
                pageSize = "500"
            });
            var url = new UriBuilder($"https://services.realestate.com.au/services/listings/summaries/search?query={query}");
            var request = new HttpRequestMessage(HttpMethod.Get, url.ToString());
            request.Headers.Add("origin", "https://www.realestate.com.au");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36");
            var response = await _client.SendAsync(request);
            var data = await response.Content.ReadAsStringAsync();
        }
    }
}