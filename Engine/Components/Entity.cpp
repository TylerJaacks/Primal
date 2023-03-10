#include "Entity.h"
#include "Transform.h"

namespace primal::game_entity
{
	namespace
	{
		util::vector<transform::component> transforms;

		util::vector<id::generation_type> generations;
		util::deque<entity_id> free_ids;
	}

	entity create_game_entity(const entity_info& info)
	{
		assert(info.transform);

		entity_id id;

		if (!info.transform) return entity { };

		if (free_ids.size() > id::min_deleted_elements)
		{
			id = free_ids.front();

			assert(!is_active(entity{ id }));

			free_ids.pop_front();

			id = entity_id { id::new_generation(id) };

			++generations[id::index(id)];
		}
		else
		{
			id = entity_id { static_cast<id::id_type>(generations.size()) };

			generations.push_back(0);
			transforms.emplace_back();
		}

		const entity new_entity { id };
		const id::id_type index { id::index(id) };

		assert(!transforms[index].is_valid());

		transforms[index] = transform::create_transform(*info.transform, new_entity);

		if (!transforms[index].is_valid()) return { };
 
		return new_entity;
	}

	void remove_game_entity(entity entity)
	{
		const entity_id id { entity.get_id() };
		const id::id_type index { id::index(id) };

		assert(is_active(entity));

		if (is_active(entity))
		{
			transform::remove_transform(transforms[id::index(id)]);

			transforms[index] = { };

			free_ids.push_back(id);
		}
	}

	bool game_entity::is_active(entity entity)
	{
		const entity_id id{ entity.get_id() };
		const id::id_type index{ id::index(id) };

		assert(index < generations.size());
		assert(generations[index] == id::generation(id));

		return (generations[index] == id::generation(id) && transforms[index].is_valid());
	}

	transform::component entity::transform() const
	{
		assert(is_active(*this));

		const id::id_type index { id::index(id_) };

		return transforms[index];
	}
}