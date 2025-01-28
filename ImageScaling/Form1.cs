using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ImageScaling
{
    public partial class Form1 : Form
    {
        //[DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\JA_proj\\x64\\Debug\\ImageScaling_Cpp.dll")]
        //public static extern void BoxBlurAsm();

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Cpp.dll")]
        public static extern IntPtr ScaleImageCpp(byte[] bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight);

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Cpp.dll")]
        public static extern void FreeImageMemory(IntPtr memory);

        [DllImport(@"C:\Users\Ryzen\Desktop\Git_repos\image_scaler\x64\Debug\ImageScaling_Asm.dll ")]
        public static extern IntPtr ScaleImageAsm(byte[] bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight);


        public Form1()
        {
            InitializeComponent();
        }

        private Bitmap bitmapPhoto;
        private Bitmap bitmapScaled;

        private int newWidth;
        private int newHeight;

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
                    currentWidth.Text = (bitmapPhoto.Width).ToString();
                    currentHeight.Text = (bitmapPhoto.Height).ToString();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while opening photo: {ex.Message}");
                }
            }


        }

        void saveImage()
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

        private void btnFormatPhoto_Click(object sender, EventArgs e)
        {
            int.TryParse(widthTextBox.Text, out newWidth);
            int.TryParse(heightTextBox.Text, out newHeight);

            Console.WriteLine(newWidth);
            Console.WriteLine(newHeight);

            if (bitmapPhoto == null)
            {
                MessageBox.Show("No image provided.");
                return;
            }

            if (CppCheck)
            {
                FormatCpp();
            }
            if (AsmCheck)
            {
                FormatAsm();
            }
        }

        private void FormatCpp()
        {
            try
            {
                byte[] inputPixels = BitmapToByteArray(bitmapPhoto);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Wywołanie funkcji DLL
                IntPtr result = ScaleImageCpp(inputPixels, bitmapPhoto.Width, bitmapPhoto.Height, newWidth, newHeight);

                stopwatch.Stop();

                double elapsedMicroseconds = stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1_000_000.0);
                labelCppTime.Text = elapsedMicroseconds.ToString();

                if (result != IntPtr.Zero)
                {
                    // Alokacja tablicy bajtów w C#
                    int outputSize = (newWidth * newHeight * 3); // Assuming 24bpp BGR format
                    byte[] outputPixels = new byte[outputSize];

                    // Kopiowanie danych z niezarządzanej pamięci
                    Marshal.Copy(result, outputPixels, 0, outputSize);

                    // Konwersja na obiekt Bitmap
                    bitmapScaled = ByteArrayToBitmap(outputPixels, newWidth, newHeight);

                    // Zwolnienie pamięci po stronie C++
                    FreeImageMemory(result);

                    MessageBox.Show("Image scaled successfully using C++.");

                    saveImage();
                }
                else
                {
                    MessageBox.Show("Scaling failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while scaling photo: {ex.Message}");
            }
        }
        private void FormatAsm()
        {
            try
            {
                byte[] inputPixels = BitmapToByteArray(bitmapPhoto);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                MessageBox.Show("Calling ScaleImageAsm...");
                // Wywołanie funkcji DLL
                IntPtr result = ScaleImageAsm(inputPixels, bitmapPhoto.Width, bitmapPhoto.Height, newWidth, newHeight);

                stopwatch.Stop();

                double elapsedMicroseconds = stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1_000_000.0);
                labelAsmTime.Text = elapsedMicroseconds.ToString();

                if (result != IntPtr.Zero)
                {
                    // Alokacja tablicy bajtów w C#
                    int outputSize = (newWidth * newHeight * 3); // Assuming 24bpp BGR format
                    byte[] outputPixels = new byte[outputSize];

                    // Kopiowanie danych z niezarządzanej pamięci
                    Marshal.Copy(result, outputPixels, 0, outputSize);

                    // Konwersja na obiekt Bitmap
                    bitmapScaled = ByteArrayToBitmap(outputPixels, newWidth, newHeight);

                    MessageBox.Show("Image scaled successfully using C++.");

                    saveImage();
                }
                else
                {
                    MessageBox.Show("Scaling failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while scaling photo: {ex.Message}");
            }
        }

        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);
            bitmap.UnlockBits(bitmapData);

            return rgbValues;
        }

        private Bitmap ByteArrayToBitmap(byte[] pixelData, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            Marshal.Copy(pixelData, 0, bitmapData.Scan0, pixelData.Length);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
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
            if (checkBoxAsm.Checked)
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
