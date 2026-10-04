#pragma once
// MSVC CRT conveniences used by the pinned AMD source; never alter SDK layouts.
#include <algorithm>
#include <cstdio>
#include <cstring>
#include <cwchar>
#include <locale>
#include <cmath>
#define __declspec(x) __attribute__((visibility("default")))
#define _countof(a) (sizeof(a) / sizeof((a)[0]))
#define sprintf_s snprintf
inline int strcpy_s(char* dst, size_t size, const char* src) {
    if (!dst || !src || !size) return 22;
    if (std::strlen(src) >= size) { dst[0] = 0; return 34; }
    std::strcpy(dst, src); return 0;
}
inline int wcscpy_s(wchar_t* dst, size_t size, const wchar_t* src) {
    if (!dst || !src || !size) return 22;
    if (std::wcslen(src) >= size) { dst[0] = 0; return 34; }
    std::wcscpy(dst, src); return 0;
}
template<size_t N> int wcscpy_s(wchar_t (&dst)[N], const wchar_t* src) {
    return wcscpy_s(dst, N, src);
}
