namespace IOCommunation
{
    public class DeviceInfo
    {
        public int Number { get; private set; } = 1;
        public string Name { get; private set; } = @"PCI-1730,BID#0";
        public string ConfigFile { get; private set; } = @"";

        public DeviceInfo()
        {
        }
    }
}
