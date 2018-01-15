#pragma once
#import <msxml3.dll> 
#include <comdef.h>

typedef struct _MENU_CONFIG {
	LPWSTR sideText;
	int sideTextLength;

	int titleBarWidth;//32
	int blankWidth;//8
	int blankHeight;//4
	int captionWidth;//23
	int largeIconSize; //32
	int smallIconSize; //24


	bool titleBarEnabled;
	bool sysStyle;
	bool oldStyle;
	bool popup;

	int DPI;
}MENU_CONFIG;

void Config_Load();
void Config_Unload();
void Config_FreeResource();
MSXML2::IXMLDOMElementPtr Config_Root();
int Config_GetDefIconSize(bool isTopMenu);
MENU_CONFIG Config_GetMenuConfig();