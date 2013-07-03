using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace Fizbin.Kinect.Gestures.Demo
{
    public partial class Window1 : Window
    {
        KinectSensor sensor;
        GestureController gController;
        Skeleton[] skeletons;

        public Window1()
        {
            InitializeComponent();
            this.Loaded += Window1_Loaded;
            this.Unloaded += Window1_Unloaded;
        }

        void Window1_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sensor != null)
                sensor.Stop();
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            sensor = KinectSensor.KinectSensors[0];

            sensor.SkeletonStream.Enable();
            sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;

            gController = new GestureController();
            gController.GestureRecognized += gController_GestureRecognized;

            sensor.Start();
        }

        void gController_GestureRecognized(object sender, GestureEventArgs e)
        {
            switch (e.GestureType)
            {
                case GestureType.SwipeLeft:
                    lbl.Content = "Swipe Left";
                    PresentationForward();
                    break;
                case GestureType.SwipeRight:
                    lbl.Content = "Swipe Right";
                    PresentationBack();
                    break;
                default:
                    lbl.Content = "None";
                    break;
            }
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);

                foreach(Skeleton s in skeletons)
                {
                    if (s.TrackingState == SkeletonTrackingState.NotTracked)
                        continue;
                    gController.UpdateAllGestures(s);
                }
            }
        }

        void PresentationForward()
        {
            System.Windows.Forms.SendKeys.SendWait("{Right}");
        }

        private void PresentationBack()
        {
            System.Windows.Forms.SendKeys.SendWait("{Left}");
        }
    }
}
