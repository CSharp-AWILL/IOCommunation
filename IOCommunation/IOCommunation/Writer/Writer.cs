using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Aqrose.Framework.Core.Attributes;
using Aqrose.Framework.Core.DataType;
using Aqrose.Framework.Core.Interface;
using Aqrose.Framework.Utility.MessageManager;
using Aqrose.Framework.Utility.Tools;
using Automation.BDaq;

namespace IOCommunation.Writer
{
    [Module("Writer", "IOCommunation", "")]
    public class Writer : ModuleData, IModule
    {
        private InstantDoCtrl _device = null;

        public readonly int PortCount = 2;

        public Dictionary<BitOfBoard, byte> ModifiedBits = new Dictionary<BitOfBoard, byte>();

        public Writer()
        {
            _device = WriterDevice.GetInstance();
        }

        public bool TryReadPort(int port, out byte data)
        {
            data = 0;
            
            try
            {
                if (null == _device || (!_device.Initialized))
                {
                    return false;
                }
                var errorCode = _device.Read(port, out data);
                if (ErrorCode.Success != errorCode)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);

                return false;
            }

            return true;
        }

        public bool TryReadBit(int port, int bit, out byte data)
        {
            data = 0;
            
            try
            {
                if (null == _device || (!_device.Initialized))
                {
                    return false;
                }
                var errorCode = _device.ReadBit(port, bit, out data);
                if (ErrorCode.Success != errorCode)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);

                return false;
            }

            return true;
        }

        public void WriteValue()
        {
            try
            {
                if (null == _device || (!_device.Initialized))
                {
                    return;
                }
                if (null == ModifiedBits || 0 == ModifiedBits.Count)
                {
                    return;
                }
                foreach (var bit in ModifiedBits)
                {
                    ErrorCode errorCode = ErrorCode.Success;
                    errorCode = _device.WriteBit(bit.Key.Port, bit.Key.Bit, bit.Value);
                    if (ErrorCode.Success != errorCode)
                    {
                        // do nothing.
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return;
        }

        public List<byte> GetMask()
        {
            List<byte> mask = new List<byte>();
            try
            {
                mask = new List<byte>(_device.Features.DoDataMask);
            }
            catch (Exception e)
            {
                MessageManager.Instance().Alarm("IOWriter: " + e.Message);
            }

            return mask;
        }

        public byte GetDirectionMask(int port)
        {
            byte direction = new byte();
            try
            {
                direction= _device.Ports[port].DirectionMask;
            }
            catch (Exception e)
            {
                MessageManager.Instance().Alarm("IOWriter: " + e.Message);
            }

            return direction;
        }

        #region IModule

        public void CloseModule()
        {
            return;
        }

        public void InitModule(string projectDirectory, string nodeName)
        {
            string configFile = projectDirectory + @"\IOWriter-" + nodeName + ".xml";
            string strParamInfo = "";
            if (File.Exists(configFile))
            {
                XmlParameter xmlParameter = new XmlParameter();
                xmlParameter.ReadParameter(configFile);

                for (int i = 0; i < PortCount; ++i)
                {
                    for (int j = 0; j < 8; ++j)
                    {
                        string paramNameOfModifiedBit = "Bit_" + i.ToString() + "_" + j.ToString();
                        strParamInfo = xmlParameter.GetParamData(paramNameOfModifiedBit);
                        if (strParamInfo != "")
                        {
                            ModifiedBits.Add(new BitOfBoard(i, j), Convert.ToByte(strParamInfo));
                        }
                    }
                }
            }

            return;
        }

        public void Run()
        {
            try
            {
                if (null != _device && _device.Initialized)
                {
                    lock (WriterDevice.PadLock)
                    {
                        WriteValue();
                    }
                }
            }
            catch (Exception e)
            {
                MessageManager.Instance().Alarm("IOWriter: " + e.Message);
            }

            return;
        }

        public void SaveModule(string projectDirectory, string nodeName)
        {
            string configFile = projectDirectory + @"\IOWriter-" + nodeName + ".xml";
            XmlParameter xmlParameter = new XmlParameter();

            foreach (var bitInfo in ModifiedBits)
            {
                string key = "Bit_" + bitInfo.Key.Port.ToString() + "_" + bitInfo.Key.Bit.ToString();
                var value = Convert.ToInt32(bitInfo.Value);

                xmlParameter.Add(key, value);
            }

            xmlParameter.WriteParameter(configFile);

            return;
        }

        public bool StartSetForm()
        {
            var form = new StaticDOForm(this);
            form.ShowDialog();
            form.Close();

            return true;
        }

        #endregion
    }
}
