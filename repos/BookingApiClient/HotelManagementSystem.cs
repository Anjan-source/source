using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookingApiClient : TargetApi
{
    private readonly HttpClient httpClient;
    private readonly Ilogger logger;
    public class HotelManagementSystem
    {

    public void HotelManagementSystem(HttpClient httpClinet, Ilogger logger)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc />
        public async Task<IList<Engagement>> GetHotelsAsync()
        {
            string path = "v0/Hotels";

            IList<Hotels> result = await GetAsync<IList<Hotels>>(path)
                .ConfigureAwait(false);

            return result;
        }
    }
}
