#ifndef IMAGE_SCALER_H
#define IMAGE_SCALER_H

#include <vector>

// Makro do eksportowania/importowania funkcji w bibliotece DLL/SLL
#ifdef IMAGE_SCALER_EXPORTS
#define IMAGE_SCALER_API __declspec(dllexport)
#else
#define IMAGE_SCALER_API __declspec(dllimport)
#endif

// Deklaracja funkcji skaluj¹cej obraz
extern "C" IMAGE_SCALER_API uint8_t * ScaleImageCpp(uint8_t * bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight);

// Deklaracja funkcji do zwalniania pamiêci alokowanej w DLL
extern "C" IMAGE_SCALER_API void FreeImageMemory(uint8_t * memory);

#endif // IMAGE_SCALER_H
