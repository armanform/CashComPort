using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace MonetaPriemnik
{
    public partial class Form1 : Form
    {
     
        //keletin byte-tar
        static byte[] strByte;
        
        // VAJNO!!!bul bukil tusken akwa, osi zat sizge kerek boladi
        static int totalMoney = 0;

        //string to output textBox
        string texBoxStr = null;

        //flag
        bool isError = false;

        public Form1()
        {
            InitializeComponent();
        }


        // open connection for cash acceptor
        private void button1_Click(object sender, EventArgs e)
        {
            serialPort1.PortName = "COM21";
            serialPort1.BaudRate = 9600;
            serialPort1.DataBits = 8;
            serialPort1.StopBits = System.IO.Ports.StopBits.One;
            serialPort1.Parity = System.IO.Ports.Parity.None;
            
            if (!serialPort1.IsOpen)
            {
                serialPort1.Open();
            }
            if (serialPort1.IsOpen)
            {
                button1.Enabled = false;
                textBox1.ReadOnly = false;
            }
        }

        //stop serial port and close the app
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen) serialPort1.Close();
            if (serialPort2.IsOpen) serialPort2.Close();
        }

        // process received data from serial port
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            totalMoney = serialPort1.ReadByte();

            if (totalMoney == 4 || totalMoney == 10 || totalMoney == 20)
            {
                this.Invoke(new EventHandler(DisplayText));
            }
        }
        

        //display received data in text box and in label
        private void DisplayText(object sender, EventArgs e)
        {
            if (totalMoney != 0)
            {
                textBox1.AppendText(totalMoney.ToString() + '\n');

                totalMoney = totalMoney * 50;
                label1.Text = totalMoney.ToString();
                totalMoney = 0;
            }

            if (texBoxStr != null)
            {
                textBox1.AppendText(texBoxStr.ToString() + '\n');
            }
        }

        //clear the text box
        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        //payout, sdachi beruge arnalgan button
        private void button2_Click(object sender, EventArgs e)
        {
            serialPort2.PortName = "COM12";
            serialPort2.BaudRate = 9600;
            serialPort2.DataBits = 8;
            serialPort2.StopBits = System.IO.Ports.StopBits.One;
            
            //parity even bolu kerek!!!
            serialPort2.Parity = System.IO.Ports.Parity.Even;

            if (!serialPort2.IsOpen)
            {
                serialPort2.Open();
                reset();
                timer1.Enabled = true;
            }
            if (serialPort2.IsOpen)
            {
                button2.Enabled = false;
            }
        }

        private void serialPort2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            strByte = new byte[serialPort2.BytesToRead];
            serialPort2.Read(strByte, 0, strByte.Length);

            if (strByte.Length >= 4)
            {
                #region Status checking
                if (strByte[3] == 0xAA)
                {
                    texBoxStr = "Payout success";
                }
                else if (strByte[3] == 0xBB)
                {
                    texBoxStr = "Payout fails";
                    isError = true;
                }
                else if (strByte[3] == 0x00)
                {
                    texBoxStr = "Status fine";
                }
                else if (strByte[3] == 0x01)
                {
                    texBoxStr = "Empty note";
                }
                else if (strByte[3] == 0x02)
                {
                    texBoxStr = "Stock less";
                }
                else if (strByte[3] == 0x03)
                {
                    texBoxStr = "Note jam";
                    isError = true;
                }
                else if (strByte[3] == 0x04)
                {
                    texBoxStr = "Over length";
                    isError = true;
                }
                else if (strByte[3] == 0x05)
                {
                    texBoxStr = "Note not exit";
                    isError = true;
                }
                else if (strByte[3] == 0x06)
                {
                    texBoxStr = "Sensor error";
                    isError = true;
                }
                else if (strByte[3] == 0x07)
                {
                    texBoxStr = "Double not error";
                    isError = true;
                }
                else if (strByte[3] == 0x08)
                {
                    texBoxStr = "Motor error";
                    isError = true;
                }
                else if (strByte[3] == 0x09)
                {
                    texBoxStr = "Dispensing busy";
                }
                else if (strByte[3] == 0x0A)
                {
                    texBoxStr = "Sensor adjusting";
                }
                else if (strByte[3] == 0x0B)
                {
                    texBoxStr = "Cheksum error";
                    isError = true;
                }
                else if (strByte[3] == 0x0C)
                {
                    texBoxStr = "Low power error";
                    isError = true;
                }
                #endregion
                this.Invoke(new EventHandler(DisplayText));
            }
            
            if (isError)
            {
                reset();
                isError = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //munda 1 200 tengelik beredi, oziniz engizesiz go kanwa kerek
            PayoutNTimes(1);
        }


        //payout jasaidi, count-kanwa 200 tengelik beredi
        private void PayoutNTimes(byte count)
        {
            byte checksum = (byte)(21 + count);
            byte[] hex = new byte [6]{ 0x01, 0x10, 0x00, 0x10, count, checksum};
            if (serialPort2.IsOpen)
            {
                serialPort2.Write(hex, 0, 6);
            }
        }

        //reset jasaidi sdachi beretindi
        private void reset()
        {
            byte[] hex = new byte[6] { 0x01, 0x10, 0x00, 0x12, 0x00, 0x23 };
            if (serialPort2.IsOpen)
            {
                serialPort2.Write(hex, 0, 6);
            }
        }

        //status aladi sdachi beretinnen
        private void getStatus()
        {
            byte[] hex = new byte[6] { 0x01, 0x10, 0x00, 0x11, 0x00, 0x22 };
            if (serialPort2.IsOpen)
            {
                serialPort2.Write(hex, 0, 6);
            }
        }

        //arbir 5 secund sain status alip turadi
        private void timer1_Tick(object sender, EventArgs e)
        {
            getStatus();
        }

    }
}
