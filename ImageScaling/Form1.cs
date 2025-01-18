using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ImageScaling
{
    public partial class Form1 : Form
    {
        //[DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\JA_proj\\x64\\Debug\\ImageScaling_Cpp.dll")]
        //public static extern void BoxBlurAsm();

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\JA_proj\\x64\\Debug\\ImageScaling_Cpp.dll")]
        public static extern Bitmap ScaleImage(const Bitmap& bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight);

        public Form1()
        {
            InitializeComponent();
        }

        private Bitmap bitmapPhoto;
        private Bitmap bitmapScaled;

        private bool CppCheck;
        private bool AsmCheck;

        private void btnChoose_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string pathPhoto = openFileDialog.FileName;

                try
                {
                    // Wczytanie obrazu w różnych formatach jako Bitmap
                    bitmapPhoto = new Bitmap(pathPhoto);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while opening photo: {ex.Message}");
                }
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (bitmapScaled == null)
                {
                    MessageBox.Show("No scaled image to save. Perform scaling first.");
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png";
                saveFileDialog.Title = "Save Scaled Image As";
                saveFileDialog.FileName = "scaled_output.png";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string savePath = saveFileDialog.FileName;
                    bitmapScaled.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                    MessageBox.Show("Image saved successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving photo: {ex.Message}");
            }
        }

        private void btnPerform_Click(object sender, EventArgs e)
        {
            if (bitmapPhoto == null)
            {
                MessageBox.Show("No image loaded. Please choose an image first.");
                return;
            }

            try
            {
                // Przykładowe nowe wymiary dla obrazu
                int newWidth = 200;  // Nowa szerokość
                int newHeight = 200; // Nowa wysokość

                // Skalowanie obrazu
                bitmapScaled = ScaleBitmap(bitmapPhoto, newWidth, newHeight);
            
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while scaling photo: {ex.Message}");
            }
        }

        private Bitmap ScaleBitmap(Bitmap bitmap, int newWidth, int newHeight)
        {
            Bitmap scaledBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(scaledBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, newWidth, newHeight);
            }
            return scaledBitmap;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {
        }

        private void checkBox_Cpp_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCpp.Checked)
            {
                CppCheck = true;
            }
            else
            {
                CppCheck = false;
            }

        }

        private void checkBox_Asm_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCpp.Checked)
            {
                AsmCheck = true;
            }
            else
            {
                AsmCheck = false;
            }
        }
    }
}
