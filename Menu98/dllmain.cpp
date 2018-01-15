// dllmain.cpp : Defines the entry point for the DLL application.
#include "Menu98.h"
#include "Menu.h"
#include "Config.h"

static HMODULE gLibModule = 0;

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		gLibModule = hModule;
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

static HWND hWnd_StartBtn;
static LONG_PTR OldWndProc_StartBtn = 0;

void Refresh() {
	if (Menu_NotCreated()) {
		Config_Load();
		Menu_Init();
		Config_Unload();
	}
}

LRESULT CALLBACK WndProc_StartBtn(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
	switch (uMsg) {
	case WM_NCHITTEST:
		return HTCLIENT;


	case WM_CONTEXTMENU:
		Refresh();

		if (GetAsyncKeyState(VK_SHIFT))
			break;

		Menu_Show(hwnd, LOWORD(lParam), HIWORD(lParam));
		return 0;

	case WM_MENUCOMMAND:
		Menu_Action((HMENU)lParam, LOWORD(wParam));
		return 0;

	case WM_MEASUREITEM:
		return MeasureItem(hwnd, wParam, (LPMEASUREITEMSTRUCT)lParam);

	case WM_DRAWITEM:
		return DrawItem(hwnd, wParam, (LPDRAWITEMSTRUCT)lParam);
	}

	return WNDPROC(OldWndProc_StartBtn)(hwnd, uMsg, wParam, lParam);
}


typedef struct MENU98_INIT_T {
	HWND hWnd_TaskBar;
	void* TrackPopupMenuEx;
	LPSTR cmdLine;
} MENU98_INIT;
FPT_TrackPopupMenuEx MyTrackPopupMenuEx;

extern "C" _declspec(dllexport) DWORD  __cdecl  __Menu98Init(MENU98_INIT* initInfo) {
	HWND hWnd_TaskBar = initInfo->hWnd_TaskBar;
	LPSTR lpszCmdLine = initInfo->cmdLine;

	if (strlen(lpszCmdLine) == 0) {	// No start cmd, assume Windows 10
		hWnd_StartBtn = FindWindowEx(hWnd_TaskBar, nullptr, L"Start", nullptr);
	} else if (lpszCmdLine[0] == '!') {
		lpszCmdLine++;
		hWnd_StartBtn = FindWindowExA(hWnd_TaskBar, 0, lpszCmdLine, nullptr);
	} else {
		hWnd_StartBtn = FindWindowA(lpszCmdLine, nullptr);
	}

	if (IsWindow(hWnd_StartBtn)) {
		OldWndProc_StartBtn = GetWindowLongPtr(hWnd_StartBtn, GWLP_WNDPROC);
		if (OldWndProc_StartBtn != 0)
			SetWindowLongPtr(hWnd_StartBtn, GWLP_WNDPROC, (LONG_PTR)&WndProc_StartBtn);
	} else {
		MessageBoxA(0, "Could not find the Start Orb, is your injection parameter correct?", "Fatal Error - Menu98", MB_ICONEXCLAMATION);
	}

	MyTrackPopupMenuEx = (FPT_TrackPopupMenuEx) initInfo->TrackPopupMenuEx;

	return 0;
}

extern "C" _declspec(dllexport) DWORD  __cdecl  __Menu98Unload(LPVOID unused) {
	if (OldWndProc_StartBtn != NULL)
		SetWindowLongPtr(hWnd_StartBtn, GWLP_WNDPROC, OldWndProc_StartBtn);

	CloseHandle(CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)FreeLibraryAndExitThread, gLibModule, 0, nullptr));

	return 0;
}