using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace CheckExpiredService.Models
{
    public class UserServiceInfoModel : TableEntity
    {
        [IgnoreProperty]
        public string UserService { get { return PartitionKey; } set { PartitionKey = value; } }
        [IgnoreProperty]
        public string UserId { get { return RowKey; } set { RowKey = value; } }

        public string Product { get; set; }

        public DateTime AddServiceTime { get; set; }

        public DateTime ServiceStartDate { get; set; }

        public DateTime ServiceExpiredDate { get; set; }

        public string ServiceTerm { get; set; }

        public string ServiceStatus { get; set; }

        public string SendMessagePlatform { get; set; }

        public string PurchasedID { get; set; }

        public int ExpiredNotifyTimes { get; set; }

        public int RemainedDays { get; set; }

        public UserServiceInfoModel(string userService, string userId)
            : base(userService, userId)
        {
            Product = string.Empty;
            AddServiceTime = DateTime.Now;
            ServiceStartDate = DateTime.Now;
            ServiceExpiredDate = DateTime.Now;
            ServiceTerm = string.Empty;
            ServiceStatus = string.Empty;
            SendMessagePlatform = string.Empty;
            PurchasedID = string.Empty;
            ExpiredNotifyTimes = -1;
            RemainedDays = -1;
        }

        public UserServiceInfoModel()
        {
        }
    }
}
