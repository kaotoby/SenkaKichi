using SenkaKichi.WcfService.Models;

namespace SenkaKichi.WcfService
{
    public partial class ServiceManager
    {
        public ServiceResult UpdateServerIpAddress() {
            var info = Servers[19];
            var data = new DmmLoginHelper(info.Server.ServerAuthorize, info.HttpHelper).GetIp();
            foreach (var item in data) {
                Servers[item.Key].Server.ServerAuthorize.IpAddress = item.Value;
                Servers[item.Key].IP = item.Value;
            }
            Database.SaveChanges();
            return ServiceResult.Success;
        }
    }
}
