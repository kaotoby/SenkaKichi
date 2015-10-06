using SenkaKichi.WcfService.Models;

namespace SenkaKichi.WcfService
{
    public partial class ServiceManager
    {
        public ServiceResult ForceUpdateSenkaData(int serverId) {
            var info = Servers[serverId];
            if (info.IsUpdating) {
                return ServiceResult.Unavailable;
            }

            info.RefreshToken();
            if (!info.Enabled) {
                return ServiceResult.Fail;
            }
            UpdateSenkaTask.UpdateDateInfo();
            UpdateSenkaTask.UpdateServerInfo(info);
            return ServiceResult.Success;
        }
    }
}
