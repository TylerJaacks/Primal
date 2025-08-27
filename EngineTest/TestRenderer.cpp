// ReSharper disable All
#include "TestRenderer.h"

#ifdef TEST_RENDERER

#include "../Platform/PlatformTypes.h"
#include "../Platform/Platform.h"
#include "../Graphics/Renderer.h"

using namespace primal;

graphics::render_surface _surfaces[4];

time_it timer{};

bool resized{ false };
bool is_restarting{ false };
void destroy_render_surface(graphics::render_surface& surface);
bool test_initialize();
void test_shutdown();

LRESULT WinProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
{
	bool toggle_fullscreen{false};

	switch (msg)
	{
	case WM_DESTROY:
	{
		bool all_closed{ true };
		for (u32 i{ 0 }; i < _countof(_surfaces); ++i)
		{
			if (_surfaces[i].window.is_valid())
			{
				if (_surfaces[i].window.is_closed())
				{
					destroy_render_surface(_surfaces[i]);
				}
				else
				{
					all_closed = false;
				}
			}
		}
		if (all_closed && !is_restarting)
		{
			PostQuitMessage(0);
			return 0;
		}
	}
	break;
	case WM_SIZE:
		resized = (wparam != SIZE_MINIMIZED);
		break;
	case WM_SYSCHAR:
		toggle_fullscreen = wparam == VK_RETURN && (HIWORD(lparam) & KF_ALTDOWN);
		break;
	case WM_KEYDOWN:
		if (wparam == VK_ESCAPE)
		{
			PostMessage(hwnd, WM_CLOSE, 0, 0);
			return 0;
		}
		else if (wparam == VK_F11)
		{
			is_restarting = true;
			test_shutdown();
			test_initialize();
		}
	}

	if ((resized && GetAsyncKeyState(VK_LBUTTON) >= 0) || toggle_fullscreen)
	{
		platform::window win{ platform::window_id{(id::id_type)GetWindowLongPtr(hwnd, GWLP_USERDATA)} };

		for (u32 i{ 0 }; i < _countof(_surfaces); ++i)
		{
			if (win.get_id() == _surfaces[i].window.get_id())
			{
				if (toggle_fullscreen)
				{
					win.set_fullscreen(!win.is_fullscreen());

					return 0;
				}
				else
				{
					_surfaces[i].surface.resize(win.width(), win.height());
					resized = false;
				}

				break;
			}
		}
	}

	return DefWindowProc(hwnd, msg, wparam, lparam);
}

void create_render_surfaces(graphics::render_surface& surface, platform::window_init_info info)
{
	surface.window = platform::create_window(&info);
	surface.surface = graphics::create_surface(surface.window);
}

void destroy_render_surface(graphics::render_surface& surface)
{
	graphics::render_surface temp{ surface };
	surface = {};

	if (temp.surface.is_valid())graphics::remove_surface(temp.surface.get_id());
	if (temp.window.is_valid())platform::remove_window(temp.window.get_id());
}

void test_shutdown()
{
	for (u32 i{ 0 }; i < _countof(_surfaces); ++i)
		destroy_render_surface(_surfaces[i]);

	graphics::shutdown();
}

bool test_initialize()
{
	while (!compile_shaders())
	{
		if (MessageBox(nullptr, L"Failed to compile engine shaders.", L"Shader Compliation Error", MB_RETRYCANCEL) != IDRETRY)
			return false;
	}

	if (!graphics::initialize(graphics::graphics_platform::direct3d12)) return false;

	platform::window_init_info info[]
	{
		{&WinProc, nullptr, L"Render Window 1", 100, 100, 400, 800 },
		{&WinProc, nullptr, L"Render Window 2", 150, 150, 800, 400 },
		{&WinProc, nullptr, L"Render Window 3", 200, 200, 400, 400 },
		{&WinProc, nullptr, L"Render Window 4", 250, 250, 800, 800 },
	};

	static_assert(_countof(info) == _countof(_surfaces));

	for (u32 i{ 0 }; i < _countof(_surfaces); ++i)
		create_render_surfaces(_surfaces[i], info[i]);

	is_restarting = false;

	return true;
}

bool engine_test::initialize()
{
	return test_initialize();
}

void engine_test::run()
{
	timer.begin();

	//std::this_thread::sleep_for(std::chrono::microseconds(10));

	for (u32 i{ 0 }; i < _countof(_surfaces); ++i)
	{
		if (_surfaces[i].surface.is_valid())
		{
			_surfaces[i].surface.render();
		}
	}

	timer.end();
}

void engine_test::shutdown()
{
	test_shutdown();
}

#endif