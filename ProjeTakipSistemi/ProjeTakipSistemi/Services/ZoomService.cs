using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProjeTakipSistemi.Services
{
    public class ZoomService
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string accountId;

        private string accessToken;
        private DateTime tokenExpiry;

        public ZoomService(string clientId, string clientSecret, string accountId)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.accountId = accountId;
        }

        private async Task EnsureTokenAsync()
        {
            if (accessToken == null || DateTime.UtcNow >= tokenExpiry)
            {
                var tokenResponse = await GetAccessTokenAsync();
                accessToken = tokenResponse.access_token;
                tokenExpiry = DateTime.UtcNow.AddSeconds((int)tokenResponse.expires_in - 30);
            }
        }

        private async Task<dynamic> GetAccessTokenAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

                var response = await httpClient.PostAsync(
                    $"https://zoom.us/oauth/token?grant_type=account_credentials&account_id={accountId}",
                    null
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception("Zoom Token Alınamadı: " + error);
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(json);
            }
        }

        public async Task<dynamic> CreateMeetingAsync(string topic, DateTime startTime, int duration)
        {
            await EnsureTokenAsync();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var body = new
                {
                    topic = topic,
                    type = 2,
                    start_time = startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    duration = duration,
                    timezone = "Europe/Istanbul",
                    settings = new
                    {
                        host_video = true,
                        participant_video = true,
                        join_before_host = false
                    }
                };

                var jsonBody = JsonConvert.SerializeObject(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.zoom.us/v2/users/me/meetings", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception("Toplantı Oluşturulamadı: " + error);
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(json);
            }
        }
    }
}
