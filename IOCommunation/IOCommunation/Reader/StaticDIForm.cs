using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IOCommunation.Reader
{
    public partial class StaticDIForm : Form
    {
        #region 
        private Label[] m_portNum;
        private Label[] m_portHex;
        private PictureBox[,] m_pictrueBox;
        private readonly Color[] colors = { Color.Yellow, Color.Blue, Color.Red, Color.Green };
        private const int m_startPort = 0;
        private const int m_portCountShow = 4;
        #endregion

        private Reader _reader = null;
        public StaticDIForm(Reader reader)
        {
            _reader = reader;
            InitializeComponent();
        }

        private void InstantDiForm_Load(object sender, EventArgs e)
        {
            //this.Text = "Static DI(" + Reader.Device.SelectedDevice.Description + ")";

            m_portNum = new Label[m_portCountShow] { PortNum0, PortNum1, PortNum2, PortNum3 };
            m_portHex = new Label[m_portCountShow] { PortHex0, PortHex1, PortHex2, PortHex3 };
            m_pictrueBox = new PictureBox[m_portCountShow, 8]{
                {pictureBox00, pictureBox01, pictureBox02, pictureBox03, pictureBox04, pictureBox05,pictureBox06, pictureBox07},
                {pictureBox10, pictureBox11, pictureBox12, pictureBox13, pictureBox14, pictureBox15,pictureBox16, pictureBox17},
                {pictureBox20, pictureBox21, pictureBox22, pictureBox23, pictureBox24, pictureBox25,pictureBox26, pictureBox27},
                {pictureBox30, pictureBox31, pictureBox32, pictureBox33, pictureBox34, pictureBox35,pictureBox36, pictureBox37}
            };

            Initialize();

            return;
        }

        private void Initialize()
        {
            for (int i = 0; i < ReaderDevice.PortCount; ++i)
            {
                if (!_reader.TryReadPort(i, out var port))
                {
                    port = 0;
                }
                m_portNum[i].Text = i.ToString();
                m_portHex[i].Text = port.ToString("X2");
                for (int j = 0; j < ReaderDevice.ChanelCount; ++j)
                {
                    var bit = new BitOfBoard(i, j);
                    var defaultBitInfo = new KeyValuePair<BitOfBoard, ReaderDevice.EStatus>(bit, 0);

                    m_pictrueBox[i, j].Click += new EventHandler(PictureBox_Click);
                    if (_reader.MonitoredBits.ContainsKey(bit))
                    {
                        m_pictrueBox[i, j].Tag = new KeyValuePair<BitOfBoard, ReaderDevice.EStatus>(bit, _reader.MonitoredBits[bit]);
                    }
                    else
                    {
                        m_pictrueBox[i, j].Tag = defaultBitInfo;
                    }
                    
                    m_pictrueBox[i, j].Image = imageList1.Images[(port >> j) & 0x1];
                    m_pictrueBox[i, j].Invalidate();
                }
            }

            HighLightBox();

            return;
        }

        private void RefreshBox()
        {
            for (int i = 0; i < ReaderDevice.PortCount; ++i)
            {
                if (!_reader.TryReadPort(i, out var data))
                {
                    continue;
                }
                m_portHex[i].Text = data.ToString("X2");
                for (int j = 0; j < ReaderDevice.ChanelCount; ++j)
                {
                    m_pictrueBox[i, j].Image = imageList1.Images[(data >> j) & 0x1];
                    m_pictrueBox[i, j].Invalidate();
                }
            }

            HighLightBox();

            return;
        }

        private void HighLightBox()
        {
            for (int i = 0; i < ReaderDevice.PortCount; ++i)
            {
                for (int j = 0; j < ReaderDevice.ChanelCount; ++j)
                {
                    var bit = new BitOfBoard(i, j);
                    m_pictrueBox[i, j].BackColor = _reader.MonitoredBits.ContainsKey(bit) ? colors[Convert.ToInt32(_reader.MonitoredBits[bit])] : Color.Transparent;
                    m_pictrueBox[i, j].Invalidate();
                }
            }

            return;
        }

        #region Event

        private void PictureBox_Click(object sender, EventArgs e)
        {
            PictureBox box = (PictureBox)sender;
            var boxInfo = (KeyValuePair<BitOfBoard, ReaderDevice.EStatus>)box.Tag;
            BitOfBoard bit = new BitOfBoard(boxInfo.Key.Port, boxInfo.Key.Bit);
            int temp = Convert.ToInt32(boxInfo.Value);
            if (_reader.MonitoredBits.ContainsKey(bit))
            {
                temp = (temp + 1) % 4;
            }
            else
            {
                temp = 0;
            }
            var bitValue = (ReaderDevice.EStatus)temp;
            _reader.MonitoredBits[bit] = bitValue;
            box.Tag = new KeyValuePair<BitOfBoard, ReaderDevice.EStatus>(bit, bitValue);
            box.BackColor = colors[temp];
            box.Invalidate();

            return;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ReaderDevice.PortCount; ++i)
            {
                for (int j = 0; j < ReaderDevice.ChanelCount; ++j)
                {
                    var bit = new BitOfBoard(i, j);
                    var defaultBitInfo = new KeyValuePair<BitOfBoard, ReaderDevice.EStatus>(bit, 0);
                    m_pictrueBox[i, j].Tag = defaultBitInfo;
                    m_pictrueBox[i, j].Invalidate();
                }
            }
            //
            var tempBitInfos = new Dictionary<BitOfBoard, ReaderDevice.EStatus>(_reader.MonitoredBits);
            foreach (var bitInfo in tempBitInfos)
            {
                _reader.MonitoredBits[bitInfo.Key] = 0;
            }
            _reader.MonitoredBits.Clear();
            //
            HighLightBox();

            return;
        }

        private void buttonOfRunTest_Click(object sender, EventArgs e)
        {
            _reader.Run();
            RefreshBox();

            return;
        }

        #endregion
    }
}