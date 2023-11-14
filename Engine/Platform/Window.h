#pragma once
#include "../Common/CommonHeaders.h"


namespace primal::platform
{
	DEFINE_TYPED_ID(window_id);

	class window
	{
	public:
		constexpr explicit window(const window_id id) : id_{ id } {}
		constexpr window() : id_{ id::invalid_id } {}
		[[nodiscard]] constexpr window_id get_id() const { return id_; }
		[[nodiscard]] constexpr bool is_valid() const { return id::is_valid(id_); }

		void set_fullscreen(bool is_fullscreen) const;
		[[nodiscard]] bool is_fullscreen() const;

		[[nodiscard]] void* handle() const;

		void set_caption(const wchar_t* caption) const;

		[[nodiscard]] math::u32v4 size() const;

		void resize(u32 width, u32 height) const;

		[[nodiscard]] u32 width() const;
		[[nodiscard]] u32 height() const;

		[[nodiscard]] bool is_closed() const;
	private:
		window_id id_ { id::invalid_id };
	};
}
