using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SpeechPhoto_WPF
{
    public partial class CapturedPhotoWindow : Window
    {
        private string tempFilePath;

        public CapturedPhotoWindow(string tempFilePath)
        {
            InitializeComponent();
            this.tempFilePath = tempFilePath;
            DisplayCapturedPhoto();
        }

        private void DisplayCapturedPhoto()
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(tempFilePath));
            capturedImageView.Source = bitmapImage;
        }

        private BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "PNG Image (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                SaveCapturedBitmapToFile(filePath);
                MessageBox.Show($"Photo saved to {filePath}");
            }
        }

        private void SaveCapturedBitmapToFile(string filePath)
        {

        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            // Code to upload the image to cloud
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
