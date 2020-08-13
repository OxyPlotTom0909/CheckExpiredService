using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace CheckExpiredService.Models
{
    public class TradeCallMACRecordModel : TableEntity
    {
        public string AuthenticateToken { get; set; }

        public bool Verified { get; set; }

        public string UserIdentify { get; set; }

        public DateTime AddServiceTime { get; set; }

        public DateTime ServiceStartDate { get; set; }

        public DateTime ServiceExpiredDate { get; set; }

        public string ServiceTerm { get; set; }

        public string ServiceStatus { get; set; }

        public int RemainedDays { get; set; }

        public TradeCallMACRecordModel(string software, string mac)
            : base(software, mac)
        {
            AuthenticateToken = string.Empty;
            Verified = false;
            UserIdentify = string.Empty;
            AddServiceTime = DateTime.Now;
            ServiceStartDate = DateTime.Now;
            ServiceExpiredDate = DateTime.Now;
            ServiceTerm = string.Empty;
            ServiceStatus = string.Empty;
            RemainedDays = -1;
        }

        public TradeCallMACRecordModel()
        {
        }
    }
}
