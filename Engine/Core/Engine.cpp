#if !defined(SHIPPING)
#include <thread>

#include "../Content/ContentLoader.h"

bool engine_initialize()
{
	const bool result { primal::content::load_game() };

	return result;
}

void engine_update()
{
	primal::script::update(10.f);
	std::this_thread::sleep_for(std::chrono::microseconds(10));
}

void engine_shutdown()
{
	primal::content::unload_game();
}
#endif