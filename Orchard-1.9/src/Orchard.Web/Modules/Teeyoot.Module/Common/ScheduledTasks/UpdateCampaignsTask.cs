using Orchard.Data;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.Common.ScheduledTasks
{
    public class CmapaignUpdaterTaskHandler : IScheduledTaskHandler
    {
        private const string TASK_TYPE = "CmapaignUpdaterTask";

        private readonly IScheduledTaskManager _taskManager;
        private readonly ICampaignService _campaignService;
        private readonly IRepository<CampaignRecord> _campaignRepository;
        private readonly IRepository<OrderRecord> _orderRepository;

        public ILogger Logger { get; set; }

        public CmapaignUpdaterTaskHandler(IScheduledTaskManager taskManager, ICampaignService campaignService, IRepository<CampaignRecord> campaignRepository, IRepository<OrderRecord> orderRepository)
        {
            _taskManager = taskManager;
            //foreach (var item in  _taskManager.GetTasks(TASK_TYPE))
            //{
            //    _taskManager.DeleteTasks(item.ContentItem);
            //}
            _campaignService = campaignService;
            _campaignRepository = campaignRepository;
            _orderRepository = orderRepository;

            Logger = NullLogger.Instance;

            try
            {
                var firstDate = DateTime.UtcNow.AddMinutes(2);
                //firstDate = TimeZoneInfo.ConvertTimeToUtc(firstDate, TimeZoneInfo.Local);
                ScheduleNextTask(firstDate);
            }
            catch (Exception e)
            {
                this.Logger.Error(e, e.Message);
            }
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TASK_TYPE)
            {
                try
                {
                    Logger.Information("----------------------------- Update Campaigns --------------------------------");
                    
                    /*foreach (var campaign in _campaignRepository.Fetch(aa => aa.WhenDeleted == null))
                    {
                        var orders = _orderRepository.Fetch(aa => aa.Campaign.Id == campaign.Id && (aa.OrderStatusRecord.Name == "Approved" ||
                                aa.OrderStatusRecord.Name == "Printing" ||
                                aa.OrderStatusRecord.Name == "Shipped" ||
                                aa.OrderStatusRecord.Name == "Delivered"));

                        campaign.ProductCountSold = orders.Select(aa => aa.TotalSold).Sum();
                        _campaignRepository.Update(campaign);

                    }*/
                }
                catch (Exception e)
                {
                    this.Logger.Error("Error occured when running Update Campaigns task ---------------- >" + e.ToString(), e.Message);
                }
                finally
                {
                    Logger.Information("----------------------------- Check Update Campaigns task finished --------------------------------");
                    var nextTaskDate = DateTime.UtcNow.AddMinutes(5);
                    ScheduleNextTask(nextTaskDate);
                }
            }
        }

        private void ScheduleNextTask(DateTime date)
        {
            if (date > DateTime.UtcNow)
            {
                var tasks = this._taskManager.GetTasks(TASK_TYPE);
                foreach (var item in tasks)
                {
                    if (item.ScheduledUtc <= DateTime.UtcNow) _taskManager.DeleteTasks(item.ContentItem);
                }
                tasks = this._taskManager.GetTasks(TASK_TYPE);

                if (tasks == null || tasks.Count() == 0)
                this._taskManager.CreateTask(TASK_TYPE, date, null);
            }
        }
    }
}