using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//Included for Excel and comports
using System.Data.OleDb;
using System.IO;
using System.IO.Ports;
using System.Threading;




namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        private string Excel03ConString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;HDR={1}'";
        private string Excel07ConString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 8.0;HDR={1}'";

        public Form1()
        {
            InitializeComponent();        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            var ports = SerialPort.GetPortNames();
            btnOpen.Enabled = true;
            btnClose.Enabled = false;
            btnWrite.Enabled = false;
            btnRead.Enabled = false;

            cmbComs.DataSource = ports;           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }
  
        private void openFileDialog1_FileOk_1(object sender, CancelEventArgs e)
        {
            string filePath = openFileDialog1.FileName;
            string extension = Path.GetExtension(filePath);
            //string header = rbHeaderYes.Checked ? "YES" : "NO";
            string header = "NO";
            string conStr, sheetName;
            conStr = string.Empty;
            switch (extension)
            {

                case ".xls": //Excel 97-03
                    conStr = string.Format(Excel03ConString, filePath, header);
                    break;

                case ".xlsx": //Excel 07
                    conStr = string.Format(Excel07ConString, filePath, header);
                    break;
            }

            //Get the name of the First Sheet.
            using (OleDbConnection con = new OleDbConnection(conStr))
            {
                using (OleDbCommand cmd = new OleDbCommand())
                {
                    cmd.Connection = con;
                    con.Open();
                    DataTable dtExcelSchema = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                    con.Close();
                }
            }

            //Read Data from the First Sheet.
            using (OleDbConnection con = new OleDbConnection(conStr))
            {
                using (OleDbCommand cmd = new OleDbCommand())
                {
                    using (OleDbDataAdapter oda = new OleDbDataAdapter())
                    {
                        DataTable dt = new DataTable();
                        cmd.CommandText = "SELECT * From [" + sheetName + "]";
                        cmd.Connection = con;
                        con.Open();
                        oda.SelectCommand = cmd;
                        oda.Fill(dt);
                        con.Close();

                        //Populate DataGridView.
                        dataGridView1.DataSource = dt;
                    }
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if(cmbComs.Text!="")
            {
                ArduinoCom.PortName = cmbComs.Text;
                ArduinoCom.Open();

                btnOpen.Enabled = false;
                btnClose.Enabled = true;
                btnWrite.Enabled = true;
                btnRead.Enabled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            ArduinoCom.Close();

            btnOpen.Enabled = true;
            btnClose.Enabled = false;
            btnWrite.Enabled = false;
            btnRead.Enabled = false;
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            string SendString="";
            string  Number;
            string  mem;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    mem = row.Cells[0].Value.ToString();
                    Number = row.Cells[1].Value.ToString();
                    SendString = "WRITEAT" + Number + '-' + mem  +'>'+'\n';
                    ArduinoCom.Write(SendString);
                    Thread.Sleep(100);
                }
                
            }
            if(SendString !="") MessageBox.Show("All phone numbers saved in memory");
            else MessageBox.Show("No phone numbers to write");

        }

        private void btnRead_Click(object sender, EventArgs e)
        {

        }
    }
}
