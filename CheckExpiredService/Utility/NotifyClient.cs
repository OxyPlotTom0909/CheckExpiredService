using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

using CheckExpiredService.Message;

namespace CheckExpiredService.Utility
{
    public class NotifyClient : IDisposable
    {
        private const string NOTIFY_BOT_URI = "https://notify-bot.line.me";
        private const string NOTIFY_API_URI = "https://notify-api.line.me";

        private HttpClient _client;
        private string _notifyUri;
        private bool disposedValue;

        public NotifyClient(string notifyUri = NOTIFY_BOT_URI)
        {
            _client = new HttpClient();

            _notifyUri = notifyUri;
        }

        public string GetNotifyAuthurizationCode(string userId)
        {
            var clientId = Environment.GetEnvironmentVariable("LineNotifyClientId");
            var redirectUri = Environment.GetEnvironmentVariable("RedirectUri");

            string notifyUri = $"{_notifyUri}/oauth/authorize?" +
                $"response_type=code&" +
                $"client_id={clientId}&" +
                $"redirect_uri={redirectUri}&" +
                $"scope=notify&" +
                $"state={userId}";

            return notifyUri;
        }

        public async Task<NotifyAuthurizationToken> GetNotifyAccessToken(string code)
        {
            var clientId = Environment.GetEnvironmentVariable("LineNotifyClientId");
            var clientSecret = Environment.GetEnvironmentVariable("LineNotifyClientSerect");
            var redirectUri = Environment.GetEnvironmentVariable("RedirectUri");

            HttpResponseMessage response;
            string notifyuri = $"{_notifyUri}/oauth/token?" +
                $"grant_type=authorization_code&" +
                $"code={code}&" +
                $"redirect_uri={redirectUri}&" +
                $"client_id={clientId}&" +
                $"client_secret={clientSecret}";

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Dictionary<string, string> content = new Dictionary<string, string>();
            var fromdata = new FormUrlEncodedContent(content);
            response = await _client.PostAsync(notifyuri, fromdata).ConfigureAwait(false);
            var returnString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            dynamic result = JsonConvert.DeserializeObject(returnString);
            if (result == null) { return null; }

            return new NotifyAuthurizationToken((string)result?.status, (string)result?.message, (string)result?.access_token);
        }

        public async Task<string> SentNotifyToUser(string userToken, string message)
        {
            _notifyUri = NOTIFY_API_URI;
            string notifyuri = $"{_notifyUri}/api/notify";
            HttpResponseMessage response;

            var hadHeader = _client.DefaultRequestHeaders.Contains("Authorization");
            if (!hadHeader)
            {
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + userToken);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var fromData = new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("message", message)
                });
            response = await _client.PostAsync(notifyuri, fromData).ConfigureAwait(false);

            var returnString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            dynamic result = JsonConvert.DeserializeObject(returnString);
            if (result == null) { return null; }

            return (string)result?.message;
        }

        public async Task<string> SentNotifyUrlImageToUser(string userToken, string userName, string fileUrlSmall, string fileUrl)
        {
            _notifyUri = NOTIFY_API_URI;
            string notifyuri = $"{_notifyUri}/api/notify";
            HttpResponseMessage response;

            var hadHeader = _client.DefaultRequestHeaders.Contains("Authorization");
            if (!hadHeader)
            {
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + userToken);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var fromData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", $"\n\n@{userName} - 新活動通知"),
                    new KeyValuePair<string, string>("imageThumbnail",fileUrlSmall),
                    new KeyValuePair<string, string>("imageFullsize",fileUrl)
                });

            response = await _client.PostAsync(notifyuri, fromData).ConfigureAwait(false);

            var returnString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            dynamic result = JsonConvert.DeserializeObject(returnString);
            if (result == null) { return null; }

            return (string)result?.message;
        }

        public async Task<string> SentNotifyImageFileToUser(string userToken, string fileUrl, string fileName)
        {
            _notifyUri = NOTIFY_API_URI;
            string notifyuri = $"{_notifyUri}/api/notify";
            HttpResponseMessage response;

            var hadHeader = _client.DefaultRequestHeaders.Contains("Authorization");
            if (!hadHeader)
            {
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + userToken);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var fromData = new MultipartFormDataContent
                {
                    {new StringContent("\n\n新通知!!"), "message"},
                    {new ByteArrayContent(await new HttpClient().GetByteArrayAsync(fileUrl)), "imageFile", fileName}
                };
            
            response = await _client.PostAsync(notifyuri, fromData).ConfigureAwait(false);

            var returnString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            dynamic result = JsonConvert.DeserializeObject(returnString);
            if (result == null) { return null; }

            return (string)result?.message;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置 Managed 狀態 (Managed 物件)。
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: 將大型欄位設為 null。
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NotifyClient()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
