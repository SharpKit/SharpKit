using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpKit.Installer
{
    public partial class DialogForm : Form
    {
        public DialogForm()
        {
            InitializeComponent();
            //ShowInTaskbar = false;
        }
        public FlowLayoutPanel FlowLayoutPanel1 { get { return flowLayoutPanel1; } }

        public TextBox TextBox1 { get { return textBox1; } }
    }

}
