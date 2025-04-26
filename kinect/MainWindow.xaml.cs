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

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Check for available Kinect sensors
            if (KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No Kinect sensor detected. Please check the USB connection and power adapter.");
                return;
            }

            try
            {
                // Get the first Kinect sensor
                kinectSensor = KinectSensor.KinectSensors[0];
                if (kinectSensor == null)
                {
                    MessageBox.Show("Failed to initialize Kinect sensor.");
                    return;
                }

                // Check sensor status
                if (!kinectSensor.IsRunning)
                {
                    // Initialize streams
                    kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                    // Set up bitmap for RGB display
                    colorBitmap = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgr32, null);
                    KinectImage.Source = colorBitmap;

                    // Subscribe to frame-ready event
                    kinectSensor.ColorFrameReady += KinectSensor_ColorFrameReady;

                    // Start the sensor
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