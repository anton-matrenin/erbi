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
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using Coding4Fun.Kinect.Wpf;

namespace Fizbin.Kinect.Gestures.Demo
{

    public partial class Window2 : Window
    {
        UserInfo[] userInfos;
        KinectSensor sensor;
        Skeleton[] skeletons;
        InteractionStream inter;
        bool isGrip = false;
        bool isMultitouch = false;

        public Window2()
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
            userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];

            TransformSmoothParameters parameters = new TransformSmoothParameters();
            parameters.Smoothing = 0.7f;
            parameters.Correction = 0.3f;
            parameters.Prediction = 0.4f;
            parameters.JitterRadius = 1.0f;
            parameters.MaxDeviationRadius = 0.5f;

            sensor.SkeletonStream.Enable(parameters);
            sensor.DepthStream.Enable();

            inter = new InteractionStream(sensor, new DIC());

            sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;           
            sensor.DepthFrameReady += sensor_DepthFrameReady;    
            inter.InteractionFrameReady += inter_InteractionFrameReady;

            sensor.Start();
        }

        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    inter.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
            }
        }

        void inter_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (var frame = e.OpenInteractionFrame())
            {
                if (frame != null)
                {
                    frame.CopyInteractionDataTo(userInfos);
                }
            }

            if (isMultitouch)
                return;

            foreach (var _userInfo in userInfos)
            {
                var hands = _userInfo.HandPointers;
                foreach (var hand in hands)
                {
                    if (hand.IsTracked)
                    {
                        if (hand.HandEventType == InteractionHandEventType.Grip)
                            isGrip = true;
                        if (hand.HandEventType == InteractionHandEventType.GripRelease)
                            isGrip = false;
                    }
                }
            }
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                    inter.ProcessSkeleton(skeletons, sensor.AccelerometerGetCurrentReading(), frame.Timestamp);

                    foreach (Skeleton s in skeletons)
                    {
                        ControlSkeleton(s);                     
                    }
                }
            }
        }

        void ControlSkeleton(Skeleton s)
        {
            if (s.TrackingState == SkeletonTrackingState.Tracked)
            {
                
                Joint jointRight = s.Joints[JointType.HandRight];
                Joint jointLeft = s.Joints[JointType.HandLeft];
                Joint head = s.Joints[JointType.Head];

                if(head.Position.Z > 1.5)
                {                   
                    Joint scaledRight = jointRight.ScaleTo((int)SystemParameters.PrimaryScreenWidth,
                        (int)SystemParameters.PrimaryScreenHeight, 0.30f, 0.30f);

                    CheckMultitouch(jointRight, jointLeft, head);

                    if (isMultitouch)
                    {
                        MouseMethods.SendMouseInput(WheelValue(jointRight, jointLeft));
                    }
                    else
                    {
                        MouseMethods.SendMouseInput((int)scaledRight.Position.X,
                                                    (int)scaledRight.Position.Y,
                                                    (int)SystemParameters.PrimaryScreenWidth,
                                                    (int)SystemParameters.PrimaryScreenHeight,
                                                    isGrip);
                    }
                }
            }
        }

        void CheckMultitouch(Joint jr, Joint jl, Joint head)
        {
            if (head.Position.Z - jr.Position.Z > 0.45 && head.Position.Z - jl.Position.Z > 0.45)
                isMultitouch = true;
            else
                isMultitouch = false;
        }

        int WheelValue(Joint jr, Joint jl)
        {
            if (jr.Position.X - jl.Position.X > 0.5)
                return 40;
            else
                return -40;
            
        }
    }
}
