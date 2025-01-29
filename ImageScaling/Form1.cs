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
        //[DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\JA_proj\\x64\\Debug\\ImageScaling_Cpp.dll")]
        //public static extern void BoxBlurAsm();

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Cpp.dll", CallingConvention = CallingConvention.Cdecl)]

        public static extern IntPtr ScaleImageCpp(byte[] bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight);

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Cpp.dll")]
        public static extern void FreeImageMemory(IntPtr memory);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        private static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, int dwFreeType);

        [DllImport("C:\\Users\\Ryzen\\Desktop\\Git_repos\\image_scaler\\x64\\Debug\\ImageScaling_Asm.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ScaleImageAsm(
            IntPtr inputPtr,       // (1) wskaźnik na piksele wejściowe
            IntPtr outputPtr,      // (2) wskaźnik na bufor wyjściowy
            int iWidth,        // (3) szerokość wejściowa
            int iHeight,       // (4) wysokość wejściowa
            int oWidth,       // (5) szerokość docelowa
            int oHeight       // (6) wysokość docelowa
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
                    // Wczytanie obrazu w różnych formatach jako Bitmap
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
                if (bitmapPhoto == null)
                {
                    MessageBox.Show("No input image loaded.");
                    return;
                }

                // Wczytujemy docelowe wymiary z TextBox (np. newWidth, newHeight)
                if (newWidth <= 0 || newHeight <= 0)
                {
                    MessageBox.Show("Invalid target dimensions.");
                    return;
                }

                // 1) Konwertujemy do 32bpp
                using (Bitmap bmp32 = ConvertTo32bpp(bitmapPhoto))
                {
                    int iWidth = bmp32.Width;
                    int iHeight = bmp32.Height;

                    // 2) Pobieramy spłaszczone dane (4 bajty/piksel, bez stride)
                    byte[] inputData = BitmapTo32bppArrayFlattened(bmp32);

                    // 3) Alokujemy pamięć dla inputu
                    IntPtr inputPtr = Marshal.AllocHGlobal(inputData.Length);
                    Marshal.Copy(inputData, 0, inputPtr, inputData.Length);

                    // 4) Alokujemy pamięć dla outputu (newWidth * newHeight * 4)
                    int outputSize = newWidth * newHeight * 4;
                    IntPtr outputPtr = Marshal.AllocHGlobal(outputSize);

                    // (można wyzerować outputPtr, ale ASM i tak nadpisze dane)

                    // 5) Wywołujemy funkcję ASM
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    ScaleImageAsm(
                        inputPtr,    // #1
                        outputPtr,   // #2
                        iWidth,      // #3
                        iHeight,     // #4
                        newWidth,    // #5
                        newHeight    // #6
                    );

                    sw.Stop();
                    double elapsedMicroseconds = sw.ElapsedTicks / (Stopwatch.Frequency / 1_000_000.0);
                    labelAsmTime.Text = elapsedMicroseconds.ToString();

                    // 6) Kopiujemy wynik do zarządzanej tablicy
                    byte[] outputData = new byte[outputSize];
                    Marshal.Copy(outputPtr, outputData, 0, outputSize);

                    // 7) Tworzymy Bitmapę 32bpp ze spłaszczonych danych
                    bitmapScaled = ArrayTo32bppBitmapFlattened(outputData, newWidth, newHeight);

                    // 8) Zwalniamy wskaźniki
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
                int stride = bd.Stride;  // może być >= width * 4
                IntPtr dstScan0 = bd.Scan0;

                int bytesPerPixel = 4;
                for (int y = 0; y < height; y++)
                {
                    IntPtr dstPtr = dstScan0 + y * stride;
                    int srcOffset = y * (width * bytesPerPixel);

                    // Kopiujemy (width * 4) bajtów na wiersz
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
                int stride = bd.Stride; // może być >= width*4
                IntPtr scan0 = bd.Scan0;

                for (int y = 0; y < height; y++)
                {
                    // Adres w pamięci oryginalnego wiersza
                    IntPtr srcPtr = scan0 + y * stride;
                    // Pozycja w wynikowej tablicy (bez paddingu)
                    int dstOffset = y * (width * bytesPerPixel);

                    // Kopiujemy (width * 4) bajtów
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
            // Jeśli już jest 32bpp, to tylko klonujemy
            if (source.PixelFormat == PixelFormat.Format32bppArgb ||
                source.PixelFormat == PixelFormat.Format32bppRgb)
            {
                return new Bitmap(source);
            }

            // Inaczej konwertujemy do 32bpp (bez kanału alfa -> Format32bppRgb)
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




        // == Deklaracje z DLL asm ==
        // Załóżmy, że skompilowałeś do "ImageScaling_Asm.dll"
        // i używasz standardowej konwencji C (Cdecl) w Windows x64.
     

    

        private const int MEM_RELEASE = 0x8000;

        //private void Main()
        //{
        //    // 1) Wczytujemy obraz JPEG z pliku
        //    //string inputPath = "test.jpg";
        //    string outputPath = "nowyobraz.jpg";

        //    //if (!File.Exists(inputPath))
        //    //{
        //    //    Console.WriteLine($"Błąd: Plik '{inputPath}' nie istnieje.");
        //    //    return;
        //    //}

        //    //bitmapPhoto = new Bitmap(pathPhoto);
        //    //currentWidth.Text = (bitmapPhoto.Width).ToString();
        //    //currentHeight.Text = (bitmapPhoto.Height).ToString();

        //    //using (Bitmap bmpInput = new Bitmap(inputPath))

        //    {
        //        // Upewniamy się, że mamy 24 bity na piksel (bez kanału alfa).
        //        // Można zrobić konwersję przez:
        //        using (Bitmap bmp24 = ConvertTo24bpp(bitmapPhoto))
        //        {
        //            int originalWidth = bmp24.Width;
        //            int originalHeight = bmp24.Height;

        //            // Nowe wymiary docelowe
        //            int newWidth = originalWidth / 2;   // Przykładowo o połowę
        //            int newHeight = originalHeight / 2; // ...

        //            // 2) Zrzucamy dane z Bitmapy do tablicy bajtów w formacie BGR (24bpp)
        //            byte[] originalData = BitmapTo24bppArray(bmp24, out int rowSize);

        //            // UWAGA: rowSize to faktyczna liczba bajtów na wiersz w tym, 
        //            //        jak .NET przechowuje dane (może być zaokrąglone do wielokrotności 4).
        //            //        Jednak nasz kod ASM sam oblicza (newWidth*3 + 3)&~3 dla docelowego obrazu. 
        //            //        Dla oryginału nie zawsze musimy mieć (width*3 + 3)&~3 – 
        //            //        ważne, by w tablicy były zapisy wiersz po wierszu, BGR bez odstępów.

        //            // 3) Alokujemy pamięć niezarządzaną i kopiujemy do niej dane
        //            int originalSize = originalData.Length;
        //            IntPtr originalPtr = Marshal.AllocHGlobal(originalSize);
        //            Marshal.Copy(originalData, 0, originalPtr, originalSize);

        //            // 4) Wywołujemy naszą funkcję asm
        //            //IntPtr resizedPtr = ScaleImageAsm(
        //            //    originalPtr,
        //            //    originalWidth,
        //            //    originalHeight,
        //            //    newWidth,
        //            //    newHeight
        //            //);

        //            // Sprawdzamy, czy funkcja nie zwróciła 0 (NULL)
        //            if (resizedPtr == IntPtr.Zero)
        //            {
        //                Console.WriteLine("ScaleImageAsm zwrócił NULL (błąd).");
        //            }
        //            else
        //            {
        //                // 5) Obliczamy rozmiar nowego obrazu
        //                //    newRowSize = (newWidth * 3 + 3) & ~3 (z kodu ASM)
        //                int newRowSize = (newWidth * 3 + 3) & ~3;
        //                int newPixelArraySize = newRowSize * newHeight;

        //                // 6) Kopiujemy dane z powrotem do tablicy .NET
        //                byte[] resizedData = new byte[newPixelArraySize];
        //                Marshal.Copy(resizedPtr, resizedData, 0, newPixelArraySize);

        //                // 7) Tworzymy nowy Bitmap 24bpp z wyniku
        //                using (Bitmap bmpOutput = ArrayTo24bppBitmap(resizedData, newWidth, newHeight, newRowSize))
        //                {
        //                    // 8) Zapisujemy do pliku
        //                    bmpOutput.Save(outputPath, ImageFormat.Jpeg);
        //                    Console.WriteLine($"Zapisano wynik do {outputPath}");
        //                }

        //                // Zwolnienie pamięci zaalokowanej przez VirtualAlloc w ASM
        //                bool freed = VirtualFree(resizedPtr, IntPtr.Zero, MEM_RELEASE);
        //                if (!freed)
        //                {
        //                    Console.WriteLine("Błąd zwalniania pamięci z VirtualAlloc!");
        //                }
        //            }

        //            // Zwolnienie oryginalnej pamięci z AllocHGlobal
        //            Marshal.FreeHGlobal(originalPtr);
        //        }
        //    }

        //    Console.WriteLine("Test zakończony.");
        //    Console.ReadKey();
        //}

        /// <summary>
        /// Konwertuje dowolny Bitmap do 24-bitowego formatu BGR.
        /// </summary>
        private static Bitmap ConvertTo24bpp(Bitmap source)
        {
            if (source.PixelFormat == PixelFormat.Format24bppRgb)
            {
                // Już jest 24 bpp
                return new Bitmap(source);
            }
            // Inaczej konwertujemy
            Bitmap bmp24 = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp24))
            {
                g.DrawImage(source, new Rectangle(0, 0, bmp24.Width, bmp24.Height));
            }
            return bmp24;
        }

        /// <summary>
        /// Zwraca tablicę bajtów (BGR) o rozmiarze (height * stride), 
        /// gdzie stride to liczba bajtów na wiersz (wyrównana do 4).
        /// </summary>
        private static byte[] BitmapTo24bppArray(Bitmap bmp, out int stride)
        {
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
                throw new ArgumentException("Bitmap musi być w formacie 24bpp.");

            BitmapData bd = null;
            try
            {
                bd = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb
                );

                stride = bd.Stride;  // liczba bajtów w jednym wierszu
                int totalBytes = stride * bmp.Height;
                byte[] data = new byte[totalBytes];

                // Kopiujemy z pamięci obrazu do tablicy
                Marshal.Copy(bd.Scan0, data, 0, totalBytes);
                return data;
            }
            finally
            {
                if (bd != null)
                    bmp.UnlockBits(bd);
            }
        }

        /// <summary>
        /// Tworzy nowy Bitmap 24bpp z tablicy bajtów (BGR) o zadanym stride i rozmiarach.
        /// </summary>
        private static Bitmap ArrayTo24bppBitmap(byte[] data, int width, int height, int stride)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bd = null;
            try
            {
                bd = bmp.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb
                );

                // Jeśli .NET obliczy inny stride niż nasz (co się może zdarzyć),
                // musimy kopiować wierszami.
                int bmpStride = bd.Stride;
                int minStride = Math.Min(bmpStride, stride);
                int bytesToCopy = Math.Min(minStride, width * 3);

                for (int y = 0; y < height; y++)
                {
                    IntPtr destPtr = bd.Scan0 + y * bmpStride;
                    int srcOffset = y * stride;
                    Marshal.Copy(data, srcOffset, destPtr, bytesToCopy);
                }
            }
            finally
            {
                if (bd != null)
                    bmp.UnlockBits(bd);
            }
            return bmp;
        }

    }
}
