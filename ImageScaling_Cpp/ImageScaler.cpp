#include "pch.h"
#include "ImageScaler.h"
#include <vector>
#include <stdexcept>
#include <cmath>
#include <cstring>

/*
struct Pixel {
    unsigned char b, g, r; // BMP u¿ywa kolejnoœci kolorów BGR
};

// Klasa Bitmap do przechowywania danych obrazu BMP
struct Bitmap {
    std::vector<unsigned char> header; // Nag³ówek BMP (54 bajty)
    std::vector<Pixel> pixels;         // Dane pikseli
    int width;                         // Szerokoœæ obrazu
    int height;                        // Wysokoœæ obrazu
};
*/

// Funkcja skaluj¹ca
extern "C" IMAGE_SCALER_API Bitmap ScaleImage(const Bitmap& bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight) {
    // Sprawdzenie zgodnoœci wymiarów bitmapy z argumentami
    if (bitmapPhoto.width != originalWidth || bitmapPhoto.height != originalHeight) {
        throw std::invalid_argument("Podane wymiary nie zgadzaj¹ siê z rzeczywistymi wymiarami obrazu.");
    }

    // Skalowanie obrazu
    std::vector<Pixel> resizedPixels(newWidth * newHeight);
    for (int y = 0; y < newHeight; ++y) {
        for (int x = 0; x < newWidth; ++x) {
            int srcX = static_cast<int>(x * originalWidth / static_cast<float>(newWidth));
            int srcY = static_cast<int>(y * originalHeight / static_cast<float>(newHeight));
            resizedPixels[y * newWidth + x] = bitmapPhoto.pixels[srcY * originalWidth + srcX];
        }
    }

    // Tworzenie nowej bitmapy
    Bitmap resizedBitmap;
    resizedBitmap.header = bitmapPhoto.header; // Kopiowanie nag³ówka

    // Aktualizacja nag³ówka BMP
    int rowSize = (newWidth * 3 + 3) & ~3; // Ka¿dy wiersz musi byæ wyrównany do 4 bajtów
    int pixelArraySize = rowSize * newHeight;
    int fileSize = 54 + pixelArraySize;

    std::memcpy(&resizedBitmap.header[2], &fileSize, 4);           // Rozmiar ca³ego pliku
    std::memcpy(&resizedBitmap.header[18], &newWidth, 4);          // Nowa szerokoœæ
    std::memcpy(&resizedBitmap.header[22], &newHeight, 4);         // Nowa wysokoœæ
    std::memcpy(&resizedBitmap.header[34], &pixelArraySize, 4);    // Rozmiar danych pikseli

    // Przypisanie przeskalowanych pikseli
    resizedBitmap.pixels = std::move(resizedPixels);
    resizedBitmap.width = newWidth;
    resizedBitmap.height = newHeight;

    return resizedBitmap;
}
