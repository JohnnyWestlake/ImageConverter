using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageConverter.Common
{
    public class ConversionOptions
    {
        public Guid EncoderId { get; set; }
        public string FileExtention { get; set; }
        public CreationCollisionOption CollisionOption { get; set; }
    }
}
