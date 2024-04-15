#include "Test.h"

#pragma comment(lib, "Engine.lib")

#if TEST_ENTITY_COMPONENTS
#include "TestEntityComponents.h"
#elif TEST_WINDOW
#include "TestWindow.h"
#elif TEST_RENDERER
#include "TestRenderer.h"
#else
#error One of the tests need to be enabled
#endif

#ifdef _WIN64
#include <Windows.h>
#include <filesystem>

std::filesystem::path set_current_directory_to_executable_path()
{
	wchar_t path[MAX_PATH];

	if (const uint32_t length{ GetModuleFileName(nullptr, &path[0], MAX_PATH) }; !length || GetLastError() == ERROR_INSUFFICIENT_BUFFER) return {};

	const std::filesystem::path p{ path };

	current_path(p.parent_path());

	return std::filesystem::current_path();
}

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int)
{
#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif

	set_current_directory_to_executable_path();

	engine_test test{};

	if (test.initialize())
	{
		MSG msg{};

		bool is_running{ true };

		while (is_running)
		{
			while (PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE))
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);

				is_running &= (msg.message != WM_QUIT);
			}

			test.run();
		}
	}

	test.shutdown();

	return 0;
}

#else
int main()
{
#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif
	engine_test test{};

	if (test.initialize())
	{
		test.run();
	}

	test.shutdown();
}

#endif