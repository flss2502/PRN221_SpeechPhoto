using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;
using System.IO.Pipes;
using System.Diagnostics;
using System.Linq;

namespace SpeechPhoto_WPF
{
    public partial class CapturedPhotoWindow : Window
    {
        private BitmapImage bitmapImage;

        public CapturedPhotoWindow(Bitmap bitmapImage) 
        {
            InitializeComponent();
            this.bitmapImage = ConvertToBitmapImage(bitmapImage);
            Dispatcher.BeginInvoke(new Action(() => DisplayCapturedPhoto()));
        }

        private void DisplayCapturedPhoto()
        {
            // Chuyển đổi Bitmap sang BitmapImage để hiển thị trong control hình ảnh của WPF
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
                System.Windows.MessageBox.Show($"Photo saved to {filePath}");
            }
        }

        private void SaveCapturedBitmapToFile(string filePath)
        {
            try
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();

                BitmapSource bitmapSource = (BitmapSource)capturedImageView.Source;

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(fileStream);
                }

                System.Windows.MessageBox.Show($"Photo saved to {filePath}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving photo: {ex.Message}");
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Account cloudinaryAccount = new Account(
                    "dltdfgtzt",
                    "857332939335692",
                    "bhTKRNb9hnxLVZfUB853aSAPW1o"
                );

                Cloudinary cloudinary = new Cloudinary(cloudinaryAccount);

                byte[] imageBytes;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    encoder.Save(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                string publicId = $"image_{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid()}";

                ImageUploadParams uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(publicId, new MemoryStream(imageBytes)), // Sử dụng MemoryStream chứa byte array của ảnh
                    PublicId = publicId,
                };

                ImageUploadResult uploadResult = await cloudinary.UploadAsync(uploadParams);
                System.Windows.MessageBox.Show($"Image uploaded successfully. Public ID: {uploadResult.PublicId}");
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error uploading image to Cloudinary: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
