using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PorcelainRingInspector.IOCommunation.Reader
{
    public partial class ReaderForm : Form
    {
        private OuterInspectorForCircle _reader = new OuterInspectorForCircle();
        public ReaderForm(OuterInspectorForCircle reader)
        {
            _reader = reader;
            InitializeComponent();
        }
    }
}
