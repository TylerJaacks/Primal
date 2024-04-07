#include "CommonHeaders.h"
#include "D3D12Interface.h"
#include "Graphics/GraphicsPlatformInterface.h"

namespace primal::graphics::d3d12
{
	void get_platform_interface(platform_interface& platform)
	{
		platform.initialize = core::initialize;
		platform.shutdown = core::shutdown;
	}
}