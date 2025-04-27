

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace KinectStrawberryPlucker
{
    public partial class MainWindow : Window
    {
        private KinectSensor kinectSensor;
        private WriteableBitmap colorBitmap;
        private WriteableBitmap depthBitmap;
        private byte[] depthPixels;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No Kinect sensor detected. Please check the USB connection and power adapter.");
                return;
            }

            try
            {
                kinectSensor = KinectSensor.KinectSensors[0];
                if (kinectSensor == null)
                {
                    MessageBox.Show("Failed to initialize Kinect sensor.");
                    return;
                }

                if (!kinectSensor.IsRunning)
                {
                    kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                    colorBitmap = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgr32, null);
                    KinectImage.Source = colorBitmap;

                    depthBitmap = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Gray8, null);
                    DepthImage.Source = depthBitmap;
                    depthPixels = new byte[640 * 480];

                    kinectSensor.ColorFrameReady += KinectSensor_ColorFrameReady;
                    kinectSensor.DepthFrameReady += KinectSensor_DepthFrameReady;

                    kinectSensor.Start();
                    MessageBox.Show("Kinect sensor started successfully.");
                }
                else
                {
                    MessageBox.Show("Kinect sensor is already running.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Kinect: {ex.Message}");
            }
        }

        private void KinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    byte[] colorData = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(colorData);
                    colorBitmap.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        colorData, colorFrame.Width * 4, 0);
                }
            }
        }

        private void KinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    short[] depthData = new short[depthFrame.PixelDataLength];
                    depthFrame.CopyPixelDataTo(depthData);

                    for (int i = 0; i < depthData.Length; i++)
                    {
                        int depth = depthData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                        depthPixels[i] = (byte)(depth / 256); // Normalize to 0-255 for grayscale
                    }

                    depthBitmap.WritePixels(new Int32Rect(0, 0, depthFrame.Width, depthFrame.Height),
                        depthPixels, depthFrame.Width, 0);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (kinectSensor != null)
            {
                if (kinectSensor.IsRunning)
                {
                    kinectSensor.Stop();
                }
                kinectSensor.Dispose();
            }
            base.OnClosed(e);
        }
    }
}
