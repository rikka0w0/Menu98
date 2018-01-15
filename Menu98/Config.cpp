#include "Menu98.h"
#include "Config.h"

MSXML2::IXMLDOMDocumentPtr cfgDoc;
MSXML2::IXMLDOMElementPtr cfgRoot;

MENU_CONFIG config;

void Config_LoadXML() {
	CoInitialize(NULL);
	cfgDoc.CreateInstance(__uuidof(MSXML2::DOMDocument30));
	cfgDoc->load(L"Menu.xml");
	cfgRoot = cfgDoc->documentElement;
}

void Config_Load() {
	Config_LoadXML();

	_bstr_t bstr;
	bstr = cfgRoot->attributes->getNamedItem(TEXT("text"))->text;
	config.sideTextLength = lstrlen(bstr);
	config.sideText = (LPWSTR)malloc(sizeof(LPWSTR) * config.sideTextLength);
	lstrcpy(config.sideText, bstr);
	SF(bstr);
	
	bstr = cfgRoot->attributes->getNamedItem(TEXT("titleBarWidth"))->text;
	config.titleBarWidth = _wtoi(bstr) / 2;
	SF(bstr);
	bstr = cfgRoot->attributes->getNamedItem(TEXT("blankWidth"))->text;
	config.blankWidth = _wtoi(bstr);
	SF(bstr);
	bstr = cfgRoot->attributes->getNamedItem(TEXT("blankHeight"))->text;
	config.blankHeight = _wtoi(bstr);
	SF(bstr);
	bstr = cfgRoot->attributes->getNamedItem(TEXT("captionWidth"))->text;
	config.captionWidth = _wtoi(bstr);
	SF(bstr);
	bstr = cfgRoot->attributes->getNamedItem(TEXT("largeIconSize"))->text;
	config.largeIconSize = _wtoi(bstr);
	SF(bstr);
	bstr = cfgRoot->attributes->getNamedItem(TEXT("smallIconSize"))->text;
	config.smallIconSize = _wtoi(bstr);
	SF(bstr);

	bstr = cfgRoot->attributes->getNamedItem(TEXT("style"))->text;
	char style = _wtoi(bstr);	//0 - system style, 1 - win8 style, 2 - win8 style with sidebar, 3 - xp style, 4 - xp style with sidebar
	SF(bstr);
	config.titleBarEnabled = (style == 2) || (style == 4);
	config.sysStyle = (style < 1) || (style>4);
	config.oldStyle = (style == 3) || (style == 4);

	HDC hdc = GetDC(NULL);
	config.DPI = GetDeviceCaps(hdc, LOGPIXELSX);
	ReleaseDC(NULL, hdc);
}

void Config_Unload() {
	cfgRoot.Release();
	cfgDoc.Release();
	CoUninitialize();
}

void Config_FreeResource() {
	free(config.sideText);
}

MSXML2::IXMLDOMElementPtr Config_Root() {
	return cfgRoot;
}

int Config_GetDefIconSize(bool isTopMenu){
	return isTopMenu ? config.largeIconSize : config.smallIconSize;
}

MENU_CONFIG Config_GetMenuConfig() {
	return config;
}