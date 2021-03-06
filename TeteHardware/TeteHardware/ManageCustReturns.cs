﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace TeteHardware
{
    public partial class ManageCustReturns : Form
    {
        public formAfterLogin ReferenceToAfterLogin { get; set; } //Reference formEmployeeManage to this form
        public MySqlConnection conn; //connection

        Test func = new Test();
        public ManageCustReturns()
        {
            InitializeComponent();
            conn = new MySqlConnection("Server=localhost;Database=tetehardware;Uid=root;Pwd=root"); //connection
            this.Opacity = 0;
            timer1.Start();
        }

        private void ManageCustReturns_Load(object sender, EventArgs e)
        {
            txtCalReturn.Text = DateTime.Now.ToShortDateString();
            calReturned.Location = new Point(342, 137);
            calReturned.MaxDate = DateTime.Now;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Opacity += 0.1; //form transition using timer
        }
        bool mouseDown; //boolean for mousedown
        Point lastLocation; //variable for the last location of the mouse

        private void ManageCustReturns_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true; //sets mousedown to true
            lastLocation = e.Location; //gets the location of the form and sets it to lastlocation
        }

        private void ManageCustReturns_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown) //if mouseDown is true, point to the last location of the mouse
            {
                this.Location = new Point((this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y); //gets the coordinates of the location of the mouse
                this.Update(); //updates the location of the mouse
            }
        }

        private void ManageCustReturns_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false; //sets mousedown to false
        }

        //Buttons Handling - put all codes for any buttons here
        private void btnClose_Click(object sender, EventArgs e)
        {
            ReferenceToAfterLogin.Show();
            this.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtTrans.Text == "" || txtCust.Text == "" || txtCalReturn.Text == "" || txtQty.Text == "") //DATA VALIDATION
            {
                MessageBox.Show("Please supply all necessary fields.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning); //shows a message box if textboxes are blank
            }
            else
            {
                saveToDatabase();
            }
            
        }

        private void txtCalReturn_Click(object sender, EventArgs e)
        {
            calReturned.Show();
        }

        //textbox handling
        private void txtQty_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(txtQty.Text, "  ^ [0-9]")) //textbox only accepts numbers
            {
                txtQty.Text = "";
            }
        }
        private void txtQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.')) //with decimals
            {
                e.Handled = true;
            }
        }
        private void calReturned_DateSelected(object sender, DateRangeEventArgs e)
        {
            txtCalReturn.Text = calReturned.SelectionRange.Start.ToShortDateString();
            calReturned.Hide();
        }

        private void txtQty_Leave(object sender, EventArgs e)
        {
            int myRowIndex = dataGridProduct.CurrentRow.Index;
            try
            {

                if (int.Parse(txtQty.Text) > int.Parse(dataGridProduct.Rows[myRowIndex].Cells["Quantity"].Value.ToString()))
                {
                    MessageBox.Show("Quantity returned is greater than the defective quantity!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //txtQty.Focus();
                    txtQty.SelectAll();
                }
            }
            catch (Exception) { }
        }
        private void populateDataGridProducts()
        {
            dataGridProduct.DataSource = null;      //remove datasource link for datagridProduct
            try
            {
                conn.Open(); //opens the connection
                MySqlCommand query = new MySqlCommand("SELECT a.transDate AS 'Transaction Date', a.prodID AS 'Product ID', b.prodName AS 'Product Name', a.transQty AS 'Quantity' FROM tbl_transact a, tbl_product b WHERE a.transNum = '" + txtTrans.Text + "' AND a.prodID = b.prodID", conn); 
                MySqlDataAdapter adp = new MySqlDataAdapter(query); //adapter for query
                DataTable dt = new DataTable(); //datatable for adapter
                BindingSource bs = new BindingSource();
                dt.Locale = System.Globalization.CultureInfo.InvariantCulture;
                adp.Fill(dt);
                bs.DataSource = dt;
                dataGridProduct.DataSource = bs;
                conn.Close();
                dataGridProduct.AutoResizeColumns();
                dataGridProduct.ClearSelection();
                dataGridProduct.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
            catch (Exception x)
            {
                MessageBox.Show("Error in populating datagridTable : " + x.ToString());
                conn.Close();
            }
        }

        private void saveToDatabase()
        {
            //insert to tbl_returnfrom
            int myRowIndex = dataGridProduct.CurrentRow.Index;
            try
            {
                conn.Open();
                MySqlCommand query = new MySqlCommand("INSERT INTO tbl_returnfrom(transNum, prodID, custName, retQty, retDate, retDefect, empID) VALUES('" + txtTrans.Text + "','" + dataGridProduct.Rows[myRowIndex].Cells["Product ID"].Value.ToString() + "','" + txtCust.Text + "','" + txtQty.Text + "','" + txtCalReturn.Text + "', '" + txtRemarks.Text + "', " + TeteHardware.Properties.Settings.Default.loginID + ")", conn);
                query.ExecuteNonQuery();
                conn.Close();
                func.ChangeLog("tbl_returnfrom", "All", "None");
                MessageBox.Show("Added Successfully!", "", MessageBoxButtons.OK);
            }
            catch (Exception x)
            {
                MessageBox.Show("Error in Add() :" + x.ToString());
                conn.Close();
            }
            //refresh dataGridProduct
            populateDataGridProducts();
            txtCalReturn.Text = "";
            txtRemarks.Text = "";
            txtQty.Text = "0";

        }

        private void Strto2DecPlaces(TextBox myTextbox)
        {
            if (myTextbox.Text.Contains("."))
            {
                myTextbox.Text = Convert.ToString(decimal.Round(decimal.Parse(myTextbox.Text + "000"), 2));
            }
            else
            {
                myTextbox.Text = Convert.ToString(decimal.Round(decimal.Parse(myTextbox.Text + ".000"), 2));
            }
        }

        private void txtTrans_Leave(object sender, EventArgs e)
        {
            populateDataGridProducts();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)     //Close Window
            {
                ReferenceToAfterLogin.Show();
                this.Dispose();
            }
            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
