#ifndef IMAGE_SCALER_H
#define IMAGE_SCALER_H

#include <vector>

// Makro do eksportowania/importowania funkcji w bibliotece DLL/SLL
#ifdef IMAGE_SCALER_EXPORTS
#define IMAGE_SCALER_API __declspec(dllexport)
#else
#define IMAGE_SCALER_API __declspec(dllimport)
#endif

// Struktura pikseli w formacie BMP
struct Pixel {
    unsigned char b, g, r;
};

// Struktura Bitmap do przechowywania obrazu BMP
struct Bitmap {
    std::vector<unsigned char> header; // Nag³ówek BMP
    std::vector<Pixel> pixels;         // Dane pikseli
    int width;                         // Szerokoœæ obrazu
    int height;                        // Wysokoœæ obrazu
};

// Deklaracja funkcji skaluj¹cej
extern "C" IMAGE_SCALER_API Bitmap ScaleImage(const Bitmap & bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight);

#endif // IMAGE_SCALER_H
