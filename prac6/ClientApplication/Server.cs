using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
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
        private RestClient _registryServer;
        private ClientData _myClientData; //Stores client data for this client

        private List<JobData> _jobs;
        private List<JobData> _inProgressJobs;
        private List<JobData> _doneJobs;
        private uint _jobIdCounter;

        private string _statusString;


        private Server()
        {
            _host = null;
            _jobs = new List<JobData>();
            _inProgressJobs = new List<JobData>();
            _doneJobs = new List<JobData>();
            _jobIdCounter = 0;
            
            _statusString = "Not Started";

            _myClientData = null;
            _registryServer = new RestClient("https://localhost:44392/");

            //Start service to check for timed out in-progress jobs every 5 seconds
            DispatcherTimer dTimer = new DispatcherTimer();
            dTimer.Tick += new EventHandler(CheckTimeouts);
            dTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dTimer.Start();
        }

        public int NumJobs
        {
            get => _jobs.Count;
        }

        public ReadOnlyCollection<JobData> CompletedJobs
        {
            get => _doneJobs.AsReadOnly();
        }

        public string Status
        {
            get => _statusString;
        }

        public ClientData EndpointData
        {
            get => new ClientData(_myClientData.Address, _myClientData.Port);
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

            try
            {
                _host.AddServiceEndpoint(typeof(IServer), tcp, url);
            }
            catch (UriFormatException)
            {
                _statusString = "Error";
                _host = null;
                throw new ArgumentException($"Invalid server URL '{url}'");
            }

            //Post self to registry server
            RestRequest request = new RestRequest("api/register");
            request.AddJsonBody(new ClientData(address, port));
            IRestResponse response = _registryServer.Post(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _statusString = "Error";
                throw new ArgumentException($"Server failed to open - '{response.Content}'");
            }

            //Open to receive communications
            _host.Open();

            _myClientData = new ClientData(address, port);

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

            _myClientData = null;

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

        private void AddInProgressJob(JobData job)
        {
            //Set job to time out in 1 minute
            job.Timeout = DateTime.Now.AddSeconds(10);

            _inProgressJobs.Add(job);
        }

        private void CheckTimeouts(object sender, EventArgs e)
        {
            int ii = 0;
            while (ii < _inProgressJobs.Count)
            {
                //If timeout has expired, remove from list of in-progress jobs
                if (_inProgressJobs[ii].Timeout.CompareTo(DateTime.Now) <= 0)
                {
                    //Remove element from list of in progress jobs and add it back to list of available jobs
                    JobData removeJob = _inProgressJobs[ii];
                    _inProgressJobs.RemoveAt(ii);

                    //Add job to list of available jobs (with a NEW ID so if the original client eventually does return it won't see the job as still "in progress")
                    AddNewJob(removeJob.Python);
                }
                else
                {
                    ii++;
                }
            }
        }

        // SERVER FUNCTIONALITY
        public List<uint> GetAvailableJobs()
        {
            return _jobs.Select(job => job.Id).ToList();
        }

        public TransmitJobData DownloadJob(uint id)
        {
            TransmitJobData transmitJob = null;
            int ii = 0;
            foreach (JobData job in _jobs)
            {
                if (job.Id == id)
                {
                    transmitJob = new TransmitJobData(id);

                    //Set encoded python and hash
                    transmitJob.SetEncodedPython(job.Python);

                    //Move job to list of in progress jobs
                    _jobs.RemoveAt(ii);
                    AddInProgressJob(job);

                    break;
                }

                ii++;
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

            //Move job to list of in progress jobs
            _jobs.RemoveAt(0);
            AddInProgressJob(job);

            return transmitJob;
        }

        public bool PostCompletedJob(uint id, string result, ClientData sender)
        {
            //NOTE: Not B64-encoding result string as out of scope of requirements

            bool found = false;
            int ii = 0;
            foreach (JobData job in _inProgressJobs)
            {
                //Find job that was completed
                if (job.Id == id)
                {
                    found = true;

                    //Remove job from pending jobs list
                    _inProgressJobs.RemoveAt(ii);

                    //Set job result
                    job.Result = result;

                    //Add job to completed jobs list
                    _doneJobs.Add(job);

                    break;
                }

                ii++;
            }

            //Update the sender's score if a job was completed
            if (found)
            {
                RestRequest request = new RestRequest("api/add-to-score");
                request.AddJsonBody(sender);
                _registryServer.Post(request);
            }

            return found;
        }
    }
}
