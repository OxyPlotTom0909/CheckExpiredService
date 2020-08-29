using System;
using System.Collections.Generic;
using System.Linq;
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
        public static async Task Run([TimerTrigger("0 0 0 * * 1-6")] TimerInfo myTimer, ILogger log)
        {
            string message = string.Empty;
            string messageContent = string.Empty;
            string responseMessage = string.Empty;

            var connectingString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var serviceParam = Environment.GetEnvironmentVariable("ServiceParameter").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).
                             ToDictionary(x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[0],
                                          x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            
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
                                             TableStorage<TradeCallMACRecordModel>.SelectCondition.NotEqual,
                                             "Terminated").Result;
                var now = DateTime.Now;

                // Comfirm the MAC is availble to use notify funtion
                foreach (var alive in mac)
                {
                    var days = Math.Ceiling(new TimeSpan(alive.ServiceExpiredDate.Ticks - now.Ticks).TotalDays);
                    alive.RemainedDays = Convert.ToInt16(days);

                    if (alive.RemainedDays <= 0)
                    {
                        alive.ServiceStatus = "Expired";
                    }

                    await macList.UpdateAsync(alive);
                }

                var clientList = await TableStorage<UserLineInfo>.CreateTable(connectingString, "ClientInfo");

                var serviceTable = serviceList.FindAsync("ServiceStatus",
                                                         TableStorage<UserServiceInfoModel>.SelectCondition.NotEqual,
                                                         "Terminated").Result;

                var admin = new List<UserLineInfo>();
                foreach (var item in adminArray)
                {
                    var user = clientList.FindAsync("User", item).Result;
                    if (user != null)
                        admin.Add(user);
                }

                var agents = string.Empty;
                foreach(var item in csArray)
                    agents += (item + "\n");
                
                foreach (var service in serviceTable)
                {
                    var days = Math.Ceiling(new TimeSpan(service.ServiceExpiredDate.Ticks - now.Ticks).TotalDays);
                    service.RemainedDays = Convert.ToInt16(days);

                    if (days <= Convert.ToInt16(serviceParam["ServiceRemindDays"]) && service.ExpiredNotifyTimes > 0)
                    {
                        
                        var client = clientList.FindAsync("User", service.RowKey).Result;

                        if (days > 0)
                            messageContent = $"@{client.UserName}, 您的交易通知服務將於 {service.ServiceExpiredDate.ToString("yyyy/MM/dd")} 到期，請儘快連絡營業員\n{agents}";
                        else
                            messageContent = $"@{client.UserName}, 您的交易通知服務已到期，請連絡營業員\n{agents}";

                        if (client != null)
                        {
                            using (var notifyClient = new NotifyClient())
                            {
                                if (client.NotifyToken.Length > 1)
                                {
                                    await notifyClient.SentNotifyToUser(client.NotifyToken,
                                                                        $"\n\n@{client.UserName} - 到期通知\n\n{messageContent}");

                                }
                            }
                            
                        }

                        if (admin.Count > 0)
                        {
                            foreach (var item in admin)
                            {
                                using (var notifyClient = new NotifyClient())
                                {
                                    if (item.NotifyToken.Length > 1)
                                    {
                                        await notifyClient.SentNotifyToUser(item.NotifyToken,
                                                                            $"\n\n@{client.UserName} - 到期通知\n\n" +
                                                                            $"會員 @{client.UserName} 交易通知服務將於 {days}日 後到期，請通知會員");

                                    }
                                }
                                    
                            }
                        }
                    }

                    if (days <= 0 && service.ServiceStatus == "Alive")
                        service.ServiceStatus = "Expired";

                    if (days <= 0 && service.ExpiredNotifyTimes > 0)
                        service.ExpiredNotifyTimes--;

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
