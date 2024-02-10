#include "Geometry.h"

namespace primal::tools
{
	using namespace math;
	using namespace DirectX;

	namespace
	{
		void recalculate_normals(mesh& m)
		{
			const u32 num_indices{ static_cast<u32>(m.raw_indices.size()) };

			m.normals.reserve(num_indices);

			for (u32 i{0}; i < num_indices; ++i)
			{
				const u32 i0{ m.raw_indices[i] };
				const u32 i1{ m.raw_indices[++i] };
				const u32 i2{ m.raw_indices[++i] };

				const XMVECTOR v0{ XMLoadFloat3(&m.positions[i0]) };
				const XMVECTOR v1{ XMLoadFloat3(&m.positions[i1]) };
				const XMVECTOR v2{ XMLoadFloat3(&m.positions[i2]) };

				const XMVECTOR e0{ v1 - v0 };
				const XMVECTOR e1{ v2 - v0 };

				const XMVECTOR n{ XMVector3Normalize(XMVector3Cross(e0, e1)) };

				XMStoreFloat3(&m.normals[i], n);

				m.normals[i - 1] = m.normals[i];
				m.normals[i - 2] = m.normals[i];
			}
		}

		void process_normals(mesh& m, const f32 smoothing_angle)
		{
			const f32 cos_angle{ XMScalarCos(pi - smoothing_angle * pi / 180.0f) };

			const bool is_hard_edge{ XMScalarNearEqual(smoothing_angle, 180.0f, epsilon) };
			const bool is_soft_edge{ XMScalarNearEqual(smoothing_angle, 0.0f, epsilon) };

			const u32 num_indices{ static_cast<u32>(m.raw_indices.size()) };
			const u32 num_vertices{ static_cast<u32>(m.positions.size()) };

			assert(num_indices && num_vertices);

			m.indices.resize(num_indices);

			utl::vector<utl::vector<u32>> idx_ref(num_vertices);

			for (u32 i{0}; i < num_indices; ++i)
			{
				idx_ref[m.raw_indices[i]].emplace_back(i);
			}

			for (u32 i{0}; i < num_vertices; ++i)
			{
				auto& refs{ idx_ref[i] };

				u32 num_refs = { static_cast<u32>(refs.size()) };

				for (u32 j{ 0 }; j < num_refs; ++j)
				{
					m.indices[refs[j]] = static_cast<u32>(m.vertices.size());

					auto& [tangent, position, normal, uv]{ m.vertices.emplace_back() };

					position = m.positions[m.raw_indices[refs[j]]];

					XMVECTOR n1{ XMLoadFloat3(&m.normals[refs[j]]) };

					if (!is_hard_edge)
					{
						for (u32 k{j + 1}; k < num_refs; ++k)
						{
							f32 n{ 0.0f };

							const XMVECTOR n2{ XMLoadFloat3(&m.normals[refs[k]]) };

							if (!is_soft_edge)
							{
								XMStoreFloat(&n, XMVector3Dot(n1, n2) * XMVector3ReciprocalLength(n1));
							}

							if (is_soft_edge || n >= cos_angle)
							{
								n1 += n2;

								m.indices[refs[k]] = m.indices[refs[j]];

								refs.erase(refs.begin() + k);

								--num_refs;
								--k;
							}
						}
					}

					XMStoreFloat3(&normal, XMVector3Normalize(n1));
				}
			}
		}

		void process_vertices(mesh& m, const geometry_import_settings& settings)
		{
			assert((m.raw_indices.size() % 3) == 0);

			if (settings.calculate_normals || m.normals.empty())
			{
				recalculate_normals(m);
			}

			process_normals(m, settings.smoothing_angle);

			if (!m.uv_sets.empty())
			{
				process_uvs(m);
			}

			pack_vertices_static(m);
		}
	}

	void process_scene(scene& scene, const geometry_import_settings& settings)
	{
		for (auto& [name, meshes]: scene.lod_groups)
		{
			for (auto& m : meshes)
			{
				process_vertices(m, settings);
			}
		}
	}

	void pack_data(const scene& scene, scene_data& data)
	{

	}
}