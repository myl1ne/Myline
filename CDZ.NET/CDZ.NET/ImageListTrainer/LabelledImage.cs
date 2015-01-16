using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CDZNET.Applications.ImageListTrainer
{
    public class RecursiveLabel
    {
        public string label;
        public Rectangle box;
        public List<RecursiveLabel> components = new List<RecursiveLabel>();
    }

    public class LabelledImage
    {
        public List<RecursiveLabel> labels = new List<RecursiveLabel>();
        public Bitmap image;
    }
}
