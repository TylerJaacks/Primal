#if !defined(SHIPPING)
#include <thread>

#include "../Components/Script.h"

#include "../Content/ContentLoader.h"

#include "../Graphics/Renderer.h"

#include "../Platform/Platform.h"
#include "../Platform/PlatformTypes.h"

using namespace primal;

namespace
{
	graphics::render_surface game_window{};

	LRESULT win_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
	{
		switch (msg)
		{
		case WM_DESTROY:
		{
			if (game_window.window.is_closed())
			{
				PostQuitMessage(0);

				return 0;
			}
		}
		case WM_SYSCHAR:
			if (wparam == VK_RETURN && (HIWORD(lparam) & KF_ALTDOWN))
			{
				game_window.window.set_fullscreen(!game_window.window.is_fullscreen());

				return 0;
			}
			break;
		default:
			break;
		}

		return DefWindowProc(hwnd, msg, wparam, lparam);
	}

}

bool engine_initialize()
{
	if (!content::load_game()) return false;

	constexpr platform::window_init_info info
	{
		&win_proc, nullptr, L"Primal Game"
	};

	game_window.window = platform::create_window(&info);

	if (!game_window.window.is_valid()) return false;

	return true;
}

void engine_update()
{
	script::update(10.f);
	std::this_thread::sleep_for(std::chrono::microseconds(10));
}

void engine_shutdown()
{
	platform::remove_window(game_window.window.get_id());
	content::unload_game();
}
#endif