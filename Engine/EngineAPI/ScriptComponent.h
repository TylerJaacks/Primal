#pragma once
#include "../Components/ComponentsCommon.h"

namespace primal::script
{

	DEFINE_TYPED_ID(script_id);

	class component final
	{
	public:
		constexpr explicit component(const script_id id) : id_{ id } {}
		constexpr component() : id_{ id::invalid_id } {}
		[[nodiscard]] constexpr script_id get_id() const { return id_; }
		[[nodiscard]] constexpr bool is_valid() const { return id::is_valid(id_); }

	private:
		script_id id_;
	};
}