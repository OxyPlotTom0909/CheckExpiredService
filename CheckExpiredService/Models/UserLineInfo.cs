using Microsoft.WindowsAzure.Storage.Table;

namespace CheckExpiredService.Models
{
    public class UserLineInfo : TableEntity
    {
        public string UserName { get; set; }

        public string UserClass { get; set; }

        public string UserStatus { get; set; }

        public string UserData1 { get; set; }

        public string UserData2 { get; set; }

        public string UserData3 { get; set; }

        public string UserData4 { get; set; }

        public string UserData5 { get; set; }

        public string NotifyRequestTime { get; set; }

        public string NotifyToken { get; set; }

        public UserLineInfo(string userType, string userID)
            :base(userType, userID)
        {
            UserName = string.Empty;
            UserClass = string.Empty;
            UserStatus = string.Empty;
            UserData1 = string.Empty;
            UserData2 = string.Empty;
            UserData3 = string.Empty;
            UserData4 = string.Empty;
            UserData5 = string.Empty;
            NotifyRequestTime = string.Empty;
            NotifyToken = string.Empty;
        }

        public UserLineInfo(){}
    }
}
