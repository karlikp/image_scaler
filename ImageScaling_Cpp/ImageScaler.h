#ifndef IMAGE_SCALER_H
#define IMAGE_SCALER_H

#include <vector>

// Macro to export/import functions in DLL/SLL library
#ifdef IMAGE_SCALER_EXPORTS
#define IMAGE_SCALER_API __declspec(dllexport)
#else
#define IMAGE_SCALER_API __declspec(dllimport)
#endif

// Declaration of the image scaling function
extern "C" IMAGE_SCALER_API uint8_t * ScaleImageCpp(uint8_t * bitmapPhoto, int originalWidth, int originalHeight, int newWidth, int newHeight);

// Declaration of a function to free memory allocated in a DLL
extern "C" IMAGE_SCALER_API void FreeImageMemory(uint8_t * memory);

#endif // IMAGE_SCALER_H
