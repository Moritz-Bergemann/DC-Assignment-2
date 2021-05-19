using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        List<uint> GetAvailableJobs();

        [OperationContract]
        TransmitJobData DownloadJob(uint id);

        [OperationContract]
        TransmitJobData DownloadFirstJob();

        [OperationContract]
        bool PostCompletedJob(uint id, string result);
    }
}
