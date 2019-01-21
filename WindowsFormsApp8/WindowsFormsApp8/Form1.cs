using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Video;
using Accord.Video.VFW;
using System.Drawing.Imaging;

namespace WindowsFormsApp8
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap currentFrame;
        private Thread captureThread;
        private AVIWriter writer = new AVIWriter("MPEG-4");
        private bool recordVideo;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo fi in videoDevices)
            {
                comboBox1.Items.Add(fi.Name);
            }
            videoSource = null;
            captureThread = null;
            recordVideo = false;

            button1.Enabled = false;
            button2.Enabled = false;
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                comboBox2.Items.Clear();
                videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
                foreach (VideoCapabilities vc in videoSource.VideoCapabilities)
                {
                    comboBox2.Items.Add("Rozdzielczosc " + vc.FrameSize);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                pictureBox1.Invoke(new Action(delegate { pictureBox1.Image = null; pictureBox1.Invalidate(); })); videoSource = null;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            videoSource.VideoResolution = videoSource.VideoCapabilities[comboBox2.SelectedIndex];
            if (videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                button2.Enabled = videoSource.IsRunning;
                pictureBox1.Invoke(new Action(delegate
                {
                    pictureBox1.Image = null; pictureBox1.Invalidate();
                }));
                videoSource.NewFrame += videoSource_NewFrame;
                videoSource.Start();

                button2.Enabled = videoSource.IsRunning;
            }
            button3.Enabled = true; button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (videoSource != null)
            {
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                    pictureBox1.Invoke(new Action(delegate { pictureBox1.Image = null; pictureBox1.Invalidate(); }));
                }
                else
                {
                    videoSource.NewFrame += videoSource_NewFrame; videoSource.Start();
                }
                button2.Enabled = videoSource.IsRunning;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            recordVideo = true;
        }

        void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            currentFrame = (Bitmap)eventArgs.Frame.Clone();
            if ((captureThread == null || !captureThread.IsAlive) && recordVideo)
            {
                writer.Open("test.avi", 320, 240);
                Bitmap threadFrame = (Bitmap)currentFrame.Clone();
                for (int i = 0; i < 240; i++)
                {           
                    writer.AddFrame(threadFrame);
                }
                writer.Close();
            }
            pictureBox1.Invoke(new Action(delegate { pictureBox1.Image = currentFrame; }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap snapshot = (Bitmap)currentFrame.Clone();
            ImageFormat format = ImageFormat.Png; snapshot.Save("zdjecie.png");
        }
    }
}
