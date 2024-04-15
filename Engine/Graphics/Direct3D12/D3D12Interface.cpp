#include "CommonHeaders.h"
#include "D3D12Interface.h"
#include "Graphics/GraphicsPlatformInterface.h"
#include "D3D12Core.h"

namespace primal::graphics::d3d12
{
	void get_platform_interface(platform_interface& platform_interface)
	{
		platform_interface.initialize = core::initialize;
		platform_interface.shutdown = core::shutdown;

		platform_interface.surface.create = core::create_surface;
		platform_interface.surface.remove = core::remove_surface;
		platform_interface.surface.resize = core::resize_surface;
		platform_interface.surface.width = core::surface_width;
		platform_interface.surface.height = core::surface_height;
		platform_interface.surface.render = core::render_surface;

		platform_interface.platform = graphics_platform::direct3d12;
	}
}