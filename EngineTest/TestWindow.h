// ReSharper disable CppClangTidyHicppMultiwayPathsCovered
// ReSharper disable CppParameterMayBeConst
#pragma once
#include "Test.h"
#include "../Platform/PlatformTypes.h"
#include "../Platform/Platform.h"

using namespace primal;

inline platform::window windows[4];

inline LRESULT WinProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
{
	switch (msg)
	{
	case WM_DESTROY:
	{
		bool all_closed{ true };
		for (u32 i{ 0 }; i < _countof(windows); i++)
		{
			if (!windows[i].is_closed())
			{
				all_closed = false;
			}
		}

		if (all_closed)
		{
			PostQuitMessage(0);

			return 0;
		}
	}
	case WM_SYSCHAR:
		if (wparam == VK_RETURN && (HIWORD(lparam) & KF_ALTDOWN))
		{
			const platform::window win{ platform::window_id{static_cast<id::id_type>(GetWindowLongPtr(hwnd, GWLP_USERDATA))} };

			win.set_fullscreen(!win.is_fullscreen());

			return 0;
		}
	break;
default: 
	break;
	}

	return DefWindowProc(hwnd, msg, wparam, lparam);
}

class engine_test : test
{
public:
	bool initialize() override
	{
		platform::window_init_info info[]
		{
			{&WinProc, nullptr, L"Test Window 1", 100, 100, 400, 800 },
			{&WinProc, nullptr, L"Test Window 2", 150, 150, 800, 400 },
			{&WinProc, nullptr, L"Test Window 3", 200, 200, 400, 400 },
			{&WinProc, nullptr, L"Test Window 4", 250, 250, 800, 800 },
		};

		static_assert(_countof(info) == _countof(windows));

		for (u32 i{ 0 }; i < _countof(windows); ++i)
			windows[i] = platform::create_window(&info[i]);

		return true;
	}

	void run() override
	{
		std::this_thread::sleep_for(std::chrono::microseconds(10));
	}

	void shutdown() override
	{
		for (u32 i{ 0 }; i < _countof(windows); ++i)
		{
			platform::remove_window(windows[i].get_id());
		}
	}
};