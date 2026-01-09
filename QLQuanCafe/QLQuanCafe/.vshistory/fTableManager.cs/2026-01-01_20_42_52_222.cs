using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLQuanCafe
{
    public partial class fTableManager : Form
    {
        string currentUserName;

        public fTableManager()
        {
            InitializeComponent();
            currentUserName = userName;
        }

        private void fTableManager_Load(object sender, EventArgs e)
        {

        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAdmin f = new fAdmin();
            f.ShowDialog();
        }

        private void smnThongTinCaNhan_Click(object sender, EventArgs e)
        {
            formAccountProfile f = new formAccountProfile();
            f.ShowDialog();
        }

        private void tsmnDangXuat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
