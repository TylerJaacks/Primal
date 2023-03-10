#pragma once

#include "../EngineAPI/Components/TransformComponent.h"

namespace primal::game_entity
{
	DEFINE_TYPED_ID(entity_id);

	class entity
	{
	public:
		constexpr explicit entity(entity_id id) : id_{ id } { }
		constexpr entity() : id_{ id::invalid_id } { }

		[[nodiscard]] constexpr entity_id get_id() const { return id_; }
		[[nodiscard]] constexpr bool is_valid() const { return id_ != id::invalid_id; }

		[[nodiscard]] transform::component transform() const;

	private:
		entity_id id_;
	};
}