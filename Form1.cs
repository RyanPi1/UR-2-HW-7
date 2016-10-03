/*HW 7 Lane Following Race C# Code
    Programmer: Ryan Pizzirusso
    Due Date: Oct. 5, 2016*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.IO.Ports;

namespace HW_7_PC_Side
{
    public partial class Form1 : Form
    {
        //globals for Vision
        Capture capture = null;
        int cannyThresh;
        int cannyLinkThresh;
        int heightThird;
        int halfWidth;

        //globals for serial
        SerialPort sp1 = new SerialPort();
        byte[] send = new byte[2];
            //sends two byte representing a ratio of distance to edge on left/right

        //other globals
        bool running;

        public Form1()
        {
            InitializeComponent();
        }//end constructor

        private void Form1_Load(object sender, EventArgs e)
        {/*code to run on startup*/
            //initialize vision
            capture = new Capture(0);
            capture.ImageGrabbed += Disp_Captured;
            capture.Start();

            //calculate constants for image processing
            heightThird = imageBox1.Height / 3;
            halfWidth = imageBox1.Width / 2;

            //initialize serial
            sp1.PortName = textBox1.Text;
            sp1.BaudRate = 115200;
            try
            {
                sp1.Open();
                send[0] = Convert.ToByte(0);
                send[1] = Convert.ToByte(0);
                sp1.Write(send, 0, 2);
            }
            catch
            {
                label3.Visible = true;
            }

            //state initialization
            running = false;

            //initialize canny variables
            cannyThresh = trackBar1.Value;
            label1.Text = cannyThresh.ToString();

            cannyLinkThresh = trackBar2.Value;
            label2.Text = cannyLinkThresh.ToString();
        }//end Load Event Handler

        public void Disp_Captured(object sender, EventArgs e)
        {
            //save original captured image and display in imageBox1
            Image<Bgr, Byte> img = capture.RetrieveBgrFrame().Resize(imageBox1.Width, imageBox1.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            imageBox1.Image = img;

            //convert to HSV
            Image<Hsv, Byte> HSVimg = img.Convert<Hsv, Byte>();

            //convert to grayscale, then canny, then display edges in imageBox2
            Image<Gray, Byte> cannyImg = img.Convert<Gray, Byte>().PyrDown().PyrUp();
            cannyImg = cannyImg.Canny(cannyThresh, cannyLinkThresh);
            imageBox2.Image = cannyImg;

            int currentLeftDist, currentRightDist;
            int leftDistSum, rightDistSum;
            int avgLeftDist, avgRightDist;
            int i;

            if (running)
            {
                leftDistSum = rightDistSum = 0; //initialize sums at 0

                for (int k = heightThird * 2; k < heightThird * 3; k++)
                {//repeat for each row of bottom third
                    currentLeftDist = currentRightDist = 0; //initialize current row values at 0

                    i = halfWidth;
                    while (cannyImg.Data[k, i, 0] == 0 && i > 0)
                    {//count number of pixels to left side
                        i--;
                        currentLeftDist++;
                    }//end while
                    leftDistSum += currentLeftDist;

                    i = halfWidth;
                    while (cannyImg.Data[k, i, 0] == 0 && i < cannyImg.Width - 1)
                    {//calculate number of pixels to right side
                        i++;
                        currentRightDist++;
                    }//end while
                    rightDistSum += currentRightDist;
                }//end for (int k = heightThird * 2; k < heightThird * 3; k++)

                //calculate averages to each side
                avgLeftDist = leftDistSum / heightThird;
                avgRightDist = rightDistSum / heightThird;

                //send averages to arduino
                send[0] = Convert.ToByte(avgLeftDist);
                send[1] = Convert.ToByte(avgRightDist);
                sp1.Write(send, 0, 2);
            }//end if running
        }//end Disp_Captured event handler

        protected override void OnClosing(CancelEventArgs e)
        {/*safty feature for vision code*/
            capture.Stop();
            base.OnClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {/*code to run whenever button1 is clicked.  changes COM Port to one entered in textBox.*/
            if (sp1.IsOpen)
            {//close any already open ports
                sp1.Close();
            }

            label3.Visible = false; //hide error message

            try
            {//check if text is entered, set as port
                sp1.PortName = textBox1.Text;
            }
            catch
            {//display error message if no readable text
                label3.Visible = true;
                return;
            }

            sp1.BaudRate = 115200;

            try
            {//try to open specified channel
                sp1.Open();
            }
            catch
            {//display error if fails
                label3.Visible = true;
                return;
            }

            //next 3: send both bytes = 0 upon new port selection
            send[0] = Convert.ToByte(0);
            send[1] = Convert.ToByte(0);
            sp1.Write(send, 0, 2);
        }//end button1_Click event handler

        private void StartStopBtn_Click(object sender, EventArgs e)
        {/*code to change running state of robot (start and stop)*/
            if (running) //== true
            {
                running = false;

                send[0] = Convert.ToByte(0);
                send[1] = Convert.ToByte(0);
                sp1.Write(send, 0, 2);

                StartStopBtn.Text = "Start";
                StartStopBtn.BackColor = Color.Lime;
            }//end if
            else if (sp1.IsOpen)//the robot is not running AND there is an open COM port
            {
                running = true;
                StartStopBtn.Text = "Stop";
                StartStopBtn.BackColor = Color.Red;
            }//end else if
        }//end StartStopBtn_Click event handler

        private void trackBar1_Scroll(object sender, EventArgs e)
        {/*changes the value of the canny threshold whenever trackBar1 is changed*/
            cannyThresh = trackBar1.Value;
            label1.Text = cannyThresh.ToString();
        }//end trackBar1_Scroll event handler

        private void trackBar2_Scroll(object sender, EventArgs e)
        {/*changes the value of the canny Linking threshold whenever trackBar2 is changed*/
            cannyLinkThresh = trackBar2.Value;
            label2.Text = cannyLinkThresh.ToString();
        }//end trackbar2_Scroll event handler

    }
}
