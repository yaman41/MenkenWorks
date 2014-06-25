using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;

namespace JointTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor _KinectDevice;


        public MainWindow()
        {
            try
            {
                InitializeComponent();
                //Kinectが接続されているか確認する
                if(KinectSensor.KinectSensors.Count == 0){ //Kinectの個数
                    throw new Exception("Kinectを接続詞て下さい");
                }
                //Kinectの動作を開始する
                StartKinect(KinectSensor.KinectSensors[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

         //Kinectの動作を開始する
        private void StartKinect(KinectSensor kinect)
        {
            //RGBカメラを有効にして, フレーム更新イベントを登録する
            kinect.ColorStream.Enable();
            kinect.ColorFrameReady +=
                new EventHandler<ColorImageFrameReadyEventArgs>(
                    kinect_ColorFrameReady);

            //スケルトンを有効にして、フレーム更新イベントを登録する
            kinect.SkeletonFrameReady +=
                new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
            kinect.SkeletonStream.Enable();

            //Kinectの動作を開始する
            kinect.Start();
        }

        //Kinectの動作を停止する
        private void StopKinect(KinectSensor kinect)
        {
            if (kinect != null)
            {
                if (kinect.IsRunning)
                {
                    //フレーム更新イベントを削除する
                    kinect.ColorFrameReady -= kinect_ColorFrameReady;
                    kinect.SkeletonFrameReady -= kinect_SkeletonFrameReady;

                    //kinectの停止と、ネイティブソースを開放する
                    kinect.Stop();
                    kinect.Dispose();

                    rgbImage.Source = null;
                    
                }
            }
        }

        //RGBカメラのフレーム更新イベント
        void kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            try
            {
                //RGBカメラのフレームデータを取得する
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (colorFrame != null)
                    {
                        //RGBカメラのピクセルデータを取得する
                        byte[] colorPixel = new byte[colorFrame.PixelDataLength];
                        colorFrame.CopyPixelDataTo(colorPixel);

                        //ピクセルデータをビットマップに変換する
                        rgbImage.Source = BitmapSource.Create(colorFrame.Width,
                            colorFrame.Height, 96, 96, PixelFormats.Bgr32, null,
                            colorPixel, colorFrame.Width * colorFrame.BytesPerPixel);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            try
            {
                //kinectのインスタンスを取得する
                KinectSensor kinect = sender as KinectSensor;
                if (kinect == null)
                {
                    return;
                }

                //スケルトンのフレームを取得する
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame != null)
                    {
                        DrawSkeleton(kinect, skeletonFrame);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DrawSkeleton(KinectSensor kinect, SkeletonFrame skeletonFrame)
        {
            //スケルトンのデータを取得する
            Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
            skeletonFrame.CopySkeletonDataTo(skeletons);

            canvasSkeleton.Children.Clear();

            //トラッキングされているスケルトンを描画する
            foreach (Skeleton skeleton in skeletons)
            {
                //スケルトンがトラッキング状態の時はジョイントを描画する
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    foreach (Joint joint in skeleton.Joints)
                    {
                        //ジョイントがトラッキングされていなければ次へ
                        if (joint.TrackingState == JointTrackingState.NotTracked)
                        {
                            continue;
                        }

                        //ジョイントの座標を描画する
                        //this.DrawEllipse(kinect, joint.Position);
                        //線を描く
                        this.DrawLine(kinect, skeleton.Joints[JointType.HandLeft], skeleton.Joints[JointType.HandRight]);
                    }
                }
            }
        }

        void DrawEllipse(KinectSensor kinect, SkeletonPoint position)
        {
            const int R = 5;

            //スケルトンの座標を、RGBカメラの座標に変換する
            ColorImagePoint point = kinect.MapSkeletonPointToColor(position, kinect.ColorStream.Format);

            //座標を画面のサイズに変換する
            point.X = (int)ScaleTo(point.X, kinect.ColorStream.FrameWidth, canvasSkeleton.Width);
            point.Y = (int)ScaleTo(point.Y, kinect.ColorStream.FrameHeight, canvasSkeleton.Height);

            //円を描く
            canvasSkeleton.Children.Add(new Ellipse()
            {
                Fill = new SolidColorBrush(Colors.Red),
                Margin = new Thickness(point.X - R, point.Y - R, 0, 0),
                Width = R * 2,
                Height = R * 2,
            });
        }

        double ScaleTo(double value, double source, double dest)
        {
            return (value * dest) / source;
        }

        void DrawLine(KinectSensor kinect, Joint from, Joint to)
        {
            
            JointPoint jointFrom = getJointPoint(kinect, from);
            JointPoint JointTo = getJointPoint(kinect, to);
            this.output1.Text = "" + jointFrom.getPointX();
            this.output2.Text = "" + JointTo.getPointX();
            canvasSkeleton.Children.Add(new Line()
            {
                Stroke = new SolidColorBrush(Colors.Red),
                X1 = jointFrom.getPointX(),
                X2 = JointTo.getPointX(),
                Y1 = jointFrom.getPointY(),
                Y2 = JointTo.getPointY(),
                StrokeThickness = 2,
            });
 

        }

        private JointPoint getJointPoint(KinectSensor kinect, Joint joint)
        {
            JointPoint jointPoint = new JointPoint();
            //スケルトンの座標を、RGBカメラの座標に変換する
            ColorImagePoint point = kinect.MapSkeletonPointToColor(joint.Position, kinect.ColorStream.Format);

            //座標を画面のサイズに変換する
            point.X = (int)ScaleTo(point.X, kinect.ColorStream.FrameWidth, canvasSkeleton.Width);
            point.Y = (int)ScaleTo(point.Y, kinect.ColorStream.FrameHeight, canvasSkeleton.Height);
            int z = (int)(1000 - (joint.Position.Z * 100));
            jointPoint.setPoint(point.X, point.Y, z);
            return jointPoint;

        }

        //Windowが閉じられるときのイベント
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(KinectSensor.KinectSensors[0]);
        }
       
    }
}
