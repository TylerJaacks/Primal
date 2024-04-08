#include "CommonHeaders.h"
#include "D3D12Interface.h"
#include "Graphics/GraphicsPlatformInterface.h"
#include "D3D12Core.h"

namespace primal::graphics::d3d12
{
	void get_platform_interface(platform_interface& platform)
	{
		platform.initialize = core::initialize;
		platform.render = core::render;
		platform.shutdown = core::shutdown;
	}
}