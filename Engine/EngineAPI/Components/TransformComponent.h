#pragma once

namespace primal::transform
{
	DEFINE_TYPED_ID(transform_id);

	class component final
	{
	public:
		constexpr explicit component(transform_id id) : id_{ id } { }
		constexpr component() : id_{ id::invalid_id } { }

		[[nodiscard]] constexpr transform_id get_id() const { return id_; }
		[[nodiscard]] constexpr bool is_valid() const { return id_ != id::invalid_id; }

		[[nodiscard]] math::v4 rotation() const;
		[[nodiscard]] math::v3 position() const;
		[[nodiscard]] math::v3 scale() const;

	private:
		transform_id id_;
	};
}