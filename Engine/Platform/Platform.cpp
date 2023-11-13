#include "Platform.h"
#include "PlatformTypes.h"

namespace primal::platform
{
#ifdef _WIN64
	namespace
	{
		struct window_info
		{
			HWND hwnd{ nullptr };
			RECT client_area{ 0,0,1920,1080 };
			RECT fullscreen_area{};
			POINT top_left{ 0,0 };
			DWORD style{ WS_VISIBLE };
			bool is_fullscreen{ false };
			bool is_closed{ false };
		};

		utl::vector<window_info> windows;

		utl::vector<u32> available_slots;

		u32 add_to_windows(window_info info)
		{
			u32 id{ u32_invalid_id };

			if (available_slots.empty())
			{
				id = static_cast<id::id_type>(windows.size());

				windows.emplace_back(info);
			}
			else
			{
				id = available_slots.back();

				available_slots.pop_back();

				assert(id != u32_invalid_id);

				windows[id] = info;
			}

			return id;
		}

		void remove_from_windows(u32 id)
		{
			assert(id < windows.size());

			available_slots.emplace_back(id);
		}

		window_info& get_from_id(const window_id id)
		{
			assert(id < windows.size());
			assert(windows[id].hwnd);

			return windows[id];
		}

		window_info& get_from_handle(window_handle handle)
		{
			const window_id id { static_cast<id::id_type>(GetWindowLongPtr(handle, GWLP_USERDATA)) };

			return get_from_id(id);
		}

		LRESULT CALLBACK internal_window_proc(const HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
		{
			window_info* info{ nullptr };

			switch (msg)
			{
			case WM_DESTROY:
				get_from_handle(hwnd).is_closed = true;
				break;
			default: 
				break;
			}

			const LONG_PTR long_ptr{ GetWindowLongPtr(hwnd, 0) };

			return long_ptr
				? reinterpret_cast<window_proc>(long_ptr)(hwnd, msg, wparam, lparam)  // NOLINT(performance-no-int-to-ptr)
				: DefWindowProc(hwnd, msg, wparam, lparam);
		}
	}

	window create_window(const window_init_info* const init_info)
	{
		const window_proc callback{ init_info ? init_info->callback : nullptr };
		const window_handle parent{ init_info ? init_info->parent : nullptr };


		WNDCLASSEX wc;

		ZeroMemory(&wc, sizeof(wc));

		wc.cbSize = sizeof(WNDCLASSEX);
		wc.style = CS_HREDRAW | CS_VREDRAW;
		wc.lpfnWndProc = internal_window_proc;
		wc.cbClsExtra = 0;
		wc.cbWndExtra = callback ? sizeof(callback) : 0;
		wc.hInstance = nullptr;
		wc.hIcon = LoadIcon(nullptr, IDI_APPLICATION);
		wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
		wc.hbrBackground = CreateSolidBrush(RGB(26, 48, 76));
		wc.lpszMenuName = nullptr;
		wc.lpszClassName = L"PrimalWindow";
		wc.hIconSm = LoadIcon(nullptr, IDI_APPLICATION);

		RegisterClassEx(&wc);

		window_info info{};

		RECT rc{ info.client_area };

		AdjustWindowRect(&rc, info.style, FALSE);


		const wchar_t* caption{ (init_info && init_info->caption) ? init_info->caption : L"Primal Game" };

		const s32 left{ (init_info && init_info->left) ? init_info->left : info.client_area.left };
		const s32 top{ (init_info && init_info->top) ? init_info->top : info.client_area.top };

		const s32 width{ (init_info && init_info->width) ? init_info->width : rc.right - rc.left };
		const s32 height{ (init_info && init_info->height) ? init_info->height : rc.bottom - rc.top };

		info.style |= parent ? WS_CHILD : WS_OVERLAPPED;

		info.hwnd = CreateWindowEx(
			0,
			wc.lpszClassName,
			caption,
			info.style,
			left, top,
			width, height,
			parent,
			nullptr,
			nullptr,
			nullptr
		);

		if (info.hwnd)
		{
			const window_id id{ add_to_windows(info) };

			SetWindowLongPtr(info.hwnd, GWLP_USERDATA, (LONG_PTR) id);

			if (callback) SetWindowLongPtr(info.hwnd, 0, reinterpret_cast<LONG_PTR>(callback));

			assert(GetLastError() == 0);

			ShowWindow(info.hwnd, SW_SHOWNORMAL);
			UpdateWindow(info.hwnd);

			return window{ id };
		}

		return {};
	}

