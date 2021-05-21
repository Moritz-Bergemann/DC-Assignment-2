namespace APIClasses
{
    public class ClientData
    {
        public string Address;
        public uint Port;

        public ClientData()
        {
        }

        public ClientData(string address, uint port)
        {
            Address = address;
            Port = port;
        }

        public ClientData(ClientScoreData scoreData)
        {
            Address = scoreData.Address;
            Port = scoreData.Port;
        }
    }
}
