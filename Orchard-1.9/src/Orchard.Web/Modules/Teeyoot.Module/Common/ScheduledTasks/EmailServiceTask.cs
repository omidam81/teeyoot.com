using Mandrill;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;

namespace Teeyoot.Module.Common.ScheduledTasks
{
    public class EmailServiceTaskHandeler : IScheduledTaskHandler
    {

         private readonly IMailChimpSettingsService _settingsService;
        private readonly IRepository<Outbox> _mailOutBox;

        private readonly IRepository<OrderRecord> _orders;


        private const string TASK_TYPE = "EmailServiceTask";

        private readonly IScheduledTaskManager _taskManager;
        public ILogger Logger { get; set; }


        public EmailServiceTaskHandeler(IScheduledTaskManager taskManager, IMailChimpSettingsService settingsService, IRepository<Outbox> mailOutBox, IRepository<OrderRecord> orders)
        {
            _settingsService = settingsService;
            _mailOutBox = mailOutBox;
            _taskManager = taskManager;
            _orders = orders;

            //foreach (var item in _taskManager.GetTasks(TASK_TYPE))
            //{
            //    _taskManager.DeleteTasks(item.ContentItem);
            //}


            Logger = NullLogger.Instance;

            try
            {
                var firstDate = DateTime.UtcNow.AddMinutes(3);
                //firstDate = TimeZoneInfo.ConvertTimeToUtc(firstDate, TimeZoneInfo.Local);
                ScheduleNextTask(firstDate);
            }
            catch (Exception e)
            {
                this.Logger.Error(e, e.Message);
            }
        }

        private void ScheduleNextTask(DateTime firstDate)
        {
            if (firstDate > DateTime.UtcNow)
            {
                var tasks = this._taskManager.GetTasks(TASK_TYPE);
                foreach (var item in tasks)
                {
                    //if (item.ScheduledUtc <= DateTime.UtcNow) _taskManager.DeleteTasks(item.ContentItem);
                }
                tasks = this._taskManager.GetTasks(TASK_TYPE);

                if (tasks == null || tasks.Count() == 0)
                this._taskManager.CreateTask(TASK_TYPE, firstDate, null);
            }

            var api =
                      new MandrillApi(
                          _settingsService.GetSettingByCulture(/*request.BuyerCultureRecord.Culture*/ "en-MY")
                              .List()
                              .First()
                              .ApiKey);

            var countToSend = 100;

            var emails = _mailOutBox.Table.Take(countToSend).ToArray();
            foreach (Outbox outBox in emails)
            {
                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Mandrill.Model.MandrillMessage>(outBox.Data);
                var result = api.Messages.Send(message);
                _mailOutBox.Delete(outBox);
            }
        }


        
        public void Process(ScheduledTaskContext context)
        {

            if (context.Task.TaskType == TASK_TYPE)
            {
                try
                {
                    Logger.Information("----------------------------- Email Sender task started --------------------------------");
                    // _campaignService.CheckExpiredCampaigns();


                    var api =
                              new MandrillApi(
                                  _settingsService.GetSettingByCulture(/*request.BuyerCultureRecord.Culture*/ "en-MY")
                                      .List()
                                      .First()
                                      .ApiKey);

                    var countToSend = 100;

                    var emails = _mailOutBox.Table.Take(countToSend).ToArray();
                    foreach (Outbox outBox in emails)
                    {

                        if (outBox.EmailType == "Purchase")
                        {
                            if (outBox.OrderId.HasValue && _orders.Get(outBox.OrderId.Value).OrderStatusRecord.Name != OrderStatus.Approved.ToString())
                            {
                                continue;
                            }
                        }
                        var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Mandrill.Model.MandrillMessage>(outBox.Data);
                        var result = api.Messages.Send(message);
                        _mailOutBox.Delete(outBox);
                    }
                }
                catch (Exception e)
                {
                    this.Logger.Error("Error occured when running Order Checker campaigns task ---------------- >" + e.ToString(), e.Message);
                    var nextTaskDate = DateTime.UtcNow.AddMinutes(1).AddSeconds(4);
                    ScheduleNextTask(nextTaskDate);
                }
                finally
                {
                    Logger.Information("----------------------------- Check Order Checker task finished --------------------------------");
                    var nextTaskDate = DateTime.UtcNow.AddMinutes(1).AddSeconds(4);
                    ScheduleNextTask(nextTaskDate);
                }
            }
        }
    }
}