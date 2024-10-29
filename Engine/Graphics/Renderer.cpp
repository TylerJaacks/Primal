#include "Renderer.h"
#include "GraphicsPlatformInterface.h"
#include "Direct3D12/D3D12Interface.h"

namespace primal::graphics
{
	namespace
	{
		constexpr const char* engine_shader_paths[]
		{
			R"(.\shaders\d3d12\shaders.bin)"
		};

		platform_interface gfx{};

		bool set_platform_interface(graphics_platform platform)
		{
			switch (platform)
			{
			case graphics_platform::direct3d12:
				d3d12::get_platform_interface(gfx);

				break;
			default:
				return false;
			}

			assert(gfx.platform == platform);

			return true;
		}
	}

	bool initialize(graphics_platform platform)
	{
		return set_platform_interface(platform) && gfx.initialize();
	}

	void shutdown()
	{
		if (gfx.platform != static_cast<graphics_platform>(-1)) gfx.shutdown();
	}

	const char* get_engine_shaders_path()
	{
		return engine_shader_paths[static_cast<u32>(gfx.platform)];
	}

	const char* get_engine_shaders_path(graphics_platform platform)
	{
		return engine_shader_paths[static_cast<u32>(platform)];
	}

	surface create_surface(platform::window window)
	{
		return gfx.surface.create(window);
	}

	void remove_surface(surface_id id)
	{
		assert(id::is_valid(id));

		gfx.surface.remove(id);
	}

	void surface::resize(u32 width, u32 height) const
	{
		assert(is_valid());

		gfx.surface.resize(id_, width, height);
	}

	u32 surface::width() const
	{
		assert(is_valid());

		return gfx.surface.width(id_);
	}

	u32 surface::height() const
	{
		assert(is_valid());

		return gfx.surface.height(id_);
	}

	void surface::render() const
	{
		assert(is_valid());

		gfx.surface.render(id_);
	}
}