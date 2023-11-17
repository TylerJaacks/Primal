// ReSharper disable CppInconsistentNaming
#include "Common.h"
#include "CommonHeaders.h"

#include "../Engine/Components/Script.h"
#include "../Engine/Platform/Platform.h"
#include "../Engine/Platform/PlatformTypes.h"
#include "../Engine/Graphics/Renderer.h"

#ifndef WIN32_MEAN_AND_LEAN
#define WIN32_MEAN_AND_LEAN
#endif

#include <Windows.h>

using namespace primal;

namespace
{
	HMODULE game_code_dll { nullptr };

	using _get_script_creator = primal::script::detail::script_creator(*)(size_t);

	_get_script_creator get_script_creator { nullptr };

	using _get_script_names = LPSAFEARRAY(*)(void);

	_get_script_names get_script_names { nullptr };

	utl::vector<graphics::render_surface> surfaces;
}

EDITOR_INTERFACE u32 LoadGameCodeDll(const char* dll_path)
{
	if (game_code_dll) return 0;

	game_code_dll = LoadLibraryA(dll_path);

	assert(game_code_dll);

	get_script_creator = reinterpret_cast<_get_script_creator>(GetProcAddress(game_code_dll, "get_script_creator"));
	get_script_names = reinterpret_cast<_get_script_names>(GetProcAddress(game_code_dll, "get_script_names"));

	return (game_code_dll && get_script_creator && get_script_names) ? TRUE : FALSE;
}

EDITOR_INTERFACE u32 UnloadGameCodeDll()
{
	if (!game_code_dll) return FALSE;

	assert(game_code_dll);

	const int result = FreeLibrary(game_code_dll);

	assert(result);

	game_code_dll = nullptr;

	return TRUE;
}

EDITOR_INTERFACE script::detail::script_creator GetScriptCreator(const char* name)
{
	return (game_code_dll && get_script_creator) ? get_script_creator(script::detail::string_hash()(name)) : nullptr;
}

EDITOR_INTERFACE LPSAFEARRAY GetScriptNames()
{
	return (game_code_dll && get_script_names) ? get_script_names() : nullptr;
}

EDITOR_INTERFACE u32 CreateRenderSurface(const HWND host, const s32 width, const s32 height)
{
	assert(host);

	const platform::window_init_info info{ nullptr, host, nullptr, 0, 0, width, height };
	graphics::render_surface surface{ platform::create_window(&info), {} };

	assert(surface.window.is_valid());

	surfaces.emplace_back(surface);

	return static_cast<u32>(surfaces.size()) - 1;
}

EDITOR_INTERFACE void RemoveRenderSurface(const u32 id)
{
	assert(id < surfaces.size());

	platform::remove_window(surfaces[id].window.get_id());
}

EDITOR_INTERFACE HWND GetWindowHandle(const u32 id)
{
	assert(id < surfaces.size());

	return static_cast<HWND>(surfaces[id].window.handle());
}

EDITOR_INTERFACE void ResizeRenderSurface(const u32 id)
{
	assert(id < surfaces.size());

	surfaces[id].window.resize(0, 0);
}
