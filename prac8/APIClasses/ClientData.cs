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
    }
}
