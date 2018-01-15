#include "Menu98.h"
#include "Menu.h"
#include "Config.h"

HMENU hMainMenu;
MENUTAG* firstTag;

void CalcScale();

bool Menu_NotCreated() {
	return hMainMenu == NULL;
}


void loadMenuResource(MSXML2::IXMLDOMNodePtr curNode, MENUTAG* menuTag, bool isTopMenu) {
	_bstr_t bstr;
	MSXML2::IXMLDOMNodePtr myNode;
	LPWSTR iconPath;
	int iconIndex;


	bstr = curNode->text;
	menuTag->command = (LPWSTR)malloc(sizeof(LPWSTR) * lstrlen(bstr));
	lstrcpy(menuTag->command, bstr);
	wcstok_s(menuTag->command, TEXT("|"), &(menuTag->arguments));
	SF(bstr);

	bstr = curNode->attributes->getNamedItem(TEXT("text"))->text;
	menuTag->text = (LPWSTR)malloc(sizeof(LPWSTR) * lstrlen(bstr));
	lstrcpy(menuTag->text, bstr);
	SF(bstr);

	myNode = curNode->attributes->getNamedItem(TEXT("index"));
	if (myNode == NULL) {
		iconIndex = 0;
	}
	else {
		bstr = myNode->text;
		iconIndex = _wtoi((LPWSTR)bstr);
		SF(bstr);
	}

	myNode = curNode->attributes->getNamedItem(TEXT("icon"));
	if (myNode == NULL) {
		iconPath = NULL;
	}
	else {
		bstr = myNode ->text;
		iconPath = (LPWSTR)malloc(sizeof(LPWSTR) * lstrlen(bstr));
		lstrcpy(iconPath, bstr);
		SF(bstr);
	}



	if (Config_GetMenuConfig().sysStyle) {
		menuTag->iconSize = Config_GetDefIconSize(isTopMenu);
	}
	else {
		myNode = curNode->attributes->getNamedItem(TEXT("size"));
		if (myNode == NULL) {
			menuTag->iconSize = Config_GetDefIconSize(isTopMenu);
		}
		else {
			bstr = myNode->text;
			menuTag->iconSize = _wtoi((LPWSTR)bstr);
			SF(bstr);
		}
	}


	if (iconPath != NULL) {
		menuTag->icon = NULL;
		if (menuTag->iconSize > 20) {
			if (ExtractIconEx(iconPath, iconIndex, &(menuTag->icon), NULL, 1) == 0) {
				ExtractIconEx(iconPath, iconIndex, NULL, &(menuTag->icon), 1);
			}
		}
		else {
			if (ExtractIconEx(iconPath, iconIndex, NULL, &(menuTag->icon), 1) == 0) {
				ExtractIconEx(iconPath, iconIndex, &(menuTag->icon), NULL, 1);
			}
		}
		free(iconPath);
	}

	if (menuTag->icon != NULL && Config_GetMenuConfig().sysStyle) {
		menuTag->bitmap = IconToBitmap(menuTag->icon, DPI_SCALE(menuTag->iconSize));
		DestroyIcon(menuTag->icon);
	}
	
	

	//menuTag->icon = ExtractIcon(GetModuleHandle(NULL), iconPath, iconIndex);
	//menuTag->isTopMenu = isTopMenu;
	menuTag->nextTag = nullptr;
}


void loadMenuTree(MSXML2::IXMLDOMNodePtr curNode, HMENU menu, UINT* menuIndex, bool isTopMenu, MENUTAG* prevTag) {
	MENUTAG* menuTag;
	
	if (strcmp(curNode->nodeName, "item") == 0) {
		menuTag = (MENUTAG*)malloc(sizeof(MENUTAG));
		loadMenuResource(curNode, menuTag, isTopMenu);
		
		
		




		if (Config_GetMenuConfig().sysStyle) {
			AppendMenu(menu, MF_STRING, *menuIndex, menuTag->text);
			SetMenuItemBitmaps(menu, *menuIndex, MF_BYCOMMAND, menuTag->bitmap, nullptr);
		}
		else {
			AppendMenu(menu, MF_OWNERDRAW, *menuIndex, (LPCWSTR)menuTag);
		}

		MENUITEMINFO  info;
		info.cbSize = sizeof(MENUITEMINFO);
		info.fMask = MIIM_DATA | MIIM_STRING;
		info.dwItemData = (ULONG_PTR)menuTag;
		info.dwTypeData = menuTag->text;
		SetMenuItemInfo(menu, *menuIndex, false, &info);



		if (prevTag == nullptr) {
			firstTag = menuTag;
		}
		else {
			prevTag->nextTag = menuTag;
		}
		*menuIndex = *menuIndex + 1;
	}

		
	if (strcmp(curNode->nodeName, "separator") == 0) {
		if (Config_GetMenuConfig().sysStyle)
			AppendMenu(menu, MF_SEPARATOR, MENU_SEPCIAL, (LPWSTR)MENU_SPLITTER);
		else 
			AppendMenu(menu, MF_OWNERDRAW | MF_SEPARATOR, MENU_SEPCIAL, (LPWSTR)MENU_SPLITTER);
	}

	if (strcmp(curNode->nodeName, "submenu") == 0) {
		HMENU subMenu = CreatePopupMenu();
		menuTag = (MENUTAG*)malloc(sizeof(MENUTAG));
		loadMenuResource(curNode, menuTag, isTopMenu);


		if (Config_GetMenuConfig().sysStyle) {
			AppendMenu(menu, MF_STRING | MF_POPUP, (UINT_PTR)subMenu, menuTag->text);
			SetMenuItemBitmaps(menu, (UINT)subMenu, MF_BYCOMMAND, menuTag->bitmap, nullptr);			
		}
		else {
			AppendMenu(menu, MF_OWNERDRAW | MF_POPUP, (UINT_PTR)subMenu, (LPCWSTR)menuTag);
		}
		
		
		if (prevTag == nullptr) {
			firstTag = menuTag;
		}
		else {
			prevTag->nextTag = menuTag;
		}
		

		for (int i = 0; i != curNode->childNodes->length; ++i) {
			loadMenuTree(curNode->childNodes->item[i], subMenu, menuIndex, false, menuTag);
		}
	}

}



