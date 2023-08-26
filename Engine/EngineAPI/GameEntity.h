#pragma once

#include <string>

#include "TransfromComponent.h"
#include "ScriptComponent.h"

namespace primal
{
	namespace game_entity
	{

		DEFINE_TYPED_ID(entity_id);

		class entity
		{
		public:
			constexpr explicit entity(const entity_id id) : id_{ id } {}
			constexpr entity() : id_{ id::invalid_id } {}
			[[nodiscard]] constexpr entity_id get_id() const { return id_; }
			[[nodiscard]] constexpr bool is_valid() const { return id::is_valid(id_); }

			[[nodiscard]] transform::component transform() const;
			[[nodiscard]] script::component script() const;
		private:
			entity_id id_;
		};
	}

	namespace script {
		class entity_script : public game_entity::entity
		{
		public:
			virtual ~entity_script() = default;
			virtual void begin_play() {}
			virtual void update(float) {}
		protected:
			constexpr explicit entity_script(const game_entity::entity entity)
				: game_entity::entity{ entity.get_id() } {}
		};

		namespace detail {
			using script_ptr = std::unique_ptr<entity_script>;
			using script_creator = script_ptr(*)(game_entity::entity entity);
			using string_hash = std::hash<std::string>;

			u8 register_script(size_t, script_creator);

#ifdef USE_WITH_EDITOR
			extern "C" __declspec(dllexport)
#endif
			script_creator get_script_creator(size_t tag);

			template<class script_class> script_ptr create_script(game_entity::entity entity)
			{
				assert(entity.is_valid());

				return std::make_unique<script_class>(entity);
			}

#ifdef USE_WITH_EDITOR

			u8 add_script_name(const char* name);

#define REGISTER_SCRIPT(TYPE)														\
			namespace {																\
				const u8 _reg##TYPE													\
				{																	\
					primal::script::detail::register_script(						\
						primal::script::detail::string_hash()(#TYPE),				\
						&primal::script::detail::create_script<TYPE>)				\
				};																	\
																					\
				const u8 _name_##TYPE												\
				{																	\
					primal::script::detail::add_script_name(#TYPE)					\
				}																	\
			}																		
#else
#define REGISTER_SCRIPT(TYPE)														\
			namespace {																\
				const u8 _reg##TYPE													\
				{																	\
					primal::script::detail::register_script(						\
						primal::script::detail::string_hash()(#TYPE),				\
						&primal::script::detail::create_script<TYPE>)				\
				};																	\
			}																		
#endif
		}
	}

}