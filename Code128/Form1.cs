using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSI.Code128;

namespace CSI.Code128
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var code128 = new Code128(null);

            //txtText.Text = code128.GetAllCharacter();
            Encode();
        }

        private void btnEncode_Click(object sender, EventArgs e)
        {
            Encode();
        }

        private void Encode()
        {
            var text = txtText.Text;

            var code128 = new Code128(text);
            lblBarcode.Text = code128.ToString();
            txtResult.Text = code128.ToString();

            //var code128 = FlexCode128.GenBarcodeEncode(text);
            //lblBarcode.Text = code128;
            //txtResult.Text = code128;
        }
    }
}
