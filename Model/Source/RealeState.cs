using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using PropertyTool.DataBase;

namespace PropertyTool.Model.Source
{
    public class RealeStateSource : ISource
    {

        private readonly HttpClient _client;
        public RealeStateSource()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("origin", "https://www.realestate.com.au");
            _client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36");

        }

        public async IAsyncEnumerable<IEnumerable<Property>> GetProperties()
        {
            var query = new Query
            {
                channel = "sold",
                filters = new Filters
                {
                    soldDateRange = new DateRange
                    {
                        minimum = "2019-5-29",
                        maximum = "2020-5-29"
                    },
                    surroundingSuburbs = true,
                    excludeTier2 = true,
                    geoPrecision = "address",
                    excludeAddressHidden = true
                },
                boundingBoxSearch = new double[] {
                    -34.73941494337401,
                    150.4609964865428,
                    -33.42462497398022,
                    151.43740639865217
                 },
                pageSize = 100,
                page = 1,
            };

            var url = new UriBuilder($"https://services.realestate.com.au/services/listings/search?query={JsonSerializer.Serialize(query)}");
            var response = await _client.GetAsync(url.ToString());
            var data = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            var pages = data.RootElement.GetProperty("totalResultsCount").GetInt32() / query.pageSize;
            while (query.page <= pages)
            {
                yield return data.RootElement
                    .GetProperty("tieredResults")
                    .EnumerateArray()
                    .First()
                    .GetProperty("results")
                    .EnumerateArray()
                    .Select(x => ParseProperty(x));

                query.page += 1;
                response = await _client.GetAsync(url.ToString());
                data = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            }
        }


        private Property ParseProperty(JsonElement element)
        {
            var id = GetJsonProperty(element, "listingId");
            var address = String.Join(",", new string[]{
                GetJsonProperty(element, "address.streetAddress"),
                GetJsonProperty(element, "address.suburb"),
                GetJsonProperty(element, "address.state"),
                GetJsonProperty(element, "address.postcode")
            });
            var sold = DateTime.Parse(GetJsonProperty(element, "dateSold.value"));
            var price = int.Parse(GetJsonProperty(element, "price.display").Replace("$", "").Replace(",", ""));
            var propertyType = GetJsonProperty(element, "propertyType");
            var landSize = Int32.Parse(GetJsonProperty(element, "landSize.value", "-1"));

            var property = new Property
            {
                SourceId = id,
                Address = address,
                Sold = sold,
                Price = price,
                PropertyType = propertyType,
                LandSize = landSize
            };

            return property;
        }

        private string GetJsonProperty(JsonElement element, string prop, string defaultVal = "")
        {
            try
            {
                var paths = prop.Split(".");
                foreach (var path in paths) element = element.GetProperty(path);
                return element.ToString();
            }
            catch
            {
                return defaultVal;
            }
        }

        private class Query
        {
            public string channel { get; set; }
            public Filters filters { get; set; }
            public double[] boundingBoxSearch { get; set; }
            public int pageSize { get; set; }
            public int page { get; set; }
        }

        private class Filters
        {
            public DateRange soldDateRange { get; set; }
            public bool surroundingSuburbs { get; set; }
            public bool excludeTier2 { get; set; }
            public string geoPrecision { get; set; }
            public bool excludeAddressHidden { get; set; }
        }
        private class DateRange
        {
            public string minimum { get; set; }
            public string maximum { get; set; }
        }
    }
}