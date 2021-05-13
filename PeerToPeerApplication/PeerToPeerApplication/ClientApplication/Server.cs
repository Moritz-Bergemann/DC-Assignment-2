using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using APIClasses;
using RestSharp;

namespace ClientApplication
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    class Server : IServer
    {
        public static Server Instance
        {
            get;
        } = new Server();

        private ServiceHost _host;

        private List<JobData> _jobs;
        private List<JobData> _doneJobs;
        private uint _jobIdCounter;

        private string _statusString;

        private RestClient _registryServer;

        private Server()
        {
            _host = null;
            _jobs = new List<JobData>();
            _doneJobs = new List<JobData>();
            _jobIdCounter = 0;

            _statusString = "Not Started";

            _registryServer = new RestClient("https://localhost:44392/");
        }

        public int NumJobs
        {
            get => _jobs.Count;
        }

        public int CompletedJobs
        {
            get => _doneJobs.Count;
        }

        public string Status
        {
            get => _statusString;
        }

        public void Open(string address, uint port)
        {
            _statusString = "Starting";

            if (_host != null)
            {
                throw new ArgumentException("Server already running");
            }

            string url = $"net.tcp://{address}:{port}/PeerServer";

            //Create host service
            NetTcpBinding tcp = new NetTcpBinding();

            //Bind service to this singleton & add endpoint
            _host = new ServiceHost(this);
            _host.AddServiceEndpoint(typeof(IServer), tcp, url);

            //Post self to registry server
            RestRequest request = new RestRequest("api/register");
            request.AddJsonBody(new RegistryData(address, port));
            IRestResponse response = _registryServer.Post(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ArgumentException($"Server failed to open - '{response.Content}'");
            }

            //Open to receive communications
            _host.Open();

            //Set status to open
            _statusString = "Open";
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

            _statusString = "Closed";
        }

        /// <summary>
        /// Add a new job to the list of pending jobs
        /// </summary>
        /// <param name="python">Python code for the job</param>
        public void AddNewJob(string python)
        {
            JobData job = new JobData(_jobIdCounter, python);
            
            //Increase job counter for next job
            _jobIdCounter++;

            _jobs.Add(job);
        }

        // SERVER FUNCTIONALITY
        public List<uint> GetAvailableJobs()
        {
            return _jobs.Select(job => job.Id).ToList();
        }

        public TransmitJobData DownloadJob(uint id)
        {
            TransmitJobData transmitJob = null;
            foreach (JobData job in _jobs)
            {
                if (job.Id == id)
                {
                    transmitJob = new TransmitJobData(id);

                    //Set encoded python and hash
                    transmitJob.SetEncodedPython(job.Python);

                    break;
                }
            }

            return transmitJob;
        }

        public TransmitJobData DownloadFirstJob()
        {
            if (!_jobs.Any())
            {
                return null;
            }

            JobData job = _jobs[0];
            //Create data for submission
            TransmitJobData transmitJob = new TransmitJobData(job.Id);
            transmitJob.SetEncodedPython(job.Python);

            return transmitJob;
        }

        public bool PostCompletedJob(uint id, string result)
        {
            //NOTE: Not B64-encoding result string as out of scope of requirements

            bool found = false;
            int ii = 0;
            foreach (JobData job in _jobs)
            {
                //Find job that was completed
                if (job.Id == id)
                {
                    found = true;

                    //Remove job from pending jobs list
                    _jobs.RemoveAt(ii);

                    //Set job result
                    job.Result = result;

                    //Add job to completed jobs list
                    _doneJobs.Add(job);

                    break;
                }

                ii++;
            }

            return found;
        }
    }
}
