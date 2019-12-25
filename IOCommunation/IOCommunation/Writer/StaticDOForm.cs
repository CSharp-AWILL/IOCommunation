using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace IOCommunation.Writer
{
    public partial class StaticDOForm : Form
    {
        #region
        private Label[] m_portNum;
        private Label[] m_portHex;
        private PictureBox[,] m_pictrueBox;
        private readonly Color[] colors = { Color.Yellow, Color.Blue };

        #endregion

        private Writer _writer = null;

        public StaticDOForm(Writer writer)
        {
            _writer = writer;
            InitializeComponent();
        }

        private void StaticDOForm_Load(object sender, EventArgs e)
        {
            m_portNum = new Label[ConstVal.PortCountShow] { PortNum0, PortNum1, PortNum2, PortNum3 };
            m_portHex = new Label[ConstVal.PortCountShow] { PortHex0, PortHex1, PortHex2, PortHex3 };
            m_pictrueBox = new PictureBox[ConstVal.PortCountShow, 8]
            {
                {pictureBox00, pictureBox01, pictureBox02, pictureBox03, pictureBox04, pictureBox05,pictureBox06, pictureBox07},
                {pictureBox10, pictureBox11, pictureBox12, pictureBox13, pictureBox14, pictureBox15,pictureBox16, pictureBox17},
                {pictureBox20, pictureBox21, pictureBox22, pictureBox23, pictureBox24, pictureBox25,pictureBox26, pictureBox27},
                {pictureBox30, pictureBox31, pictureBox32, pictureBox33, pictureBox34, pictureBox35,pictureBox36, pictureBox37}
            };

            InitializePortState();
        }

        private void InitializePortState()
        {
            List<byte> masks = _writer.GetMask();
            if (null == masks || masks.Count < _writer.PortCount)
            {
                return;
            }
            for (int indexOfPort = 0; indexOfPort < _writer.PortCount; ++indexOfPort)
            {
                if (!_writer.TryReadPort(indexOfPort, out var data))
                {
                    data = 0;
                }
                m_portNum[indexOfPort].Text = indexOfPort.ToString();
                m_portHex[indexOfPort].Text = data.ToString("X2");
                byte mask = masks[indexOfPort];
                byte directionMask = _writer.GetDirectionMask(indexOfPort);

                for (int indexOfChanel = 0; indexOfChanel < 8; ++indexOfChanel)
                {
                    if (((directionMask >> indexOfChanel) & 0x1) == 0 || ((mask >> indexOfChanel) & 0x1) == 0)
                    {
                        m_pictrueBox[indexOfPort, indexOfChanel].Image = imageList1.Images[2];
                        m_pictrueBox[indexOfPort, indexOfChanel].Enabled = false;
                    }
                    else
                    {
                        _writer.TryReadBit(indexOfPort, indexOfChanel, out var bitValue);
                        m_pictrueBox[indexOfPort, indexOfChanel].Click += new EventHandler(PictureBox_Click);
                        m_pictrueBox[indexOfPort, indexOfChanel].Tag = new DoBitInformation((data >> indexOfChanel) & 0x1, indexOfPort, indexOfChanel);
                        m_pictrueBox[indexOfPort, indexOfChanel].Image = imageList1.Images[(data >> indexOfChanel) & 0x1];
                        m_pictrueBox[indexOfPort, indexOfChanel].Image = Convert.ToBoolean(bitValue) ? imageList1.Images[1] : imageList1.Images[0];
                    }
                    m_pictrueBox[indexOfPort, indexOfChanel].Invalidate();
                }
            }
            HighLightBox();

            return;
        }

        private void RefreshBox()
        {
            for (int i = 0; i < _writer.PortCount; ++i)
            {
                if (!_writer.TryReadPort(i, out var data))
                {
                    continue;
                }
                m_portHex[i].Text = data.ToString("X2");
                for (int j = 0; j < 8; ++j)
                {
                    m_pictrueBox[i, j].Image = imageList1.Images[(data >> j) & 0x1];
                    m_pictrueBox[i, j].Tag = new DoBitInformation((data >> j) & 0x1, i, j);
                    m_pictrueBox[i, j].Invalidate();
                }
            }

            HighLightBox();

            return;
        }

        private void HighLightBox()
        {
            for (int i = 0; i < _writer.PortCount; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    var bit = new BitOfBoard(i, j);
                    if (_writer.ModifiedBits.ContainsKey(bit))
                    {
                        m_pictrueBox[i, j].BackColor = colors[_writer.ModifiedBits[bit]];
                    }
                    else
                    {
                        m_pictrueBox[i, j].BackColor = Color.Transparent;
                    }
                    m_pictrueBox[i, j].Invalidate();
                }
            }

            return;
        }

        #region Event

        private void PictureBox_Click(object sender, EventArgs e)
        {
            PictureBox box = (PictureBox)sender;
            DoBitInformation boxInfo = (DoBitInformation)box.Tag;
            boxInfo.BitValue = (~(int)(boxInfo).BitValue) & 0x1;
            BitOfBoard bit = new BitOfBoard(boxInfo.PortNum, boxInfo.BitNum);
            _writer.ModifiedBits[bit] = Convert.ToByte(boxInfo.BitValue);

            box.Tag = boxInfo;
            //box.Image = imageList1.Images[boxInfo.BitValue];
            box.BackColor = colors[boxInfo.BitValue];
            box.Invalidate();
            //_writer.WriteValue();
            //_writer.TryReadPort(boxInfo.PortNum, out var data);
            //m_portHex[boxInfo.PortNum].Text = data.ToString("X2");

            return;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            Dictionary<BitOfBoard, byte> tempBits = new Dictionary<BitOfBoard, byte>(_writer.ModifiedBits);
            foreach (var bit in tempBits)
            {
                _writer.ModifiedBits[bit.Key]= Convert.ToByte(0);
            }
            _writer.WriteValue();
            _writer.ModifiedBits.Clear();
            RefreshBox();

            return;
        }

        private void buttonOfRunTest_Click(object sender, EventArgs e)
        {
            _writer.Run();
            RefreshBox();

            return;
        }

        #endregion
    }

    public struct DoBitInformation
    {
        #region fields
        private int m_bitValue;
        private int m_portNum;
        private int m_bitNum;
        #endregion

        public DoBitInformation(int bitvalue, int portNum, int bitNum)
        {
            m_bitValue = bitvalue;
            m_portNum = portNum;
            m_bitNum = bitNum;
        }

        #region Properties
        public int BitValue
        {
            get { return m_bitValue; }
            set
            {
            m_bitValue = value & 0x1;
            }
        }
        public int PortNum
        {
            get { return m_portNum; }
            set
            {
            if ((value - ConstVal.StartPort) >= 0
                && (value - ConstVal.StartPort) <= (ConstVal.PortCountShow - 1))
            {
                m_portNum = value;
            }
            }
        }
        public int BitNum
        {
            get { return m_bitNum; }
            set
            {
            if (value >= 0 && value <= 7)
            {
                m_bitNum = value;
            }
            }
        }
        #endregion
    }

    public static class ConstVal
    {
        public const int StartPort = 0;
        public const int PortCountShow = 4;
    }
}
