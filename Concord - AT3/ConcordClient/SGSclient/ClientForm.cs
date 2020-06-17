using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Linq;
using SGSclient;


namespace SGSclient
{
    //The commands for interaction between the server and the client
    public enum Command
    {
        Login,      //Log into the server
        Logout,     //Logout of the server
        Message,    //Send a text message to all the chat clients
        List,       //Get a list of users in the chat room from the server
        Null        //No command
    }

    public partial class SGSclient : Form
    {
        public Socket clientSocket; //The main client socket

        public string strName;      //Name by which the user logs into the room
            
        ArrayList myAL = new ArrayList(); //array list for usernames 

        MergeSort merge = new MergeSort(); //merge sort

        LinkedList<String> myChatLog = new LinkedList<String>(); // Doubly Linked List

        private byte[] byteData = new byte[1024];

        public SGSclient()
        {
            InitializeComponent();
        }

        //Broadcast the message typed by the user to everyone
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.strMessage = txtMessage.Text;
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                //Send it to the server
                clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

                txtMessage.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "Concord CHar: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ConcordChat: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);
                Data msgReceived = new Data(byteData);

                //Accordingly process the message received
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        //for login on user side
                        myAL.Add(msgReceived.strName);
                        lstChatters.Items.Clear();
                        foreach (object name in myAL)
                        {
                            lstChatters.Items.Add(name);
                        }
                        break;
                    case Command.Logout:
                        myAL.Remove(msgReceived.strName);
                        lstChatters.Items.Remove(msgReceived.strName);
                        break;
                    case Command.Message:
                        break;
                    case Command.List:
                        //to retrieve username on other user
                        string[] names = msgReceived.strMessage.Split('*');
                        foreach(string name in names)
                        {
                            myAL.Add(name);
                        }    
                        foreach (object name in myAL)
                        {
                            lstChatters.Items.Add(name);
                        }
                        txtChatBox.Text += "<<<" + strName + " has joined the room>>>\r\n";
                        break;
                }

                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
                {
                    string temp = msgReceived.strMessage;
                    myChatLog.AddLast(temp);
                    txtChatBox.Items.Clear();
                    foreach (string message in myChatLog)
                    {
                        txtChatBox.Items.Add(message);
                    }

                }

                byteData = new byte[1024];

                clientSocket.BeginReceive(byteData,
                                          0,
                                          byteData.Length,
                                          SocketFlags.None,
                                          new AsyncCallback(OnReceive),
                                          null);

            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ConcordChat: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            this.Text = "ConcordChat: " + strName;

            //The user has logged into the system so we now request the server to send
            //the names of all users who are in the chat room
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = strName;
            msgToSend.strMessage = null;

            byteData = msgToSend.ToByte();

            clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

            byteData = new byte[1024];
            //Start listening to the data asynchronously
            clientSocket.BeginReceive(byteData,
                                       0,
                                       byteData.Length,
                                       SocketFlags.None,
                                       new AsyncCallback(OnReceive),
                                       null);

        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {
            if (txtMessage.Text.Length == 0)
                btnSend.Enabled = false;
            else
                btnSend.Enabled = true;
        }

        private void SGSClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to leave the chat room?", "ConcordChat: " + strName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            try
            {
                //Send a message to logout of the server
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Logout;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte();
                clientSocket.Send(b, 0, b.Length, SocketFlags.None);
                clientSocket.Close();
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ConcordChat: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //press enter to send a message
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(sender, null);
            }
        }

        //search users
        private void btnSearch_Click(object sender, EventArgs e)
        {
            Object myobj = txtSearch.Text;
            BinarySearch(myAL, myobj);
        }

        //sort button
        private void btnSort_Click_1(object sender, EventArgs e)
        {
            string[] temp = (string[])myAL.ToArray(typeof(string));
            merge.sort(temp);

            lstChatters.Items.Clear();
            myAL.Clear();
            foreach (string name in temp)
            {
                myAL.Add(name);
            }
            foreach (object name in myAL)
            {
                lstChatters.Items.Add(name);
            }
            btnSearch.Enabled = true;
        }

        //binary search
        public void BinarySearch(ArrayList myList, Object myObject)
        {
            int myIndex = myList.BinarySearch(myObject);
            if (myIndex < 0)
                MessageBox.Show("Item cannot be found");
            else
                lstChatters.SelectedIndex = myIndex;
        }

        //The data structure by which the server and the client interact with 
        //each other
        public class Data
        {
            //Default constructor
            public Data()
            {
                this.cmdCommand = Command.Null;
                this.strMessage = null;
                this.strName = null;
            }

            //Converts the bytes into an object of type Data
            public Data(byte[] data)
            {
                //The first four bytes are for the Command
                this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

                //The next four store the length of the name
                int nameLen = BitConverter.ToInt32(data, 4);

                //The next four store the length of the message
                int msgLen = BitConverter.ToInt32(data, 8);

                //This check makes sure that strName has been passed in the array of bytes
                if (nameLen > 0)
                    this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
                else
                    this.strName = null;

                //This checks for a null message field
                if (msgLen > 0)
                    this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
                else
                    this.strMessage = null;
            }

            //Converts the Data structure into an array of bytes
            public byte[] ToByte()
            {
                List<byte> result = new List<byte>();

                //First four are for the Command
                result.AddRange(BitConverter.GetBytes((int)cmdCommand));

                //Add the length of the name
                if (strName != null)
                    result.AddRange(BitConverter.GetBytes(strName.Length));
                else
                    result.AddRange(BitConverter.GetBytes(0));

                //Length of the message
                if (strMessage != null)
                    result.AddRange(BitConverter.GetBytes(strMessage.Length));
                else
                    result.AddRange(BitConverter.GetBytes(0));

                //Add the name
                if (strName != null)
                    result.AddRange(Encoding.UTF8.GetBytes(strName));

                //And, lastly we add the message text to our array of bytes
                if (strMessage != null)
                    result.AddRange(Encoding.UTF8.GetBytes(strMessage));

                return result.ToArray();
            }

            public string strName;      //Name by which the client logs into the room
            public string strMessage;   //Message text
            public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(txtChatBox.SelectedIndex == -1)
            {
                MessageBox.Show("No message selected");
            }
            else
            {
                LinkedListNode<string> current = myChatLog.Find(myChatLog.ElementAt(txtChatBox.SelectedIndex));
                myChatLog.AddBefore(current, "- Deleted -");
                myChatLog.Remove(current);
                txtChatBox.Items.Clear();
                foreach(string message in myChatLog)
                {
                    txtChatBox.Items.Add(message);
                }
            }
           
        }
    }
}