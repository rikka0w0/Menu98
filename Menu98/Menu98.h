#pragma once

#include <stdlib.h>


#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <shellapi.h>


// TODO: reference additional headers your program requires here

#define SYSFREE


#define WM_MENU98 0x0409
#define MENU98_SHOWMENU 0x90
#define MENU98_EXITHOST 0x91
#define MENU98_REFRESH 0x92
#define MENU98_INITMSGHOOK 0x89

HBITMAP IconToBitmap(HICON, INT);
void ClassicMenu(HMENU hMenu);

#ifdef SYSFREE
#define SF(a) SysFreeString(a)
#else
#define SF(a)
#endif

typedef BOOL(WINAPI *FPT_TrackPopupMenuEx)(HMENU, UINT, int, int, HWND, LPTPMPARAMS);
extern FPT_TrackPopupMenuEx MyTrackPopupMenuEx;