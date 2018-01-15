#include "Menu98.h"
#include "Menu.h"
#include "Config.h"

#define GRADLEVEL 1


int titleBarWidth;
int blankWidth;
int blankHeight;
int captionWidth;

int DPI;

void CalcScale() {
	titleBarWidth = DPI_SCALE(Config_GetMenuConfig().titleBarWidth);
	blankWidth = DPI_SCALE(Config_GetMenuConfig().blankWidth);
	blankHeight = DPI_SCALE(Config_GetMenuConfig().blankHeight);
	captionWidth = DPI_SCALE(Config_GetMenuConfig().captionWidth);
}

void DrawGradientV(HDC hdc, COLORREF co1, COLORREF co2, RECT& DrawRect)
{
	int r = GetRValue(co1);
	int g = GetGValue(co1);
	int b = GetBValue(co1);

	int r2 = GetRValue(co2);
	int g2 = GetGValue(co2);
	int b2 = GetBValue(co2);

	//计算宽,高
	int DrawRectWidth = DrawRect.right - DrawRect.left;
	int DrawRectHeight = DrawRect.bottom - DrawRect.top;

	if (DrawRectWidth <= 0)
		return;

	//初始化rect
	RECT rect = { 0,0,DrawRectWidth, GRADLEVEL};

	//准备GDI
	HDC hMemDC = CreateCompatibleDC(hdc);                //创建内存DC
	HBITMAP hBitmap = ::CreateCompatibleBitmap(hdc, DrawRectWidth, DrawRectHeight);//创建位图
	SelectObject(hMemDC, hBitmap);        //把位图选进内存DC
	HBRUSH hbr;


	for (int i = DrawRectHeight; i > 0; i -= GRADLEVEL)
	{
		//创建刷子
		hbr = CreateSolidBrush(RGB(r, g, b));
		FillRect(hMemDC, &rect, hbr);
		DeleteObject(hbr);

		//改变小正方体的位置
		rect.top += GRADLEVEL;
		rect.bottom += GRADLEVEL;

		//判断小正方体是否超界
		if (rect.bottom > DrawRect.bottom)
			rect.bottom = DrawRect.bottom;

		//改变颜色
		r += (r2 - r + i / 2) / i * GRADLEVEL;
		g += (g2 - g + i / 2) / i * GRADLEVEL;
		b += (b2 - b + i / 2) / i * GRADLEVEL;
	}

	//内存DC映射到屏幕DC
	BitBlt(hdc, DrawRect.left, DrawRect.top, DrawRectWidth, DrawRectHeight, hMemDC, 0, 0, SRCCOPY);

	//删除
	DeleteDC(hMemDC);
	DeleteObject(hBitmap);
}

LRESULT MeasureItem(HWND hWnd, WPARAM wParam, LPMEASUREITEMSTRUCT lpmis) {
	if (wParam != 0)
		return 0;	//Not a menu

	MENUTAG* menuTag = (MENUTAG*)lpmis->itemData;

	if (lpmis->itemID == MENU_SEPCIAL) {
		if (lpmis->itemData == MENU_SIDEBAR) {
			lpmis->itemWidth = titleBarWidth;
			lpmis->itemHeight = 5;		
		}
		if (lpmis->itemData == MENU_SPLITTER) {
			lpmis->itemWidth = 1;
			lpmis->itemHeight = 6;		
		}
		return 0;
	}

	SIZE size;
	HDC hDC = GetDC(hWnd);
	GetTextExtentPoint32(hDC, menuTag->text, lstrlen(menuTag->text), &size);

	lpmis->itemWidth = blankWidth + DPI_SCALE(menuTag->iconSize) + blankWidth + size.cx*2 + +blankWidth;
	lpmis->itemHeight = blankHeight + DPI_SCALE(menuTag->iconSize) + blankHeight;


	return 0;
}

