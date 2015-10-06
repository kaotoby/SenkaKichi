using SenkaKichi.WcfService.Models;
using System;
using System.Linq;

namespace SenkaKichi.WcfService
{
    public partial class ServiceManager
    {
        public ServiceResult UpdateMaintenanceEndTime() {
            var maintenance = ServiceManager.Current.Database.ServerMaintenances
                .OrderByDescending(m => m.Id)
                .FirstOrDefault();

            if (maintenance == null) {
                return ServiceResult.Fail;
            }
            maintenance.EndTime = DateTime.Now;
            Tasks["UpdateSenka"].NextRunTime = DateTime.Now;
            return ServiceResult.Success;
        }
    }
}
