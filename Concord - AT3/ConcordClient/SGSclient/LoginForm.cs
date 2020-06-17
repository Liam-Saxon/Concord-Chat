using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace SGSclient
{
    public partial class LoginForm : Form
    {
        public Socket clientSocket;
        public string strName;

        public LoginForm()
        {
            InitializeComponent();
            btnOK.Enabled = false;
        }
        //executes User Creation
        public void SimulateUserCreation()
        {
            string userid = txtUsername.Text;

            string password = txtPassword.Text;

            var hash = Hashing.Hash(txtPassword.Text);

            string passwordHash = hash;

            db_Update_Add_Record(userid, password, passwordHash);

        }
        //executes Login 
        public void SimulateLogin()
        {
            //TODO make the login work correctly
            strName = txtUsername.Text;
            string userid = txtUsername.Text;
            string password = txtPassword.Text;
            string passwordHash;

            //sql connection string
            string cn_String = Properties.Settings.Default.connection_String;

            using (SqlConnection cnConnection = new SqlConnection(cn_String))
            {
                //sql statement
                string sSQL = "SELECT * FROM tblDetail WHERE UsernameID=@userid";
                //use statement and connection
                SqlCommand sqlCom = new SqlCommand(sSQL, cnConnection);
                //set up paramter
                sqlCom.Parameters.AddWithValue("@userid", userid);
                //check connection is open
                cnConnection.Open();

                using (SqlDataReader sqlReader = sqlCom.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        userid = sqlReader["UsernameID"].ToString();
                        passwordHash = sqlReader["PasswordHash"].ToString();

                        var hash = passwordHash;

                        var result = Hashing.Verify(password, hash);
                        if (result == true)
                        {
                            MessageBox.Show("Username and password verifided. you may now connect to a server");
                            btnOK.Enabled = true;
                           
                        }
                        else
                        {
                            MessageBox.Show("Password or Username is incorrect");
                            btnOK.Enabled = false;
                        }
                    }
                    cnConnection.Close();
                    
                }

            }

        }
        //executes the adding of User data to database ( use in conjunction with User Creation)
        private void db_Update_Add_Record(string Username, string Password, string PasswordHash)
        {
            //--------< db_Update_Add_Record() >--------
            //add Record
            //< correct>
            Username = Username.Replace("'", "''");
            Password = Password.Replace("'", "''");
            PasswordHash = PasswordHash.Replace("'", "''");


            //</ correct>
            //< find record >
            string sSQL = "SELECT * FROM tblDetail";
            System.Data.DataTable tbl = Database.Get_DataTable(sSQL);

            //< add >
            string sql_Add = "INSERT INTO tblDetail" + "(UsernameID,Password,PasswordHash)" + "VALUES('" + Username + "','" + Password + "','" + PasswordHash + "')";
            Database.Execute_SQL(sql_Add);
            MessageBox.Show("User created.");
            //</ add >

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ipAddress = IPAddress.Parse(txtServerIP.Text);
                //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                //Connect to the server
                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message, "SGSclient", MessageBoxButtons.OK, MessageBoxIcon.Error); 
             } 
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
                strName = txtUsername.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclient", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);

                //We are connected so we login into the server
                SGSclient.Data msgToSend = new SGSclient.Data();
                msgToSend.cmdCommand = Command.Login;
                msgToSend.strName = txtUsername.Text;
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte ();

                //Send the message to the server
                clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message, "SGSclient", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (txtUsername.Text.Length > 0 && txtServerIP.Text.Length > 0)
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }

        private void txtServerIP_TextChanged(object sender, EventArgs e)
        {
            if (txtUsername.Text.Length > 0 && txtServerIP.Text.Length > 0)
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if(txtUsername.Text == null && txtPassword.Text == null)
            {
                MessageBox.Show("Username and password are empty...");
            }
            else
            {
                SimulateLogin();
            }
           
        }

        private void btnCreateUser_Click(object sender, EventArgs e)
        {
            SimulateUserCreation();
        }
    }
}