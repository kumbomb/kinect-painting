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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// FirstDisplay.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FirstDisplay : UserControl
    {
        DispatcherTimer Image_CreativeAnimation_timer = new DispatcherTimer();
        DispatcherTimer TwoimageAnimation_timer = new DispatcherTimer();
        DispatcherTimer ThreeButtnAnimation_timer = new DispatcherTimer();
        DispatcherTimer InformAnimation_timer = new DispatcherTimer();

        DispatcherTimer ThisCloseAnimation_timer = new DispatcherTimer();

        double button1_x_po = 600;
        double button2_x_po = 900;
        double button3_x_po = 1200;

        private void FirstAnimation()
        {
            MainWindow._isTutorialActive = true;
            Image_CreativeAnimation_timer.Interval = new TimeSpan(500);
            Image_CreativeAnimation_timer.Tick += new EventHandler(Image_CreativeAnimation);
            Image_CreativeAnimation_timer.Start();
        }

        private void SecondAnimation()
        {
            TwoimageAnimation_timer.Interval = new TimeSpan(9000);
            TwoimageAnimation_timer.Tick += new EventHandler(TwoAnimation);
            TwoimageAnimation_timer.Start();
           
        }
        private void ThirdAnimation()
        {
            ThreeButtnAnimation_timer.Interval = new TimeSpan(1000);
            ThreeButtnAnimation_timer.Tick += new EventHandler(ThreeButtonAnimation);
            ThreeButtnAnimation_timer.Start();
        }

        private void InFormAnimation()
        {
            InformAnimation_timer.Interval = new TimeSpan(1000);
            InformAnimation_timer.Tick += new EventHandler(InformationAnimation);
            InformAnimation_timer.Start();
        }

        private void StartClickAnimation()
        {
            ThisCloseAnimation_timer.Interval = new TimeSpan(1000);
            ThisCloseAnimation_timer.Tick += new EventHandler(ThisCloseAnimation);
            ThisCloseAnimation_timer.Start();
        }


        void InformationAnimation(object sender, EventArgs e)
        {
            if (Image_Inform.Opacity <1)
            {
                Image_Inform.Opacity += 0.00007;
            }
            else
            {
                Image_Inform.Opacity = 0;
            }

        }

        void ThisCloseAnimation(object sender, EventArgs e)
        {
            if (this.Opacity > 0)
            {
                this.Opacity -= 0.0001;
            }
            else
            {
                ThisCloseAnimation_timer.Stop();
                this.Visibility = Visibility.Hidden;
                MainWindow._isTutorialActive = false;
            }
        }

        void ThreeButtonAnimation(object sender, EventArgs e)
        {
            if(button1_x_po>0)
            {
                Btn_StartPainting.Margin = new Thickness(button1_x_po, 0, 0, 0);
                button1_x_po-=0.5;
            }
            if( button2_x_po>0)
            {
                   Btn_Howuse.Margin = new Thickness(button2_x_po, 0, 0, 0);
                button2_x_po-=0.5;
            }

            if( button3_x_po>0)
            {
                Btn_Close.Margin = new Thickness(button3_x_po, 0, 0, 0);
                button3_x_po-=0.5;
            }

            if (button1_x_po <= 0 && button2_x_po <= 0 && button3_x_po <= 0)
            {
                ThreeButtnAnimation_timer.Stop();
                InFormAnimation();
            }

        }
        void TwoAnimation(object sender, EventArgs e)
        {
            if (Image_Paintingfor.Opacity <= 1)
            {
                Image_Paintingfor.Opacity += 0.0001;
                Image_Urs.Opacity += 0.0001;
            }
            else
            {
                TwoimageAnimation_timer.Stop();
                ThirdAnimation();
            }
        }

        void Image_CreativeAnimation(object sender, EventArgs e)
        {
            if (Image_Creative.Width != 700)
            {
                Image_Creative.Width += 0.1 ;
            }
            if (Image_Creative.Height != 300)
            {
                Image_Creative.Height+=0.05;
            }

            if (Image_Creative.Width > 700 && Image_Creative.Height > 300)
            {
                Image_CreativeAnimation_timer.Stop();
                SecondAnimation();
            }
        }

        public FirstDisplay()
        {
            InitializeComponent();
            Image_Creative.Width = 50;
            Image_Creative.Height = 50;
            FirstAnimation();
          
        }

        private void StartPainting_CursorEnter(object sender, CursorEventArgs e)
        {
            DoubleAnimation StartPaintAnimator= new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(2)), FillBehavior.Stop);
            Btn_StartPainting.BeginAnimation(OpacityProperty, StartPaintAnimator);
        }


        private void StartPainting_Click(object sender, RoutedEventArgs e)
        {
            StartClickAnimation();
            InformAnimation_timer.Stop();
        }

        private void Howuse_CursorEnter(object sender, CursorEventArgs e)
        {
            DoubleAnimation HowuseAnimator = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(2)), FillBehavior.Stop);
           Btn_Howuse.BeginAnimation(OpacityProperty, HowuseAnimator);
        }

        private void Close_CursorEnter(object sender, CursorEventArgs e)
        {
            DoubleAnimation CloseAnimator = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(2)), FillBehavior.Stop);
            Btn_Close.BeginAnimation(OpacityProperty, CloseAnimator);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void Click_HowToUse(object sender, RoutedEventArgs e)
        {
            HowToUse.Visibility = Visibility.Visible;
        }
    }
}
