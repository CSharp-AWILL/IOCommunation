using System;
using Aqrose.Framework.Utility.MessageManager;
using Automation.BDaq;

namespace IOCommunation.Writer
{
    public class WriterDevice
    {
        static private InstantDoCtrl _instance = null;

        static public readonly object PadLock = new object();

        private WriterDevice()
        {
        }

        static public InstantDoCtrl GetInstance()
        {
            try
            {
                if (null == _instance)
                {
                    lock (PadLock)
                    {
                        if (null == _instance)
                        {
                            _instance = new InstantDoCtrl();
                            // move in protected block.
                            var deviceInfo = new DeviceInfo();
                            _instance.SelectedDevice = new DeviceInformation(deviceInfo.Number, deviceInfo.Name, AccessMode.ModeWriteWithReset, 0);
                            //_instance.LoadProfile(deviceInfo.ConfigFile);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageManager.Instance().Alarm("IOWriter: "+ e.Message);
            }

            return _instance;
        }
    }
}
