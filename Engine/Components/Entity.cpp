#include "Entity.h"
#include "Transform.h"
#include "Script.h"

namespace primal::game_entity {
	namespace
	{
		utl::vector<transform::component>       transforms;
		utl::vector<script::component>          scripts;

		utl::vector<id::generation_type>        generations;
		utl::deque<entity_id>                   free_ids;
	}

	entity create(const entity_info info)
	{
		assert(info.transform);

		if (!info.transform) return entity{};

		entity_id id;

		if (free_ids.size() > id::min_deleted_elements)
		{
			id = free_ids.front();

			assert(!is_alive(id));

			free_ids.pop_front();

			id = entity_id{ id::new_generation(id) };

			++generations[id::index(id)];
		}
		else
		{
			id = entity_id { static_cast<id::id_type>(generations.size()) };

			generations.push_back(0);

			// Resize components
			// NOTE: we don't call resize(), so the number of memory allocations stays low
			transforms.emplace_back();
		}

		const entity new_entity{ id };
		const id::id_type index{ id::index(id) };

		// Create transform component
		assert(!transforms[index].is_valid());
		transforms[index] = transform::create(*info.transform, new_entity);
		if (!transforms[index].is_valid()) return {};

		// Create script component
		if (info.script && info.script->script_creator)
		{
			assert(!scripts[index].is_valid());
			scripts[index] = script::create(*info.script, new_entity);
			assert(scripts[index].is_valid());
		}

		return new_entity;
	}

	void
		remove(const entity_id id)
	{
		const id::id_type index{ id::index(id) };

		assert(is_alive(id));

		transform::remove(transforms[index]);

		transforms[index] = {};

		free_ids.push_back(id);
	}

	bool
		is_alive(const entity_id id)
	{
		assert(id::is_valid(id));

		const id::id_type index{ id::index(id) };

		assert(index < generations.size());
		assert(generations[index] == id::generation(id));

		return (generations[index] == id::generation(id) && transforms[index].is_valid());
	}

	transform::component entity::transform() const
	{
		assert(is_alive(id_));

		const id::id_type index{ id::index(id_) };

		return transforms[index];
	}

	script::component entity::script() const
	{
		assert(is_alive(id_));

		const id::id_type index{ id::index(id_) };

		return scripts[index];
	}
}