void Menu_Build() {
	UINT menuIndex;

	MSXML2::IXMLDOMNodeListPtr spNodeList = Config_Root()->childNodes;
	menuIndex = 1;
	for (int i = 0; i != spNodeList->length; ++i) {
		MSXML2::IXMLDOMNodePtr curNode = spNodeList->item[i];
		
		loadMenuTree(curNode, hMainMenu, &menuIndex, true, firstTag);
	}

	if (Config_GetMenuConfig().titleBarEnabled) {
		MENUITEMINFO menuInfo;
		menuInfo.cbSize = sizeof(MENUITEMINFO);
		menuInfo.fMask = MIIM_FTYPE;
		GetMenuItemInfo(hMainMenu, 0, true, &menuInfo);
		menuInfo.fType |= MFT_MENUBARBREAK;
		menuInfo.fMask = MIIM_FTYPE;
		SetMenuItemInfo(hMainMenu, 0, true, &menuInfo);
		
		InsertMenu(hMainMenu, 0, MF_BYPOSITION | MF_GRAYED | MF_OWNERDRAW, MENU_SEPCIAL, (LPWSTR)MENU_SIDEBAR);
	}


}


void Menu_Init() {
	Menu_Destroy();

	hMainMenu = CreatePopupMenu();

	MENUINFO menuInfo;
	memset(&menuInfo, 0, sizeof(MENUINFO));
	menuInfo.cbSize = sizeof(MENUINFO);
	menuInfo.fMask = MIM_STYLE;
	menuInfo.dwStyle = MNS_NOTIFYBYPOS;
	SetMenuInfo(hMainMenu, &menuInfo);

	Menu_Build();

	/*
	MENUINFO menuInfo;
	memset(&menuInfo, 0, sizeof(MENUINFO));
	menuInfo.cbSize = sizeof(MENUINFO);
	menuInfo.fMask = MIM_BACKGROUND;
	HBITMAP h = (HBITMAP)LoadImage(GetModuleHandle(NULL), L"D:\\1.bmp", IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
	//HBITMAP bmp = LoadBitmap(hInst,MAKEINTRESOURCE(h));
	//UINT err = GetLastError();
	menuInfo.hbrBack = CreatePatternBrush(h);
	SetMenuInfo(hMenu, &menuInfo); 
	*/
}



void Menu_Show(HWND hWnd, WORD xPos, WORD yPos) {
	CalcScale();

	SetForegroundWindow(hWnd);

	if (Config_GetMenuConfig().popup) {
		POINT pt;
		GetCursorPos(&pt);
		MyTrackPopupMenuEx(hMainMenu, TPM_RIGHTBUTTON | TPM_VERNEGANIMATION, pt.x, pt.y, hWnd, NULL);
	}
	else {
		RECT myRect;
		GetWindowRect(hWnd, &myRect);
		SystemParametersInfo(SPI_GETWORKAREA, 0, &myRect, 0);
		MyTrackPopupMenuEx(hMainMenu, TPM_RIGHTBUTTON | TPM_VERNEGANIMATION, 0, myRect.bottom, hWnd , NULL);
	}

	return;
}

void Menu_Action(HMENU hMenu, WORD MenuPos) {
	MENUITEMINFO  info;
	info.cbSize = sizeof(MENUITEMINFO);
	info.fMask = MIIM_DATA;
	GetMenuItemInfo(hMenu, MenuPos, true, &info);
	MENUTAG* t = (MENUTAG*)info.dwItemData;

	ShellExecute(0, TEXT("open"), t->command, t->arguments, L"", SW_SHOWNORMAL);

}

void Menu_Destroy() {
	if (hMainMenu != nullptr) {
		Config_FreeResource();
		for (MENUTAG* curNode = firstTag; curNode != NULL; ) {
			free(curNode->command);
			free(curNode->text);
			
			if (curNode->icon != NULL)
				DestroyIcon(curNode->icon);

			if (curNode->bitmap != NULL)
				DeleteObject(curNode->bitmap);
			
			MENUTAG* f = curNode;
			curNode = curNode->nextTag;
			free(f);
		}
		firstTag = NULL;
		DestroyMenu(hMainMenu);
		hMainMenu = NULL;
	}
}
