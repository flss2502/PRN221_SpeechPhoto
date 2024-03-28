using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SpeechPhoto_WPF
{
    public class ImageViewModel
    {
        public BitmapImage Thumbnail { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