LRESULT DrawItem(HWND hWnd, WPARAM wParam, LPDRAWITEMSTRUCT lpdis) {
	if (wParam != 0)
		return 0;	//Not a menu

	MENUTAG* menuTag = (MENUTAG*)lpdis->itemData;

	RECT myRect;


	
	if (lpdis->itemID == MENU_SEPCIAL) {
		if (lpdis->itemData == MENU_SIDEBAR) {
			int tmp;
			GetClipBox(lpdis->hDC, &myRect);
			tmp = myRect.bottom - myRect.top;
			memcpy(&myRect, &(lpdis->rcItem), sizeof(RECT));
			myRect.bottom = tmp + myRect.top;
			tmp = myRect.right - myRect.left;

			DrawGradientV(lpdis->hDC, RGB(0, 0, 122), RGB(0, 0, 245), myRect);

			GetClipBox(lpdis->hDC, &myRect);
			HFONT hfont = CreateFont(captionWidth, 0, 900, 900, FW_BOLD,
				0, 0, 0, DEFAULT_CHARSET, OUT_DEFAULT_PRECIS,
				CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY,
				DEFAULT_PITCH, TEXT("Arial"));
			HFONT pOldFont = (HFONT)SelectObject(lpdis->hDC, hfont);

			COLORREF oldColor = GetTextColor(lpdis->hDC);
			SetTextColor(lpdis->hDC, RGB(255, 255, 255));
			SetBkMode(lpdis->hDC, TRANSPARENT);
			TextOut(lpdis->hDC, myRect.left + (tmp - captionWidth) / 2, myRect.bottom - (tmp - captionWidth), Config_GetMenuConfig().sideText, Config_GetMenuConfig().sideTextLength);
			SetTextColor(lpdis->hDC, oldColor);
			SelectObject(lpdis->hDC, pOldFont);
		}

		if (lpdis->itemData == MENU_SPLITTER) {
			memcpy(&myRect, &(lpdis->rcItem), sizeof(RECT));

			//Splitor	
			myRect.bottom -= 2;
			myRect.top += 3;

			FillRect(lpdis->hDC, &myRect, CreateSolidBrush(RGB(191, 191, 191)));
		}
	}
	else
	{
		//Menu Item
		memcpy(&myRect, &(lpdis->rcItem), sizeof(RECT));
		HBRUSH hbrush;
		if (lpdis->itemState & ODS_SELECTED) {
			if (Config_GetMenuConfig().oldStyle){
				SetTextColor(lpdis->hDC, RGB(255, 255, 255));
				hbrush = CreateSolidBrush(GetSysColor(COLOR_MENUHILIGHT));
				FillRect(lpdis->hDC, &myRect, hbrush);
				DeleteObject(hbrush);
			}
			else {
				hbrush = CreateSolidBrush(RGB(221, 236, 255));
				FillRect(lpdis->hDC, &myRect, hbrush);
				DeleteObject(hbrush);
				hbrush = CreateSolidBrush(RGB(38, 160, 218));
				FrameRect(lpdis->hDC, &myRect, hbrush);
				DeleteObject(hbrush);
			}
		}
		else {
			SetTextColor(lpdis->hDC, RGB(0, 0, 0));
			hbrush = CreateSolidBrush(GetSysColor(COLOR_MENU));
			FillRect(lpdis->hDC, &myRect, hbrush);
			DeleteObject(hbrush);
		}

		SetBkMode(lpdis->hDC, TRANSPARENT);

		if (menuTag->icon != NULL)
			DrawIconEx(lpdis->hDC, blankWidth + myRect.left, myRect.top + blankHeight, menuTag->icon, DPI_SCALE(menuTag->iconSize), DPI_SCALE(menuTag->iconSize), 0, NULL, DI_NORMAL);
		//DrawIcon(lpdis->hDC,  BLANKWIDTH + myRect.left, myRect.top + BLANKWIDTH, menuTag->icon);
		//TextOut(lpdis->hDC, blankWidth + DPI_SCALE(menuTag->iconSize) + blankWidth + myRect.left, myRect.top + (blankHeight + DPI_SCALE(menuTag->iconSize) + blankHeight - textHeight) / 2, menuTag->text, lstrlen(menuTag->text));
		SIZE size;
		GetTextExtentPoint32(lpdis->hDC, TEXT("T"), 1, &size);
		TextOut(lpdis->hDC, blankWidth + DPI_SCALE(menuTag->iconSize) + blankWidth + myRect.left, myRect.top + (myRect.bottom - myRect.top - size.cy) / 2, menuTag->text, lstrlen(menuTag->text));
	}


	return 0;
}