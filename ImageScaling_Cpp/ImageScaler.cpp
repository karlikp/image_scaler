#include "pch.h"
#include "ImageScaler.h"
#include <vector>
#include <stdexcept>
#include <cmath>
#include <cstring>


extern "C" IMAGE_SCALER_API uint8_t * ScaleImageCpp(uint8_t * bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight) {
    if (!bitmapPhoto || originalWidth <= 0 || originalHeight <= 0 || newWidth <= 0 || newHeight <= 0) {
        return nullptr;
    }

    int newRowSize = (newWidth * 3 + 3) & ~3; // wyrÛwnanie do 4 bajtÛw
    int newPixelArraySize = newRowSize * newHeight;

    // Alokacja pamiÍci dla nowego obrazu
    unsigned char* resizedBitmap = new unsigned char[newPixelArraySize];

    if (!resizedBitmap) {
        return nullptr;
    }

    // Skalowanie obrazu (prosta interpolacja najbliøszego sπsiada)
    for (int y = 0; y < newHeight; ++y) {
        for (int x = 0; x < newWidth; ++x) {
            int srcX = static_cast<int>(x * originalWidth / static_cast<float>(newWidth));
            int srcY = static_cast<int>(y * originalHeight / static_cast<float>(newHeight));
            const unsigned char* srcPixel = bitmapPhoto + (srcY * originalWidth * 3) + (srcX * 3);
            unsigned char* destPixel = resizedBitmap + (y * newRowSize) + (x * 3);

            destPixel[0] = srcPixel[0]; // Blue
            destPixel[1] = srcPixel[1]; // Green
            destPixel[2] = srcPixel[2]; // Red
        }
    }

    return resizedBitmap; // Zwracamy wskaünik na dane obrazu
}

extern "C" IMAGE_SCALER_API void FreeImageMemory(uint8_t * memory) {
    delete[] memory;
}

