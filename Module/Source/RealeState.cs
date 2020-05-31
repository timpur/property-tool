using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using PropertyTool.DataBase;

namespace PropertyTool.Client
{
    public class RealeState
    {

        private readonly HttpClient _client;
        public RealeState()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("origin", "https://www.realestate.com.au");
            _client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36");

        }

        public async Task<IEnumerable<string>> GetProperties()
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
                pageSize = "50"
            });
            var url = new UriBuilder($"https://services.realestate.com.au/services/listings/summaries/search?query={query}");
            var response = await _client.GetAsync(url.ToString());
            var data = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            return data.RootElement
                .GetProperty("tieredResults")
                .EnumerateArray()
                .First()
                .GetProperty("results")
                .EnumerateArray()
                .Select(x => x.GetProperty("listingId").GetString());
        }


        public async Task<Property> GetProperty(string id)
        {
            var response = await _client.GetAsync($"https://services.realestate.com.au/services/listings/{id}");
            var data = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            var address = String.Join(",", new string[]{
                GetJsonProperty(data, "address.streetAddress"),
                GetJsonProperty(data, "address.suburb"),
                GetJsonProperty(data, "address.state"),
                GetJsonProperty(data, "address.postcode")
            });
            var sold = DateTime.Parse(GetJsonProperty(data, "dateSold.value"));
            var price = int.Parse(GetJsonProperty(data, "price.display").Replace("$", "").Replace(",", ""));
            var propertyType = GetJsonProperty(data, "propertyType");
            var landSize = Int32.Parse(GetJsonProperty(data, "landSize.value", "-1"));

            var property = new Property
            {
                Id = id,
                Address = address,
                Sold = sold,
                Price = price,
                PropertyType = propertyType,
                LandSize = landSize
            };

            return property;
        }

        private string GetJsonProperty(JsonDocument document, string prop, string defaultVal = "") => GetJsonProperty(document.RootElement, prop, defaultVal);
        private string GetJsonProperty(JsonElement element, string prop, string defaultVal = "")
        {
            try
            {
                var paths = prop.Split(".");
                foreach (var path in paths) element = element.GetProperty(path);
                return element.ToString();
            }
            catch (InvalidOperationException error)
            {
                return defaultVal;
            }
            catch
            {
                return defaultVal;
            }
        }
    }
}