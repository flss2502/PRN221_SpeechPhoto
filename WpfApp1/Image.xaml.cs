using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;
using WinForms = System.Windows.Forms;
using Microsoft.Win32;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Prn221_project_2
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isImageSelectedFromList = false;
        private string SelectedImagePath;
        private Cloudinary _cloudinary;


        public MainWindow()
        {
            InitializeComponent();

            Account cloudinaryAccount = new Account(
                 "dwqq0mx4j",
                    "485134685654857",
                    "lFJj6FmcE2owKBgm_1UGvb4-m6M");
            _cloudinary = new Cloudinary(cloudinaryAccount);
        }

        private async Task<string> UploadImageToCloudinaryAsync(string filePath)
        {
            Account cloudinaryAccount = new Account(
                    "dwqq0mx4j",
                    "485134685654857",
                    "lFJj6FmcE2owKBgm_1UGvb4-m6M");

            Cloudinary cloudinary = new Cloudinary(cloudinaryAccount);

            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(filePath),
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                return uploadResult.SecureUri.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Error uploading image to Cloudinary", ex);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            dialog.InitialDirectory = "C:\\Users\\ASUS\\Pictures\\Screenshots";
            WinForms.DialogResult result = dialog.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath;
                FolderPathTextBox.Text = folderPath;

                FolderContentsListView.Items.Clear();

                try
                {
                    string[] directories = System.IO.Directory.GetDirectories(folderPath);
                    string[] files = System.IO.Directory.GetFiles(folderPath);

                    foreach (var directory in directories)
                    {
                        FolderContentsListView.Items.Add(new { Type = "Folder", Name = System.IO.Path.GetFileName(directory), Path = directory });
                    }

                    foreach (var file in files)
                    {
                        FolderContentsListView.Items.Add(new { Type = "File", Name = System.IO.Path.GetFileName(file), Path = file });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }



        private async void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem người dùng đã chọn một hình ảnh từ danh sách chưa
            if (isImageSelectedFromList)
            {
                try
                {
                    // Thực hiện đẩy ảnh lên Cloudinary
                    string cloudinaryUrl = await UploadImageToCloudinaryAsync(SelectedImagePath);
                    MessageBox.Show($"Image uploaded to Cloudinary: {cloudinaryUrl}", "Upload Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error uploading image to Cloudinary: {ex.Message}", "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Nếu người dùng chưa chọn một hình ảnh từ danh sách, hiển thị thông báo lỗi
                MessageBox.Show("Please select an image from the list to upload.", "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    






        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg; *.jpeg; *.png; *.gif|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedImagePath = openFileDialog.FileName;
                LoadImage(SelectedImagePath);
            }
        }


        private async void LoadImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();
                SelectedImage.Source = bitmap;

              
            }
            else
            {
                // Hiển thị thông báo lỗi nếu đường dẫn không hợp lệ hoặc hình ảnh không tồn tại
                MessageBox.Show("Invalid image path or image does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void FolderContentsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = FolderContentsListView.SelectedItem;

            if (selectedItem != null)
            {
                string selectedType = (string)selectedItem.GetType().GetProperty("Type").GetValue(selectedItem);

                if (selectedType == "File")
                {
                    // Cập nhật biến cờ thành true khi chọn một hình ảnh
                    isImageSelectedFromList = true;

                    // Lấy đường dẫn của hình ảnh được chọn
                    SelectedImagePath = (string)selectedItem.GetType().GetProperty("Path").GetValue(selectedItem);

                    // Load hình ảnh được chọn
                    LoadImage(SelectedImagePath);
                }
            }
        }

        private async void Select10LatestImages_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ListResourcesResult result = await _cloudinary.ListResourcesAsync(new ListResourcesByPrefixParams
                {
                    MaxResults = 30, // Lấy tối đa 30 hình ảnh
                    ResourceType = ResourceType.Image,
                    Type = "upload", // Loại tài nguyên: upload là hình ảnh
                    Direction = "desc", // Sắp xếp giảm dần theo thời gian tạo
               
                });

                Resource[] resources = result.Resources.OrderBy(resource => resource.CreatedAt).ToArray();
                List<string> latestImageUrls = result.Resources.Select(resource => resource.SecureUrl.ToString()).ToList();


                // Hiển thị các URL trong ListBox
                LatestImagesListBox.Items.Clear();
                foreach (string imageUrl in latestImageUrls)
                {
                    LatestImagesListBox.Items.Add(imageUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void LatestImagesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LatestImagesListBox.SelectedItem != null)
            {
                string selectedImageUrl = LatestImagesListBox.SelectedItem.ToString();

                BitmapImage bitmap = new BitmapImage(new Uri(selectedImageUrl));
                SelectedImageCloud.Source = bitmap;
            }
        }





        private void CreateFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                // Prompt user for new folder name
                string newFolderName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new folder name:", "Create Folder", "");

                if (!string.IsNullOrWhiteSpace(newFolderName))
                {
                    try
                    {
                        string newFolderPath = System.IO.Path.Combine(FolderPathTextBox.Text, newFolderName);

                        // Create the new folder
                        System.IO.Directory.CreateDirectory(newFolderPath);

                        // Refresh the folder contents in the ListView
                        RefreshFolderContents();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error creating folder: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderContentsListView.SelectedItem != null)
            {
                string selectedPath = (string)FolderContentsListView.SelectedItem.GetType().GetProperty("Path").GetValue(FolderContentsListView.SelectedItem);

                if (System.IO.Directory.Exists(selectedPath))
                {
                    try
                    {
                        // Delete the selected folder
                        System.IO.Directory.Delete(selectedPath, true);

                        // Refresh the folder contents in the ListView
                        RefreshFolderContents();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting folder: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RenameFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderContentsListView.SelectedItem != null)
            {
                string selectedPath = (string)FolderContentsListView.SelectedItem.GetType().GetProperty("Path").GetValue(FolderContentsListView.SelectedItem);

                if (System.IO.Directory.Exists(selectedPath))
                {
                    // Prompt user for new folder name
                    string newFolderName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new folder name:", "Rename Folder", "");

                    if (!string.IsNullOrWhiteSpace(newFolderName))
                    {
                        try
                        {
                            string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(selectedPath), newFolderName);

                            // Rename the folder
                            System.IO.Directory.Move(selectedPath, newPath);

                            // Refresh the folder contents in the ListView
                            RefreshFolderContents();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error renaming folder: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void RefreshFolderContents()
        {
            if (!string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                FolderContentsListView.Items.Clear();

                try
                {
                    string[] directories = System.IO.Directory.GetDirectories(FolderPathTextBox.Text);
                    string[] files = System.IO.Directory.GetFiles(FolderPathTextBox.Text);

                    foreach (var directory in directories)
                    {

                        FolderContentsListView.Items.Add(new { Type = "Folder", Name = System.IO.Path.GetFileName(directory), Path = directory });
                    }

                    foreach (var file in files)
                    {
                        FolderContentsListView.Items.Add(new { Type = "File", Name = System.IO.Path.GetFileName(file), Path = file });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
