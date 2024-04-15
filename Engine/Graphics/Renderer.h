#pragma once
#include "CommonHeaders.h"
#include "../Platform/Window.h"

namespace primal::graphics
{
	DEFINE_TYPED_ID(surface_id);

	class surface
	{
	public:
		constexpr explicit surface(const surface_id id) : id_{ id } {}
		constexpr surface() = default;
		constexpr surface_id get_id() const { return id_; }
		constexpr bool is_valid() const { return id::is_valid(id_); }

		void resize(u32 width, u32 height) const;

		u32 width() const;
		u32 height() const;
		
		void render() const;
	private:
		surface_id id_{ id::invalid_id };
	};

	struct render_surface
	{
		platform::window window{};
		surface surface{};
	};

	enum class graphics_platform : u32
	{
		direct3d12 = 0,
	};

	bool initialize(graphics_platform platform);
	void shutdown();

	const char* get_engine_shaders_path();
	const char* get_engine_shaders_path(graphics_platform platform);

	surface create_surface(platform::window window);
	void remove_surface(surface_id id);
}