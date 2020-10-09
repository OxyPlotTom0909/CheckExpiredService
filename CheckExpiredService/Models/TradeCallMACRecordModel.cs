using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace CheckExpiredService.Models
{
    public class TradeCallMACRecordModel : TableEntity
    {
        /// <summary>
        /// Record the MAC from the PC of softward.
        /// </summary>
        [IgnoreProperty]
        public string MAC { get { return PartitionKey; } set { PartitionKey = value; } }

        /// <summary>
        /// Generated Authenticate token by Date + Time + AgentID + Software + MAC.
        /// </summary>
        [IgnoreProperty]
        public string AuthenticateToken { get { return RowKey; } set { RowKey = value; } }

        /// <summary>
        /// Record this token belong which agent.
        /// </summary>
        public string AgentId { get; set; }

        /// <summary>
        /// Record the agent belong which company.
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Record which software use. Multicharts, Metatrader, or else.
        /// </summary>
        public string Software { get; set; }

        /// <summary>
        /// Record the trade software first call, then modify the status.
        /// </summary>
        public bool StartUsed { get; set; }

        /// <summary>
        /// Record the status change time.
        /// </summary>
        public DateTime StartUsedTime { get; set; }

        /// <summary>
        /// Record the service start date. Now is same as StartUsedTime.
        /// </summary>
        public DateTime ServiceStartDate { get; set; }

        /// <summary>
        /// Record the service expired date. Add from Start Date
        /// </summary>
        public DateTime ServiceExpiredDate { get; set; }

        /// <summary>
        /// Record the service term.
        /// </summary>
        public string ServiceTerm { get; set; }

        /// <summary>
        /// Record this purchase service status. Alive or Expired.
        /// </summary>
        public string ServiceStatus { get; set; }

        /// <summary>
        /// Record this purchase service remained days. Modified by "CheckExpiredService" function.
        /// </summary>
        public int RemainedDays { get; set; }

        public TradeCallMACRecordModel(string mac, string authenticateToken)
            : base(mac, authenticateToken)
        {
            AgentId = string.Empty;
            Company = string.Empty;
            Software = string.Empty;
            StartUsed = false;
            StartUsedTime = DateTime.Now;
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
