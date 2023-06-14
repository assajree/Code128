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
        Code128 _code128 = new Code128(null);
        public Form1()
        {
            InitializeComponent();

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

            var barcode = _code128.Encode(text);
            lblBarcode.Text = barcode;
            txtResult.Text = barcode;

            //var sum = _code128.GetSumCharacter("ËTÇd/*ÉKÇ'ÏÉ0");

        }
    }
}
