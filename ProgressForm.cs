using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Auto_Serial_Downloader
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual; // 由我们手动设置位置
            TopMost = true;                           // 永远在最上层
            Opacity = 0.70;                           // 半透明(0~1)
        }
    }
}
