using System;
using Aqrose.Framework.Utility.MessageManager;
using Automation.BDaq;

namespace IOCommunation.Reader
{
    public class ReaderDevice
    {
        public enum EStatus
        {
            LOW=0,
            HIGH,
            RISING,
            FALLING

        }

        public static int PortCount { get; } = 2;
        public static int ChanelCount { get; } = 8;
        static private InstantDiCtrl _instance = null;
        static private readonly object _padLock = new object();
        private ReaderDevice()
        {
        }

        static public InstantDiCtrl GetInstance()
        {
            try
            {
                if (null == _instance)
                {
                    lock (_padLock)
                    {
                        if (null == _instance)
                        {
                            _instance = new InstantDiCtrl();
                            // move in protected block.
                            var deviceInfo = new DeviceInfo();
                            _instance.SelectedDevice = new DeviceInformation(deviceInfo.Number, deviceInfo.Name, AccessMode.ModeRead, 0);
                            //_instance.LoadProfile(deviceInfo.ConfigFile);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageManager.Instance().Alarm("IOReader: "+ e.Message);
            }

            return _instance;
        }

    }
}
