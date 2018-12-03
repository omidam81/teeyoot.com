using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.Common.ScheduledTasks
{
    public class OrderCheckerHandler: IScheduledTaskHandler
    {
        private const string TASK_TYPE = "OrderCheckerTask";

        private readonly IScheduledTaskManager _taskManager;
        private readonly ICampaignService _campaignService;
        private readonly IOrderService _orderService;

        public ILogger Logger { get; set; }

        public OrderCheckerHandler(IScheduledTaskManager taskManager, ICampaignService campaignService, IOrderService orderService)
        {
            _taskManager = taskManager;
            //foreach (var item in _taskManager.GetTasks(TASK_TYPE))
            //{
            //    _taskManager.DeleteTasks(item.ContentItem);
            //}
            _campaignService = campaignService;
            _orderService = orderService;


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
                    Logger.Information("----------------------------- Check Order Checker task started --------------------------------");
                   // _campaignService.CheckExpiredCampaigns();

                    var _ordersToDelete = _orderService.GetAllOrders().Where(aa => aa.Email == null);//.Where(aa => aa.Created - DateTime.Now.Subtract(new DateTime(0, 0, 0, 0, 31, 0, 0)));

                    foreach (var order in _ordersToDelete)
                    {
                        //System.IO.File.Create("c:\\omid.txt").Write(new byte[] { 1, 5, 7 }, 0, 3);
                        if (order.Created.AddMinutes(15) > DateTime.UtcNow) continue;
                        _orderService.DeleteOrder(order.Id);
                    }
                }
                catch (Exception e)
                {
                    this.Logger.Error("Error occured when running Order Checker campaigns task ---------------- >" + e.ToString(), e.Message);
                    var nextTaskDate = DateTime.UtcNow.AddMinutes(10);
                    ScheduleNextTask(nextTaskDate);
                }
                finally
                {
                    Logger.Information("----------------------------- Check Order Checker task finished --------------------------------");
                    var nextTaskDate = DateTime.UtcNow.AddMinutes(10);
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
                  // if (item.ScheduledUtc <= DateTime.UtcNow) _taskManager.DeleteTasks(item.ContentItem);
                }
                tasks = this._taskManager.GetTasks(TASK_TYPE);

                if (tasks == null || tasks.Count() == 0)
                    this._taskManager.CreateTask(TASK_TYPE, date, null);
            }

            var _ordersToDelete = _orderService.GetAllOrders().Where(aa => aa.Email == null);//.Where(aa => aa.Created - DateTime.Now.Subtract(new DateTime(0, 0, 0, 0, 31, 0, 0)));

            foreach (var order in _ordersToDelete)
            {
                if (order.Created.AddMinutes(15) > DateTime.UtcNow) continue;
                _orderService.DeleteOrder(order.Id);
            }


        }
    }
}