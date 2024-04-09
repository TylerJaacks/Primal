#include "CommonHeaders.h"
#include "D3D12Interface.h"
#include "Graphics/GraphicsPlatformInterface.h"
#include "D3D12Core.h"

namespace primal::graphics::d3d12
{
	void get_platform_interface(platform_interface& platform)
	{
		platform.initialize = core::initialize;
		platform.shutdown = core::shutdown;

		platform.surface.create = core::create_surface;
		platform.surface.remove = core::remove_surface;
		platform.surface.resize = core::resize_surface;
		platform.surface.width = core::surface_width;
		platform.surface.height = core::surface_height;
		platform.surface.render = core::render_surface;
	}
}