	void remove_window(const window_id id)
	{
		// ReSharper disable once CppUseStructuredBinding
		const window_info& info{ get_from_id(id) };

		DestroyWindow(info.hwnd);

		remove_from_windows(id);
	}

	void resize_window(const window_info& info, const RECT& area)
	{
		RECT window_rect{ area };

		AdjustWindowRect(&window_rect, info.style, FALSE);

		const s32 width{ window_rect.right - window_rect.left };
		const s32 height{ window_rect.bottom - window_rect.top };

		MoveWindow(info.hwnd, info.top_left.x, info.top_left.y, width, height, true);
	}

	void set_window_fullscreen(const window_id id, const bool is_fullscreen)
	{
		window_info& info{ get_from_id(id) };

		if (info.is_fullscreen != is_fullscreen)
		{
			info.is_fullscreen = is_fullscreen;

			if (is_fullscreen)
			{
				GetClientRect(info.hwnd, &info.client_area);

				RECT rect;

				GetWindowRect(info.hwnd, &rect);

				info.top_left.x = rect.left;
				info.top_left.y = rect.top;

				info.style = 0;

				SetWindowLongPtr(info.hwnd, GWL_STYLE, info.style);
				ShowWindow(info.hwnd, SW_MAXIMIZE);
			}
			else
			{
				info.style = WS_VISIBLE | WS_OVERLAPPEDWINDOW;

				SetWindowLongPtr(info.hwnd, GWL_STYLE, info.style);

				resize_window(info, info.client_area);

				ShowWindow(info.hwnd, SW_SHOWNORMAL);
			}
		}
	}

	bool is_window_fullscreen(const window_id id)
	{
		return get_from_id(id).is_fullscreen;
	}

	window_handle get_window_handle(const window_id id)
	{
		return get_from_id(id).hwnd;
	}

	void set_window_caption(const window_id id, const wchar_t* caption)
	{
		const window_info& info{ get_from_id(id) };

		SetWindowText(info.hwnd, caption);
	}

	math::u32v4 get_window_size(const window_id id)
	{
		const window_info& info{ get_from_id(id) };

		const RECT area{ info.is_fullscreen ? info.fullscreen_area : info.client_area };

		return { static_cast<u32>(area.left), static_cast<u32>(area.top), static_cast<u32>(area.right), static_cast<u32>(area.bottom) };
	}

	void resize_window(const window_id id, const u32 width, const u32 height)
	{
		window_info& info{ get_from_id(id) };

		RECT& area{ info.is_fullscreen ? info.fullscreen_area : info.client_area };

		area.bottom = area.top + height;
		area.right = area.left + width;

		resize_window(info, area);
	}

	bool is_window_closed(const window_id id)
	{
		return get_from_id(id).is_closed;
	}
#elif
#error "must implement at least one platform."
#endif

	void window::set_fullscreen(const bool is_fullscreen) const
	{
		assert(is_valid());

		set_window_fullscreen(id_, is_fullscreen);
	}

	bool window::is_fullscreen() const
	{
		assert(is_valid());

		return is_window_fullscreen(id_);
	}

	void* window::handle() const
	{
		assert(is_valid());

		return get_window_handle(id_);
	}

	void window::set_caption(const wchar_t* caption) const
	{
		assert(is_valid());

		set_window_caption(id_, caption);
	}

	math::u32v4 window::size() const
	{
		assert(is_valid());

		return get_window_size(id_);
	}

	void window::resize(const u32 width, const u32 height) const
	{
		assert(is_valid());

		resize_window(id_, width, height);
	}

	u32 window::width() const
	{
		const math::u32v4 s{ size() };

		return s.z - s.y;
	}

	u32 window::height() const
	{
		const math::u32v4 s{ size() };

		return s.w - s.y;
	}

	bool window::is_closed() const
	{
		assert(is_valid());

		return is_window_closed(id_);
	}
}