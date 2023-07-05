#include "Transform.h"
#include "Entity.h"

namespace primal::transform
{
	namespace
	{
		util::vector<math::v3> positions;
		util::vector<math::v4 > rotations;
		util::vector<math::v3> scales;
	}

	component create_transform(const init_info& info, game_entity::entity entity)
	{
		//assert(entity.is_valid());

		const id::id_type entity_index{ id::index(entity.get_id()) };

		if (positions.size() > entity_index)
		{
			rotations[entity_index] = math::v4(info.rotation);
			positions[entity_index] = math::v3(info.position);
			scales[entity_index] = math::v3(info.scale);
		}
		else
		{
			//assert(positions.size() == entity_index);

			rotations.emplace_back(info.rotation);
			positions.emplace_back(info.position);
			scales.emplace_back(info.scale);
		}

		return component(transform_id{ static_cast<id::id_type>(entity_index) - 1 });
	}

	void remove_transform(const component component)
	{
		//assert(component.is_valid());
	}

	math::v3 component::position() const
	{
		//assert(is_valid());
		return positions[id::index(id_)];
	}

	math::v4 component::rotation() const
	{
		//assert(is_valid());
		return rotations[id::index(id_)];
	}

	math::v3 component::scale() const
	{
		//assert(is_valid());
		return scales[id::index(id_)];
	}
}