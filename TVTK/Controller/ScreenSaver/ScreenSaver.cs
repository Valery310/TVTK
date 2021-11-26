using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Media.Animation;
using System.Windows;
using TVTK.Properties;

namespace TVTK
{
    public class ScreenSaver
    {
        private Canvas canvas;
        private static Random random = new Random();
        private List<Uri> imageList;
        BitmapImage bitmapImage;
        System.Windows.Controls.Image image;
        ThicknessAnimation thicknessAnimation;
        DoubleAnimation doubleAnimation;
        Storyboard storyboard;


        public ScreenSaver(Canvas _canvas) 
        {
            canvas = _canvas;
            image = new System.Windows.Controls.Image();
            image.MaxHeight = canvas.Height * 0.1;
            image.MaxWidth = canvas.Width * 0.1;
           // int height = random.Next(0, Convert.ToInt32(canvas.Height / 2));
           // image.Margin = new System.Windows.Thickness(-50, height, 0, 0);
            //  image.Margin = new Thickness(-50, random.Next(0, Convert.ToInt32(canvas.Height)),0,0);
            canvas.Children.Add(image);           
        }



        public void StartScreenSaver(int half = 1) 
        {
            imageList = UpdateList();
            if (imageList.Count>0)
            {
                bitmapImage = new BitmapImage(imageList[random.Next(0, imageList.Count - 1)]);                
                image.Source = bitmapImage;
                int height = 0;
                if (half == 1)
                {
                    height = random.Next(0, Convert.ToInt32(canvas.Height / 2));
                   // image.Margin = new System.Windows.Thickness(-50, height, 0, 0);
                }
               
                if (half == 2)
                {
                    height = random.Next(Convert.ToInt32(canvas.Height / 2), Convert.ToInt32(canvas.Height));
                  // image.Margin = new Thickness(-50, height, 0, 0);//random.Next(Convert.ToInt32(canvas.Height / 2), Convert.ToInt32(canvas.Height))
                    //image.VerticalAlignment = VerticalAlignment.Bottom;
                  //  thicknessAnimation.From = image.Margin;
                }
                //image.Margin = new System.Windows.Thickness(-50, height, 0, 0);
                image.RenderTransform = new RotateTransform();

                thicknessAnimation = new ThicknessAnimation();
                //   thicknessAnimation.From = image.Margin;

                thicknessAnimation.Duration = TimeSpan.FromSeconds(20);
                //  image.BeginAnimation(Canvas.MarginProperty, thicknessAnimation);
                doubleAnimation = new DoubleAnimation();
                doubleAnimation.From = 0;
                doubleAnimation.To = 5000;
                doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(20));
                //   bitmapImage.BeginAnimation(BitmapImage.RotationProperty, doubleAnimation);
                // image.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
                storyboard = new Storyboard();
                Storyboard.SetTarget(doubleAnimation, image);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

                Storyboard.SetTarget(thicknessAnimation, image);
                Storyboard.SetTargetProperty(thicknessAnimation,
                new PropertyPath(Canvas.MarginProperty));

                thicknessAnimation.From = new System.Windows.Thickness(-50, height, 0, 0);
                thicknessAnimation.To = new System.Windows.Thickness(Convert.ToInt32(canvas.Width + image.MaxWidth + 100), height, 0, 0);
                doubleAnimation.Duration = new Duration(new TimeSpan(0,0, random.Next(30, 50)));
                thicknessAnimation.Duration = new Duration(new TimeSpan(0, 0, random.Next(20, 35)));
                //canvas.Children.Add(image);
                image.Visibility = Visibility.Visible;

                storyboard.Children.Add(doubleAnimation);
                storyboard.Children.Add(thicknessAnimation);
                storyboard.Begin();
                //    image.Visibility = Visibility.Collapsed;
                //   image.Margin = new System.Windows.Thickness(-50, height, 0, 0);
                //https://bugsdb.com/_en/debug/1bd1fbb077152d8f5757878fe71e2963
            }
        }

        private void GetAnimation() { }


        private List<Uri> UpdateList() 
        {
            FileInfo[] images;
            var dir = new DirectoryInfo(@".\ScreenSaver\Image");
            images = dir.GetFiles("*.png", SearchOption.AllDirectories);
      
            List<Uri> Temp = new List<Uri>();

            foreach (var item in images)
            {
                Temp.Add(new Uri(item.FullName));
            }
            return Temp;
        }
    }
}
