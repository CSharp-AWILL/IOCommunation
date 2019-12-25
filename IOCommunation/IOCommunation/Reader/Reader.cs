using System;
using System.Collections.Generic;
using System.IO;
using Aqrose.Framework.Core.Attributes;
using Aqrose.Framework.Core.DataType;
using Aqrose.Framework.Core.Interface;
using Aqrose.Framework.Utility.MessageManager;
using Aqrose.Framework.Utility.Tools;
using Automation.BDaq;

namespace IOCommunation.Reader
{
    [Module("Reader", "IOCommunation", "")]
    public class Reader : ModuleData, IModule
    {
        private InstantDiCtrl _device = null;
        private byte _bufferA = 0x00;
        private byte _bufferB = 0x00;
        private byte _olderBufferA = 0x00;
        private byte _olderBufferB = 0x00;
        private const int _cycles = 10;

        //
        [OutputData]
        public bool IsOK { get; set; } = false;
        [OutputData]
        public string Result
        {
            get { return IsOK ? "OK" : "NG"; }
        }

        public Dictionary<BitOfBoard, ReaderDevice.EStatus> MonitoredBits = new Dictionary<BitOfBoard, ReaderDevice.EStatus>();

        public Reader()
        {
            _device = ReaderDevice.GetInstance();
        }

        private bool TryReadBit(int port, int bit, out byte data)
        {
            int times = 0;
            byte oldData = 0;
            data = 0;
            
            try
            {
                if (null == _device || (!_device.Initialized))
                {
                    return false;
                }
                if (ErrorCode.Success != _device.ReadBit(port, bit, out data))
                {
                    return false;
                }
                oldData = data;
                do
                {
                    ++times;
                    if ((ErrorCode.Success == _device.ReadBit(port, bit, out data)) && (data == oldData))
                    {
                    }
                    else
                    {
                        return false;
                    }
                } while (times <= _cycles);
            }
            catch (Exception e)
            {
                MessageManager.Instance().Alarm("IOReader: " + e.Message);

                return false;
            }

            return true;
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
                MessageManager.Instance().Alarm("IOReader: "+e.Message);

                return false;
            }

            return true;
        }

        public bool CheckBit(BitOfBoard bitOfBoard, ReaderDevice.EStatus status)
        {
            byte dataOfBit = 0x00;
            byte dataOfPort = 0x00;
            byte oldDataOfPort = 0x00;
            if (0 == bitOfBoard.Port)
            {
                dataOfPort = _bufferA;
                oldDataOfPort = _olderBufferA;
            }
            else if (1 == bitOfBoard.Port)
            {
                dataOfPort = _bufferB;
                oldDataOfPort = _olderBufferB;
            }
            else
            {
                return false;
            }
            switch (status)
            {
                case ReaderDevice.EStatus.LOW:
                    return (TryReadBit(bitOfBoard.Port, bitOfBoard.Bit, out dataOfBit) && (0 == dataOfBit));
                case ReaderDevice.EStatus.HIGH:
                    return (TryReadBit(bitOfBoard.Port, bitOfBoard.Bit, out dataOfBit) && (1 == dataOfBit));
                case ReaderDevice.EStatus.RISING:
                    return ((0x00 == (0x1 & (oldDataOfPort >> bitOfBoard.Bit))) && (0x01 == (0x1 & (dataOfPort >> bitOfBoard.Bit))));
                case ReaderDevice.EStatus.FALLING:
                    return ((0x01 == (0x1 & (oldDataOfPort >> bitOfBoard.Bit))) && (0x00 == (0x1 & (dataOfPort >> bitOfBoard.Bit))));
                default:
                    return false;
            }
        }

        #region IModule

        public void CloseModule()
        {
            return;
        }

        public void InitModule(string projectDirectory, string nodeName)
        {
            string configFile = projectDirectory + @"\IOReader-" + nodeName + ".xml";
            string strParamInfo = "";
            if (File.Exists(configFile))
            {
                XmlParameter xmlParameter = new XmlParameter();
                xmlParameter.ReadParameter(configFile);

                for (int i = 0; i < ReaderDevice.PortCount; ++i)
                {
                    for (int j = 0; j < ReaderDevice.ChanelCount; ++j)
                    {
                        string paramNameOfModifiedBit = "Bit_" + i.ToString() + "_" + j.ToString();
                        strParamInfo = xmlParameter.GetParamData(paramNameOfModifiedBit);
                        if (strParamInfo != "")
                        {
                            MonitoredBits.Add(new BitOfBoard(i, j), (ReaderDevice.EStatus)Convert.ToInt32(strParamInfo));
                        }
                    }
                }
            }

            return;
        }

        public void Run()
        {
            IsOK = true;
            if (null != _device && _device.Initialized)
            {
                _olderBufferA = _bufferA;
                _olderBufferB = _bufferB;
                if (TryReadPort(0, out _bufferA) && TryReadPort(1, out _bufferB))
                {
                    foreach (var bitInfo in MonitoredBits)
                    {
                        IsOK &= CheckBit(bitInfo.Key, bitInfo.Value);
                    }
                }
                else
                {
                    _bufferA = _olderBufferA;
                    _bufferB = _olderBufferB;
                    IsOK = false;
                }
            }
            else
            {
                IsOK = false;
            }

            return;
        }

        public void SaveModule(string projectDirectory, string nodeName)
        {
            string configFile = projectDirectory + @"\IOReader-" + nodeName + ".xml";
            XmlParameter xmlParameter = new XmlParameter();

            foreach (var bitInfo in MonitoredBits)
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
            var form = new StaticDIForm(this);
            form.ShowDialog();
            form.Close();

            return true;
        }

        #endregion

    }
}
