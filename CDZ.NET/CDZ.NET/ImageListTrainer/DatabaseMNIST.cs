using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.ComponentModel;

namespace CDZNET.Applications.ImageListTrainer
{
    public static class DatabaseMNIST
    {
        public static List<LabelledImage> loadData(string pixelFile, string labelFile)
        {
            FileStream ifsPixels = new FileStream(pixelFile, FileMode.Open);
            FileStream ifsLabels = new FileStream(labelFile, FileMode.Open);
            BinaryReader brImages = new BinaryReader(ifsPixels);
            BinaryReader brLabels = new BinaryReader(ifsLabels);
            int magic1 = brImages.ReadInt32(); // stored as big endian
            magic1 = ReverseBytes(magic1); // convert to Intel format
            int imageCount = brImages.ReadInt32();
            imageCount = ReverseBytes(imageCount);
            int numRows = brImages.ReadInt32();
            numRows = ReverseBytes(numRows);
            int numCols = brImages.ReadInt32();
            numCols = ReverseBytes(numCols);
            int magic2 = brLabels.ReadInt32();
            magic2 = ReverseBytes(magic2);
            int numLabels = brLabels.ReadInt32();
            numLabels = ReverseBytes(numLabels);

            List<LabelledImage> db = new List<LabelledImage>();

            for (int cnt = 0; cnt < 1000 /*imageCount*/; cnt++)
            {
                LabelledImage img = new LabelledImage();
                img.image = new System.Drawing.Bitmap(numRows, numCols, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                for (int i = 0; i < numCols; i++)
                {
                    for (int j = 0; j < numRows; j++)
                    {
                        int bt = brImages.ReadByte();
                        img.image.SetPixel(j, i, Color.FromArgb(bt, bt, bt));
                    }
                }
                //using (var ms = new MemoryStream(brImages.ReadBytes(numRows*numCols)))
                //{
                //    img.image = new Bitmap(ms);
                //}
                byte lbl = brLabels.ReadByte(); // get the label
                RecursiveLabel label = new RecursiveLabel();
                label.label = "Label_" + (int) lbl;
                label.box = new Rectangle(0,0,numCols,numRows);
                img.labels.Add(label);
                db.Add(img);
            }
            brImages.Close();
            brLabels.Close();
            return db;
        }
        public static int ReverseBytes(int v)
        {
            byte[] intAsBytes = BitConverter.GetBytes(v);
            Array.Reverse(intAsBytes);
            return BitConverter.ToInt32(intAsBytes, 0);
        }
    }
}
