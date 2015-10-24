using log4net;
using SenkaKichi.DbModels;
using SenkaKichi.WcfService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace SenkaKichi.WcfService
{
    public class Service : IService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Service).FullName);

        public ServiceResult VerifyUserToken(int userId) {
            try {
                return ServiceManager.Current.VerifyUserToken(userId);
            } catch (Exception ex) {
                log.Error(string.Format("[UserId {0}] Unknow error when VerifyUserToken.", userId), ex);
                return ServiceResult.UnknowError;
            }
        }

        public ServiceResult UpdateServerIpAddress() {
            try {
                return ServiceManager.Current.UpdateServerIpAddress();
            } catch (Exception ex) {
                log.Error("Unknow error when UpdateServerIpAddress.", ex);
                return ServiceResult.UnknowError;
            }
        }

        public ServiceResult ForceUpdateSenkaData(int serverId) {
            try {
                return ServiceManager.Current.ForceUpdateSenkaData(serverId);
            } catch (Exception ex) {
                log.Error(string.Format("[ServerId {0}] Unknow error when UpdateSenkaData.", serverId), ex);
                return ServiceResult.UnknowError;
            }
        }
    }

    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        ServiceResult VerifyUserToken(int userId);
        [OperationContract]
        ServiceResult UpdateServerIpAddress();
        [OperationContract]
        ServiceResult ForceUpdateSenkaData(int serverId);
    }

    public enum ServiceResult
    {
        Success,
        UnknowError,
        Unavailable,
        Fail
    }
}
