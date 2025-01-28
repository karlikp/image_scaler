using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ImageScaling
{
   
    internal static class Feature
    {
        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Cpp.dll")]
        public static extern IntPtr ScaleImageCpp(byte[] bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight);

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Cpp.dll")]
        public static extern void FreeImageMemory(IntPtr memory);

        [DllImport(@"C:\Users\Ryzen\Desktop\Git_repos\image_scaler\x64\Debug\ImageScaling_Asm.dll ")]
        public static extern IntPtr ScaleBMPAsm(IntPtr pointerToInputPixels, int newWidth, int newHeight);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, uint dwFreeType);


        public static void saveImage(Bitmap bitmapScaled)
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

        public static double ProcessImageWithCppAndGetTime(byte[] inputPixels, Bitmap bitmapPhoto, int newWidth, int newHeight) 
        {
            double elapsedMicroseconds = 0;

            try
            {
               

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Wywołanie funkcji DLL
                IntPtr result = ScaleImageCpp(inputPixels, bitmapPhoto.Width, bitmapPhoto.Height, newWidth, newHeight);

                stopwatch.Stop();

                elapsedMicroseconds = stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1_000_000.0);

                if (result != IntPtr.Zero)
                {
                    // Alokacja tablicy bajtów w C#
                    int outputSize = (newWidth * newHeight * 3); // Assuming 24bpp BGR format
                    byte[] outputPixels = new byte[outputSize];

                    Marshal.Copy(result, outputPixels, 0, outputSize);

                    Bitmap bitmapScaled = ByteArrayToBitmap(outputPixels, newWidth, newHeight);

                    FreeImageMemory(result);

                    MessageBox.Show("Image scaled successfully using C++.");

                    saveImage(bitmapScaled);
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
            return elapsedMicroseconds;
        }

        public static double ProcessImageWithAsmAndGetTime(IntPtr pointerToInputPixels, Bitmap bitmapPhoto, int newWidth, int newHeight)
        {
            double elapsedMicroseconds = 0;

            try
            {
                Console.WriteLine($"Width: {bitmapPhoto.Width}, Height: {bitmapPhoto.Height}, NewWidth: {newWidth}, NewHeight: {newHeight}");
                

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Wywołanie funkcji DLL (Assembler)
                IntPtr result = ScaleBMPAsm(pointerToInputPixels, newWidth, newHeight);

                stopwatch.Stop();

                elapsedMicroseconds = stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1_000_000.0);


                if (result == IntPtr.Zero)
                {
                    Console.WriteLine("Scaling failed. The function returned NULL.");
                    return 0;
                }

                // Create a new Bitmap from the returned data
                Bitmap scaledBitmap = ByteArrayToBitmap(result, newWidth, newHeight);

                // Free the allocated memory in the DLL
                VirtualFree(result, IntPtr.Zero, 0x8000);

                // Save or display the scaled image
                saveImage(scaledBitmap);
                Console.WriteLine("Image scaling successful!");

                if (result != IntPtr.Zero)
                {
                    // Alokacja tablicy bajtów w C#
                    int outputSize = (newWidth * newHeight * 3); // Assuming 24bpp BGR format
                    byte[] outputPixels = new byte[outputSize];

                    Marshal.Copy(result, outputPixels, 0, outputSize);

                    Bitmap bitmapScaled = ByteArrayToBitmap(outputPixels, newWidth, newHeight);

                    FreeImageMemory(result);

                    MessageBox.Show("Image scaled successfully using Assembler.");

                    saveImage(bitmapScaled);
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

            return elapsedMicroseconds;
        }
        private static Bitmap ByteArrayToBitmap(IntPtr byteArrayPtr, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);

            // Lock bitmap bits to get the pointer to the pixel data
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            // Copy from unmanaged memory to the bitmap's scan0 pointer
            int stride = bitmapData.Stride;
            int totalBytes = Math.Abs(stride) * height;

            Marshal.Copy(byteArrayPtr, 0, bitmapData.Scan0, totalBytes);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public static Bitmap ByteArrayToBitmap(byte[] pixelData, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            Marshal.Copy(pixelData, 0, bitmapData.Scan0, pixelData.Length);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);
            bitmap.UnlockBits(bitmapData);

            return rgbValues;
        }
    }
}
