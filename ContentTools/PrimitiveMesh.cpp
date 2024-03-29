﻿// ReSharper disable CppInconsistentNaming
#include "Geometry.h"
#include "PrimitiveMesh.h"

namespace primal::tools
{
	namespace
	{
		using primitive_mesh_creator = void(*)(scene&, const primitive_init_info& info);

		void create_plane(scene& scene, const primitive_init_info& info);
		void create_cube(scene& scene, const primitive_init_info& info);
		void create_uv_sphere(scene& scene, const primitive_init_info& info);
		void create_ico_sphere(scene& scene, const primitive_init_info& info);
		void create_cylinder(scene& scene, const primitive_init_info& info);
		void create_capsule(scene& scene, const primitive_init_info& info);

		primitive_mesh_creator creators[]
		{
			create_plane,
			create_cube,
			create_uv_sphere,
			create_ico_sphere,
			create_cylinder,
			create_capsule
		};

		static_assert(_countof(creators) == primitives_mesh_type::count);

		struct axis
		{
			enum : u32 {
				x = 0,
				y = 1,
				z = 2
			};
		};

		mesh create_plane(const primitive_init_info& info, const u32 horizontal_index = axis::x, const u32 vertical_index = axis::z, bool flip_winding = false, const math::v3 offset = {-0.5f, 0.0f, -0.5f}, const math::v2 u_range = {0.0f, 1.0f}, const math::v2 v_range = { 0.0f, 1.0f })
		{
			assert(horizontal_index < 3 && vertical_index < 3);
			assert(horizontal_index != vertical_index);

			const u32 horizontal_count{math::clamp(info.segments[horizontal_index], 1u, 10u) };
			const u32 vertical_count{math::clamp(info.segments[vertical_index], 1u, 10u) };

			const f32 horizontal_step{ 1.0f / horizontal_count };
			const f32 vertical_step{ 1.0f / vertical_count };

			const f32 u_step{ (u_range.y - u_range.x) / horizontal_count };
			const f32 v_step{ (v_range.y - v_range.x) / vertical_count };

			mesh mesh{};

			utl::vector<math::v2> uvs;

			for (u32 j{0}; j <= vertical_count; ++j)
			{
				for (u32 i{ 0 }; i <= horizontal_count; ++i)
				{
					math::v3 position{ offset };
					f32* const as_array{ &position.x };

					as_array[horizontal_index] += i * horizontal_step;
					as_array[vertical_index] += j * vertical_step;

					mesh.positions.emplace_back(position.x * info.size.x, position.y * info.size.y, position.z * info.size.z);

					math::v2 uv{ u_range.x, 1.0f - v_range.x };

					uv.x += i * u_step;
					uv.y -= j * v_step;

					uvs.emplace_back(uv);
				}
			}

			assert(mesh.positions.size() == (((u64)horizontal_count + 1) * ((u64)vertical_count + 1)));

			const u32 row_length{ horizontal_count + 1 };

			for (u32 j{ 0 }; j < vertical_count; ++j)
			{
				u32 k{ 0 };

				for (u32 i{k}; i < horizontal_count; ++i)
				{
					const u32 index[4]
					{
						i + j * row_length,
						i + (j + 1) * row_length,
						(i + 1) + j * row_length,
						(i + 1) + (j + 1) * row_length,
					};

					mesh.raw_indices.emplace_back(index[0]);
					mesh.raw_indices.emplace_back(index[flip_winding ? 2 : 1]);
					mesh.raw_indices.emplace_back(index[flip_winding ? 1 : 2]);

					mesh.raw_indices.emplace_back(index[2]);
					mesh.raw_indices.emplace_back(index[flip_winding ? 3 : 1]);
					mesh.raw_indices.emplace_back(index[flip_winding ? 1 : 3]);
				}

				++k;
			}

			const u32 num_indices{ 3 * 2 * horizontal_count * vertical_count };

			assert(m.raw_indices.size() == num_indices);

			mesh.uv_sets.resize(1);

			for (u32 i{0}; i < num_indices; ++i)
			{
				mesh.uv_sets[0].emplace_back(uvs[mesh.raw_indices[i]]);
			}

			return mesh;
		}

		void create_plane(scene& scene, const primitive_init_info& info)
		{
			lod_group lod{};

			lod.name = "plane";
			lod.meshes.emplace_back(create_plane(info));

			scene.lod_groups.emplace_back(lod);
		}

		void create_cube(scene& scene, const primitive_init_info& info) {}
		void create_uv_sphere(scene& scene, const primitive_init_info& info) {}
		void create_ico_sphere(scene& scene, const primitive_init_info& info) {}
		void create_cylinder(scene& scene, const primitive_init_info& info) {}
		void create_capsule(scene& scene, const primitive_init_info& info) {}
	}

	EDITOR_INTERFACE void CreatePrimitiveMesh(scene_data* data, const primitive_init_info* info)
	{
		assert(data && info);
		assert(info->type < primitives_mesh_type::count);

		scene scene{};

		creators[info->type](scene, *info);

		data->settings.calculate_normals = 1;

		process_scene(scene, data->settings);
		pack_data(scene, *data);
	}
}
