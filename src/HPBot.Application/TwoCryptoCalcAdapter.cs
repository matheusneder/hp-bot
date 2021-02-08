using HPBot.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class TwoCryptoCalcAdapter
    {
        private readonly HttpClient httpClient = new HttpClient(new HttpClientHandler()
        {
            Proxy = new WebProxy()
            {
                Address = new Uri("http://localhost:8888")
            }
        })
        {
            Timeout = TimeSpan.FromSeconds(90)
        };

    public async Task<float> GetEthMiningAverageRewardBtcAsync()
        {
            var httpResponse = await httpClient.GetAsync(
                $"https://2cryptocalc.com/coin/ajax/en/pool/eth/1000000?_={DateTimeOffset.Now.ToUnixTimeMilliseconds()}");

            if(httpResponse.IsSuccessStatusCode)
            {
                var dto = JsonSerializer
                    .Deserialize<EthAverageRewardResultDto>(await httpResponse.Content.ReadAsStringAsync());

                var btcHtml = dto.data
                    .Where(d => Regex.IsMatch(d.time.html, "^<span[^>]*>Day</span>$"))
                    .Select(d => d.btc)
                    .Single()
                    .html;

                string btcText = Regex.Matches(btcHtml, "^<span>([0-9.]+)</span>$")
                    .Single().Groups[1].Value;
                
                return float.Parse(btcText, CultureInfo.InvariantCulture.NumberFormat);
            }

            throw new NotImplementedException(); // TODO: implement
        }
    }
}
