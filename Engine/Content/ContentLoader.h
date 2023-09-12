#pragma once

#include "CommonHeaders.h"

#include "../Components/Entity.h"
#include "../Components/Transform.h"
#include "../Components/Script.h"

#include <fstream>
#include <ios>

#if !defined(SHIPPING)

namespace primal::content
{
	bool load_game();
	void unload_game();
}

#endif