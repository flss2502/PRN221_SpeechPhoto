using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SpeechPhoto_WPF
{
    public partial class Image : Window
    {
        private string imagePath;
        private ObservableCollection<ImageViewModel> imageList;
        private string selectedFolderPath;

        public class ImageEventArgs : EventArgs
        {
            public string ImagePath { get; set; }
        }

        public Image(ObservableCollection<ImageViewModel> imageList, string selectedFolderPath, string imagePath)
        {
            InitializeComponent();
            this.imageList = imageList;
            this.selectedFolderPath = selectedFolderPath;
            this.imagePath = imagePath;
            DisplayImage();
        }

        private void DisplayImage()
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
                imageView.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error displaying image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        public event EventHandler<ImageEventArgs> ImageDeleted;

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                    OnImageDeleted(imagePath); // Gọi sự kiện ImageDeleted
                    this.Close();
                }
                else
                {
                    System.Windows.MessageBox.Show("Error: Image file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnImageDeleted(string imagePath)
        {
            ImageDeleted?.Invoke(this, new ImageEventArgs { ImagePath = imagePath });
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
