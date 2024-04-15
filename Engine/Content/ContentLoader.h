#pragma once

#include "CommonHeaders.h"
#include "Graphics/Renderer.h"

#include "../Components/Entity.h"
#include "../Components/Transform.h"
#include "../Components/Script.h"

#include <filesystem>
#include <fstream>
#include <ios>

#if !defined(SHIPPING)

namespace primal::content
{
	bool load_game();
	void unload_game();

	bool load_engine_shaders(std::unique_ptr<u8[]>& shaders, u64& size);
}

#endif