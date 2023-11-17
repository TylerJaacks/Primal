#pragma once
#include "ToolsCommon.h"

namespace primal::tools
{
	enum primitives_mesh_type : u32
	{
		plane,
		cube,
		uv_sphere,
		ico_sphere,
		cylinder,
		capsule,

		count
	};

	struct primitive_init_info
	{
		primitives_mesh_type type;
		u32 segments[3]{ 1, 1, 1 };
		math::v3 size{ 1, 1, 1 };
		u32 lod{ 0 };

	};
}