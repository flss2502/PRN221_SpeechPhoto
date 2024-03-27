using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SpeechPhoto_WPF
{
    public partial class CapturedPhotoWindow : Window
    {
        private string imagePath;
        private ObservableCollection<ImageViewModel> imageList;
        private string selectedFolderPath;
        private ListView imageListView;

        public CapturedPhotoWindow(ObservableCollection<ImageViewModel> imageList, string selectedFolderPath, string imagePath)
        {
            InitializeComponent();
            this.selectedFolderPath = selectedFolderPath;
            this.imageList = imageList;
            this.imagePath = imagePath;
            DisplayCapturedPhoto();
        }

        private void DisplayCapturedPhoto()
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
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
            try
            {
                Account cloudinaryAccount = new Account(
                    "dltdfgtzt",
                    "857332939335692",
                    "bhTKRNb9hnxLVZfUB853aSAPW1o"
                );

                Cloudinary cloudinary = new Cloudinary(cloudinaryAccount);

                ImageUploadParams uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imagePath),
                    PublicId = $"image_{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid()}", // Set a unique public ID for the uploaded image
                                                                                           // Additional parameters as needed (e.g., folder, tags, etc.)
                };

                ImageUploadResult uploadResult = cloudinary.Upload(uploadParams);

                // Handle the upload result as needed
                Console.WriteLine($"Image uploaded successfully. Public ID: {uploadResult.PublicId}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error uploading image to Cloudinary: {ex.Message}");
            }

            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
