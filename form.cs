using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Main_Biometric_upload
{
    public partial class Form1 : Form
    {
        private SqlConnection con = new SqlConnection("Data Source =192.168.103.33; Initial Catalog = Main_Biometric; User Id =mapims; Password=Biometric@2023;");


      //  private SqlConnection con = new SqlConnection("Data Source =localhost; Initial Catalog = Main_Biometric; User Id =mapims; Password=Biometric@2023;");
       
        
        private SqlCommand cmd;
        SqlDataReader dreader;

        private int indexRow;

        private int MaxCount;
        private int CurrentRec;

        private int CurrentData = 0;
        private int TotalData = 0;

        private bool isRunning;

        private Thread th;

        private int intcount = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        private void load()
        {

            con.Open();
            cmd = new SqlCommand("SELECT        TOP (200) ID, c_url, c_interval, d_interval FROM            app_setup WHERE(ID = 2);", con);
            try
            {
                dreader = cmd.ExecuteReader();
                if (dreader.Read())
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        label10.Text = dreader[1].ToString();
                        // label2.Text = dreader[0].ToString();
                    });
                    
                    }
                else
                {
                   // MessageBox.Show(" No Record");
                }
                dreader.Close();
            }
            catch (Exception)
            {
               // MessageBox.Show(" No Record");
            }
            finally
            {
                con.Close();
            }
        }

        private void BindGrid()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.dataGridView1.DataSource = null;
                this.dataGridView1.Rows.Clear();
                this.dataGridView1.Refresh();
                SqlDataAdapter adapt;
                if (con.State != ConnectionState.Open)
                {
                    //System.Windows.Forms.Application.Exit();
                    con.Close();
                    con.Open();
                }
                else
                {
                   
                    try
                    {
                        con.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        //System.Windows.Forms.Application.Exit();
                        if (con.State != ConnectionState.Open)
                        {
                            con.Close();
                            con.Open();
                        }
                        else
                        {
                            System.Windows.Forms.Application.Exit();
                            con.Open();
                        }
                        }
                }
                DataTable dt = new DataTable();
                adapt = new SqlDataAdapter("select  TOP (200) * from   punchtimedetails  WHERE        (status = 0) ORDER BY id ASC", con);
                adapt.Fill(dt);
                DataTable dataTable = new DataTable();
                DataSet dataSet = new DataSet();
                adapt.Fill(dataTable);
                this.dataGridView1.DataSource = dataTable;
                con.Close();
                MaxCount = dataTable.Rows.Count;
                TotalData = dataTable.Rows.Count;
                CurrentRec = 0;
                CurrentData = 0;
            });
            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = TotalData;
                progressBar1.Value = 0;
                progressBar1.Step = 1;
                progressBar1.Minimum = 0;
            });

            this.Invoke((MethodInvoker)delegate {
                label7.Text = "Searching Attence Data from SQL ..... ";
                label7.BackColor = Color.YellowGreen;
                intcount = 0;
                label8.Text = "0";

            });

            callloop();
        }



        private void CustomBindGrid(string dwEnrollNumber,
string device_name,
string status)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.dataGridView1.DataSource = null;
                this.dataGridView1.Rows.Clear();
                this.dataGridView1.Refresh();
                SqlDataAdapter adapt;
                con.Open();
                DataTable dt = new DataTable();


                string aa = "select  * from   punchtimedetails  WHERE   " +
                    "dwEnrollNumber like '%" + dwEnrollNumber + "%'  and device_name like '%" + device_name + "%' and status like '%" + status + "%' and " +
                    " punch_date   between '" + dateTimePicker1.Value.ToString("yyyyMMdd") + "' and '" + dateTimePicker2.Value.ToString("yyyyMMdd") + "'" +
                    "     ORDER BY id ASC";

                adapt = new SqlDataAdapter("select   * from   punchtimedetails  WHERE   " +
                    "dwEnrollNumber like '%" + dwEnrollNumber + "%'  and device_name like '%" + device_name + "%' and status like '%" + status + "%' and " +
                    " punch_date   between '" + dateTimePicker1.Value.ToString("yyyyMMdd") + "' and '" + dateTimePicker2.Value.ToString("yyyyMMdd") + "'" +
                    "     ORDER BY id ASC", con);
                adapt.Fill(dt);
                DataTable dataTable = new DataTable();
                DataSet dataSet = new DataSet();
                adapt.Fill(dataTable);
                this.dataGridView1.DataSource = dataTable;
                con.Close();
                MaxCount = dataTable.Rows.Count;
                TotalData = dataTable.Rows.Count;
                CurrentRec = 0;
                CurrentData = 0;
            });
            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = TotalData;
                progressBar1.Value = 0;
                progressBar1.Step = 1;
                progressBar1.Minimum = 0;
            });

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            isRunning = true;
            th = new Thread(callloop);
            th.Start();
            this.Invoke((MethodInvoker)delegate {
                label7.Text = "Upload Started";
                label7.BackColor = Color.Yellow;
                intcount = 0;
                label8.Text = "0";

            });

            load();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            th = new Thread(callloop);
            th.Start();
            this.Invoke((MethodInvoker)delegate {
                label7.Text = "Upload Started";
                label7.BackColor = Color.Yellow;

                intcount = 0;
                label8.Text = "0";
            });
        }


        private void callloop()
        {
            try {
                isRunning = true;
                if (CurrentData >= TotalData)
                {
                    this.Invoke((MethodInvoker)delegate {
                        progressBar1.Value = CurrentData;
                    });

                    this.Invoke((MethodInvoker)delegate {
                        label7.Text = "Waiting for Data ...... ";
                        label7.BackColor = Color.Yellow;
                        intcount = 0;
                        label8.Text = "0";
                        label6.Text = "0";
                    });

                    CurrentData = 0;
                    Thread.Sleep(5000);
                    if (isRunning == true)
                    {
                        BindGrid();
                    }
                    Thread.Sleep(5000);
                    if (TotalData == 0)
                    {
                        this.Invoke((MethodInvoker)delegate {
                            label7.Text = "Waiting for Data ...... " ;
                            label7.BackColor = Color.Yellow;
                            intcount = 0;
                            label8.Text = "0";
                            label6.Text = "0";
                        });
                    }
                  
                }
                this.Invoke((MethodInvoker)delegate {
                    progressBar1.Value = CurrentData;
                });
                Thread.Sleep(1500);

                int totTotalData = 0;
                this.Invoke((MethodInvoker)delegate {
                    totTotalData = this.dataGridView1.RowCount;
                });
                if (totTotalData > 0)
                {
                    getattloop();
                }
              

              

            } catch { }
        }

        private void getattloop()
        {
            try
            {


                string m_id = "";

                string p_id = "";
string t_machine = "";
                string enroll_no = "";
                string e_machine = "";
                string v_mode = "";
                string punch_dt = "";
                string device_id = "";
                string ip_address = "";
                string tmmm = "";
                string hhmm = "";

                this.Invoke((MethodInvoker)delegate
                {
                    m_id = dataGridView1.Rows[CurrentData].Cells[0].Value.ToString().Trim();
                    p_id = dataGridView1.Rows[CurrentData].Cells[0].Value.ToString().Trim();
                 t_machine = dataGridView1.Rows[CurrentData].Cells[3].Value.ToString().Trim();
                 enroll_no = dataGridView1.Rows[CurrentData].Cells[2].Value.ToString().Trim();
                 e_machine = dataGridView1.Rows[CurrentData].Cells[8].Value.ToString().Trim();
                 v_mode = dataGridView1.Rows[CurrentData].Cells[4].Value.ToString().Trim();


                    //  punch_dt = dataGridView1.Rows[CurrentData].Cells[5].Value.ToString().Trim() + " " + dataGridView1.Rows[CurrentData].Cells[6].Value.ToString().Trim();

                    DateTime theDate = DateTime.Parse(dataGridView1.Rows[CurrentData].Cells[5].Value.ToString());
                    String dateString = theDate.ToString("dd-MM-yy");
                    punch_dt = theDate.ToString("yyyy-MM-dd") + " " + dataGridView1.Rows[CurrentData].Cells[6].Value.ToString().Trim();
                    device_id = dataGridView1.Rows[CurrentData].Cells[9].Value.ToString().Trim();
                 ip_address = dataGridView1.Rows[CurrentData].Cells[10].Value.ToString().Trim();
                    tmmm = "";  // dataGridView1.Rows[CurrentData].Cells[5].Value.ToString();
                 hhmm = "";  // dataGridView1.Rows[CurrentData].Cells[6].Value.ToString();
                });
            upload(p_id,
 t_machine,
 enroll_no,
 e_machine,
 v_mode,
 punch_dt,
 device_id,
 ip_address,
 tmmm,
 hhmm, m_id);
            }
            catch { }
        }
       


        private async void upload(string p_id,
string t_machine,
string enroll_no,
string e_machine,
string v_mode,
string punch_dt,
string device_id,
string ip_address,
string tmmm,
string hhmm, string m_id)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label6.Text = "0";
                });
                string hhh = (label10.Text + "/webapi.php?p_id=" + p_id + "&t_machine=" + t_machine +
                    "&enroll_no=" + enroll_no + "&e_machine=" + e_machine + "&v_mode=" + v_mode +
                    "&punch_dt=" + punch_dt + "&device_id=" + device_id + "&ip_address=" + ip_address +
                    "&tmmm=" + tmmm + "&hhmm=" + hhmm);

                var client = new HttpClient();
                client.BaseAddress = new Uri(label10.Text + "/webapi.php");
                HttpResponseMessage response = await client.GetAsync("?p_id=" + p_id + "&t_machine=" + t_machine +
                    "&enroll_no=" + enroll_no + "&e_machine=" + e_machine + "&v_mode=" + v_mode +
                    "&punch_dt=" + punch_dt + "&device_id=" + device_id + "&ip_address=" + ip_address +
                    "&tmmm=" + tmmm + "&hhmm=" + hhmm);
                string result = await response.Content.ReadAsStringAsync();
                this.Invoke((MethodInvoker)delegate
                {
                    label6.Text = result;
                });
                if (result == "1")
                {
                    insertpunchdetails(m_id);
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        label7.Text = "Fail to Upload,       " + CurrentData + "   ---   " + TotalData;
                        label7.BackColor = Color.Red;
                        intcount = 0;
                        label8.Text = "0";
                    });
                    CurrentData = CurrentData + 1;
                    callloop();
                }
            }
            catch
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label7.Text = "Fail to Upload,       " + CurrentData + "   ---   " + TotalData;
                    label7.BackColor = Color.Red;
                    intcount = 0;
                    label8.Text = "0";
                });
                CurrentData = CurrentData + 1;
                callloop();
            }
        }

        private void uploadsql()
        {

        }

        private void insertpunchdetails(string data_id)
        {
          
            this.con.Open();
           

            string[] deviceName = new string[] {
       " UPDATE punchtimedetails SET status = 1 " +
       " WHERE ID = '" + data_id + "';"

   };


            //   string kk = string.Concat(deviceName);

            cmd = new SqlCommand(string.Concat(deviceName), con);
            if (this.cmd.ExecuteNonQuery() <= 0)
            {
                this.con.Close();
                this.Invoke((MethodInvoker)delegate {
                    label7.Text = "Fail to Update,       " + CurrentData + "   ---   " + TotalData;
                    label7.BackColor = Color.Red;
                    label6.Text = "0";
                    intcount = 0;
                    label8.Text = "0";
                });

                CurrentData = CurrentData + 1;
                callloop();

            }
            else
            {
                this.con.Close();
                this.Invoke((MethodInvoker)delegate {
                    label7.Text = "Updated Successfuly    " + CurrentData + "   ---   " + TotalData;
                    label7.BackColor = Color.Green;
                    intcount = 0;
                    label8.Text = "0";
                    label6.Text = "0";
                });

                CurrentData = CurrentData + 1;
                callloop();

                // this.BindGrid();
            }
            this.con.Close();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            if (dateTimePicker1.Value.ToString() == "")
            {
                MessageBox.Show("Please Select From Date...");
                return;
            }

            if (dateTimePicker2.Value.ToString() == "")
            {
                MessageBox.Show("Please Select To Date...");
                return;
            }

            CustomBindGrid(textBox1.Text,
textBox2.Text,
textBox3.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            th = new Thread(callloop);
            th.Start();

            isRunning = true;

            this.Invoke((MethodInvoker)delegate {
                label7.Text = "Started";
                label7.BackColor = Color.Yellow;
                intcount = 0;
                label8.Text = "0";

            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isRunning = false;
            th.Abort();

            this.Invoke((MethodInvoker)delegate {

                this.dataGridView1.DataSource = null;
                this.dataGridView1.Rows.Clear();
                this.dataGridView1.Refresh();

                label7.Text = "Stopped";
                label7.BackColor = Color.Yellow;
                intcount= 0;
                label8.Text = "0";

            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            intcount++;
            if(checkBox1.Checked == true)
            {
               // if(intcount> 400)
               // {
                    Application.Exit();

                    Environment.Exit(0);

                    //  Process.GetCurrentProcess().Kill();

                    System.Windows.Forms.Application.ExitThread();
                    System.Environment.Exit(0);
                    this.Close();
                    System.Windows.Forms.Application.Exit();

               // }
            }
            label8.Text = Convert.ToString(intcount);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
        }
    }
}
