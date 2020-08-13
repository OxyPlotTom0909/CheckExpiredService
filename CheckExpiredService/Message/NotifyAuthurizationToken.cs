using System;

namespace CheckExpiredService.Message
{
    public class NotifyAuthurizationToken
    {
        public string Status;
        public string Message;
        public string AccessToken;

        public NotifyAuthurizationToken()
        {
        }

        public NotifyAuthurizationToken(string status, string message, string accesstoken)
        {
            this.Status = status;
            this.Message = message;
            this.AccessToken = accesstoken;
        }

    }
}
