using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;


namespace ImageScaling
{
    public partial class Form1 : Form
    {
        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Cpp.dll", CallingConvention = CallingConvention.Cdecl)]
        
        public static extern IntPtr ScaleImageCpp(
            byte[] bitmapPhoto, 
            int originalWidth, 
            int originalHeight, 
            int newWidth,
            int newHeight
            );

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Cpp.dll")]
        public static extern void FreeImageMemory(IntPtr memory);

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Asm.dll", CallingConvention = CallingConvention.Cdecl)]
       
        private static extern void ScaleImageAsm(
            IntPtr inputPtr,       
            IntPtr outputPtr,      
            int iWidth,        
            int iHeight,       
            int oWidth,       
            int oHeight       
        );

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
                    // Load image in different formats as Bitmap
                    bitmapPhoto = new Bitmap(pathPhoto);
                    bitmapPhoto = ConvertTo24bpp(bitmapPhoto);

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

            if (labelAsmTime.Text != "no data" && labelCppTime.Text != "no data")
            {
                if (long.TryParse(labelAsmTime.Text, out long asmTime) && long.TryParse(labelCppTime.Text, out long cppTime))
                {
                    labelDifferenceTime.Text = (Math.Abs(cppTime - asmTime)).ToString();
                }
                else
                {
                    labelDifferenceTime.Text = "Invalid input";
                }
            }
        }

        private void FormatCpp()
        {
            try
            {
                byte[] inputPixels = BitmapToByteArray(bitmapPhoto);

                //check time
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // call DLL function
                IntPtr result = ScaleImageCpp(inputPixels, bitmapPhoto.Width, bitmapPhoto.Height, newWidth, newHeight);

                stopwatch.Stop();

                long elapsedMicroseconds = (long)Math.Round(stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1_000_000.0));
                labelCppTime.Text = elapsedMicroseconds.ToString();

                if (result != IntPtr.Zero)
                {
                    // Assuming 24bpp BGR format
                    int outputSize = (newWidth * newHeight * 3); 
                    byte[] outputPixels = new byte[outputSize];

                    // Copying data from unmanaged memory
                    Marshal.Copy(result, outputPixels, 0, outputSize);

                    // Convertion Bitmap
                    bitmapScaled = ByteArrayToBitmap(outputPixels, newWidth, newHeight);

                    // Freeing memory on the C++ side
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
                if (bitmapPhoto == null)
                {
                    MessageBox.Show("No input image loaded.");
                    return;
                }

                // Lload target dimensions from TextBox (e.g. newWidth, newHeight)
                if (newWidth <= 0 || newHeight <= 0)
                {
                    MessageBox.Show("Invalid target dimensions.");
                    return;
                }

                // Convert to Bitmap
                using (Bitmap bmp32 = ConvertTo32bpp(bitmapPhoto))
                {
                    int iWidth = bmp32.Width;
                    int iHeight = bmp32.Height;

                    // Download flattened data (4 bytes/pixel, no stride)
                    byte[] inputData = BitmapTo32bppArrayFlattened(bmp32);

                    // Allocation memory for input
                    IntPtr inputPtr = Marshal.AllocHGlobal(inputData.Length);
                    Marshal.Copy(inputData, 0, inputPtr, inputData.Length);

                    // Allocation memory for output (newWidth * newHeight * 4) 
                    int outputSize = newWidth * newHeight * 4;
                    IntPtr outputPtr = Marshal.AllocHGlobal(outputSize);


                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    ScaleImageAsm(
                        inputPtr,    
                        outputPtr,   
                        iWidth,      
                        iHeight,     
                        newWidth,    
                        newHeight    
                    );

                    stopwatch.Stop();
                    long elapsedMicroseconds = (long)Math.Round(stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1_000_000.0));

                    //time in microseconds
                    labelAsmTime.Text = elapsedMicroseconds.ToString();

                    // Copying the result to the managed table
                    byte[] outputData = new byte[outputSize];
                    Marshal.Copy(outputPtr, outputData, 0, outputSize);

                    // Creating a Bitmap from flattened data
                    bitmapScaled = ArrayTo32bppBitmapFlattened(outputData, newWidth, newHeight);

                    // Release the indicators
                    Marshal.FreeHGlobal(inputPtr);
                    Marshal.FreeHGlobal(outputPtr);

                    MessageBox.Show("Image scaled successfully in 32bpp using ASM (6 params).");
                    saveImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in FormatAsm6Params32bpp: " + ex.Message);
            }
        }

        private static Bitmap ArrayTo32bppBitmapFlattened(byte[] data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            BitmapData bd = null;
            try
            {
                bd = bmp.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    bmp.PixelFormat
                );
                int stride = bd.Stride;  
                IntPtr dstScan0 = bd.Scan0;

                int bytesPerPixel = 4;
                for (int y = 0; y < height; y++)
                {
                    IntPtr dstPtr = dstScan0 + y * stride;
                    int srcOffset = y * (width * bytesPerPixel);

                    Marshal.Copy(data, srcOffset, dstPtr, width * bytesPerPixel);
                }
            }
            finally
            {
                if (bd != null) bmp.UnlockBits(bd);
            }
            return bmp;
        }

        private static byte[] BitmapTo32bppArrayFlattened(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format32bppRgb &&
                bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException("Bitmap must be 32bpp.");
            }

            int width = bmp.Width;
            int height = bmp.Height;
            int bytesPerPixel = 4;
            byte[] result = new byte[width * height * bytesPerPixel];

            BitmapData bd = null;
            try
            {
                bd = bmp.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    bmp.PixelFormat
                );
                int stride = bd.Stride; 
                IntPtr scan0 = bd.Scan0;

                for (int y = 0; y < height; y++)
                {
                   
                    IntPtr srcPtr = scan0 + y * stride;
                    
                    int dstOffset = y * (width * bytesPerPixel);

                    Marshal.Copy(srcPtr, result, dstOffset, width * bytesPerPixel);
                }
            }
            finally
            {
                if (bd != null) bmp.UnlockBits(bd);
            }
            return result;
        }

        private static Bitmap ConvertTo32bpp(Bitmap source)
        {
            // If it is already 32bpp, we just clone
            if (source.PixelFormat == PixelFormat.Format32bppArgb ||
                source.PixelFormat == PixelFormat.Format32bppRgb)
            {
                return new Bitmap(source);
            }

            // Otherwise we convert to 32bpp (without alpha channel -> Format32bppRgb)
            Bitmap bmp32 = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppRgb);
            using (Graphics g = Graphics.FromImage(bmp32))
            {
                g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height));
            }
            return bmp32;
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
        private void Form1_Load(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e){ }
        private void label10_Click(object sender, EventArgs e){ }
        private void label12_Click(object sender, EventArgs e) { }
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
        private static Bitmap ConvertTo24bpp(Bitmap source)
        {
            if (source.PixelFormat == PixelFormat.Format24bppRgb)
            { 
                return new Bitmap(source);
            }
            Bitmap bmp24 = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp24))
            {
                g.DrawImage(source, new Rectangle(0, 0, bmp24.Width, bmp24.Height));
            }
            return bmp24;
        }
        private void labelCppTime_Click(object sender, EventArgs e) { }
        private void btmExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
