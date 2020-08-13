using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

using CheckExpiredService.CloudStorage;
using CheckExpiredService.Models;
using CheckExpiredService.Utility;

namespace CheckExpiredService
{
    public static class CheckExpiredService
    {
        [FunctionName("CheckExpiredService")]
        public static async Task Run([TimerTrigger("0 0 */2 * * *")] TimerInfo myTimer, ILogger log)
        {
            string message = string.Empty;
            string messageContent = string.Empty;
            string responseMessage = string.Empty;

            var connectingString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var serviceRemindDays = Environment.GetEnvironmentVariable("ServiceRemindDays");
            var adminId = Environment.GetEnvironmentVariable("AdminId");
            var csLink = Environment.GetEnvironmentVariable("CSContact");
            var adminArray = adminId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var csArray = csLink.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                var serviceList = await TableStorage<UserServiceInfoModel>.CreateTable(connectingString, "ClientServiceInfo");
                var macList = await TableStorage<TradeCallMACRecordModel>.CreateTable(connectingString, "MACTable");

                // Only check alive service
                var mac = macList.FindAsync("ServiceStatus",
                                             TableStorage<TradeCallMACRecordModel>.SelectCondition.Equal,
                                             "Alive").Result;
                var now = DateTime.Now;

                // Comfirm the MAC is availble to use notify funtion
                foreach (var alive in mac)
                {
                    //var exipred = new DateTime(Convert.ToInt64(alive.ServiceExpiredDate));

                    var days = alive.ServiceExpiredDate - now;
                    alive.RemainedDays = days.Days + 1;

                    if (days.Days <= 0)
                    {
                        alive.ServiceStatus = "Expired";
                    }

                    await macList.UpdateAsync(alive);
                }

                var clientList = await TableStorage<UserLineInfo>.CreateTable(connectingString, "ClientInfo");

                var serviceTable = serviceList.FindAsync("ServiceStatus",
                                                         TableStorage<UserServiceInfoModel>.SelectCondition.Equal,
                                                         "Alive").Result;

                var admin = clientList.FindAsync("User", adminArray[0]).Result;
                var notifyClient = new NotifyClient();

                foreach (var service in serviceTable)
                {
                    //var exipred = new DateTime(Convert.ToInt64(service.ServiceExpiredDate));

                    //var days = now.Day - exipred.Day;
                    var days = service.ServiceExpiredDate - now;
                    service.RemainedDays = days.Days + 1;

                    if (days.Days <= 0)
                    {
                        service.ServiceStatus = "Expired";
                    }

                    if (days.Days <= Convert.ToInt16(serviceRemindDays))
                    {
                        var client = clientList.FindAsync("User", service.RowKey).Result;
                        messageContent = $"@{client.UserName}, 您的交易通知服務將於 {days} 後到期，請儘快連絡營業員 {csArray[0]}";

                        if (client != null)
                        {
                            if (client.NotifyToken.Length > 1)
                            {
                                await notifyClient.SentNotifyToUser(client.NotifyToken,
                                                                    $"\n\n@{client.UserName} - 到期通知\n\n{messageContent}");

                            }
                        }

                        if (admin != null)
                        {
                            if (admin.NotifyToken.Length > 1)
                            {
                                await notifyClient.SentNotifyToUser(client.NotifyToken,
                                                                    $"\n\n@{client.UserName} - 到期通知\n\n" +
                                                                    $"會員 @{client.UserName} 交易通知服務將於 {days} 後到期，請通知會員");

                            }

                        }
                    }

                    await serviceList.UpdateAsync(service);
                }

                message = $"推播完成";
            }
            catch (Exception ex)
            {
                message = $"推播未完成，錯誤原因： {ex.ToString()}";
            }
            finally
            {
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}, {message}");
            }

        }

    }
}
