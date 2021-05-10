using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using APIClasses;

namespace ClientApplication
{
    class Server : IServer
    {
        public static Server Instance
        {
            get;
        } = new Server();

        private ServiceHost _host;

        private List<JobData> _jobs;
        private uint _jobCounter;

        private Server()
        {
            _host = null;
            _jobs = new List<JobData>();
            _jobCounter = 0;
        }

        public void OpenServer(string url)
        {
            if (_host != null)
            {
                throw new ArgumentException("Server already running");
            }

            //Create host service
            NetTcpBinding tcp = new NetTcpBinding();

            //Bind service & add endpoint
            _host = new ServiceHost(typeof(Server));
            _host.AddServiceEndpoint(typeof(IServer), tcp, url);

            //Open to receive communications
            _host.Open();
        }

        public void CloseServer()
        {
            if (_host == null)
            {
                throw new ArgumentException("Server is not running");
            }

            //Close host and reset to null
            _host.Close();
            _host = null;
        }

        /// <summary>
        /// Add a new job to the list of pending jobs
        /// </summary>
        /// <param name="python">Python code for the job</param>
        public void PostNewJob(string python)
        {
            JobData job = new JobData(_jobCounter, python);
            
            //Increase job counter for next job
            _jobCounter++;

            _jobs.Add(job);
        }

        // SERVER FUNCTIONALITY
        public List<TransmitJobData> GetAvailableJobs()
        {
            List<TransmitJobData> transmitJobs = new List<TransmitJobData>();

            foreach (JobData job in _jobs)
            {
                TransmitJobData transmitJob = new TransmitJobData(job.Id);

                //Set encoded python and hash
                transmitJob.SetEncodedPython(job.Python);

                transmitJobs.Add(transmitJob);
            }

            return transmitJobs;
        }

        public bool PostCompletedJob(int id, string result)
        {
            //NOTE: Not B64-encoding result string as out of scope of requirements
            //If given ID not in list of job IDs
            if (_jobs.All(job => job.Id != id))
            {
                throw new ArgumentException("Job ID does not exist!");
            }


        }
    }
}
