#pragma once

#define STANDARD_DPI 96
#define DPI_SCALE(in) in * Config_GetMenuConfig().DPI / STANDARD_DPI

#define MENU_SPLITTER 2
#define MENU_SIDEBAR 1
#define MENU_SEPCIAL 0

#define MENUID_MODERNCONTEXTMENU 666


typedef struct _MENUTAG *PMENUTAG;

typedef struct _MENUTAG{
	LPWSTR arguments;
	LPWSTR command;
	LPWSTR text;
	HICON icon;
	HBITMAP bitmap;
	UINT iconSize;

	PMENUTAG nextTag;
}MENUTAG;

bool Menu_NotCreated();

void Menu_Init();

void Menu_Show(HWND hWnd, WORD xPos, WORD yPos);

LRESULT MeasureItem(HWND hWnd, WPARAM wParam, LPMEASUREITEMSTRUCT lParam);

LRESULT DrawItem(HWND hWnd, WPARAM wParam, LPDRAWITEMSTRUCT lParam);

void Menu_Action(HMENU hMenu, WORD MenuPos);

void Menu_Destroy();

