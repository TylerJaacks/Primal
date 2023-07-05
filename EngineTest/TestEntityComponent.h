#pragma once
#include "Test.h"

#include "../Engine/Common/CommonHeaders.h"
#include "../Engine/Components/Entity.h"
#include "../Engine/Components/Transform.h"

#include <iostream>
#include <ctime>

using namespace primal;

class test_entity_component final : public test
{
public:
	bool initialize() override
	{
		srand ((u32) time(nullptr));

		return true;
	}

	bool run() override
	{
		u32 i = 0;

		do
		{
			for (u32 i { 0 }; i < 10; ++i)
			{
				create_random();
				remove_random();

				num_entries_ = (u32) entities_.size();
			}

			i++;

			print_results();
		} while (i == 1);

		return true;
	}

	bool shutdown() override { return true; }

private:
	void create_random()
	{
		u32 count = rand() % 20;

		if (!entities_.empty()) count = 1000;

		transform::init_info transform_info { };

		game_entity::entity_info entity_info 
		{ 
			&transform_info 
		};

		while (count > 0)
		{
			++added_;

			game_entity::entity entity { game_entity::create_game_entity(entity_info) };

			printf("Entity: %d\n", entity.get_id());

			if (entity.get_id() >= 4294967295)
			{
				printf("ERROR!\n");
			}

			// assert(entity.is_valid() && id::is_valid(entity.get_id()));

			entities_.push_back(entity);

			//assert(game_entity::is_alive(entity));

			--count;
		}
	}

	void remove_random()
	{
		u32 count = rand() % 20;

		if (!entities_.empty()) count = 1000;

		while (count > 0)
		{
			++remove_;

			const u32 index{ static_cast<u32>(rand()) % static_cast<u32>(entities_.size()) };

			const game_entity::entity entity { entities_[index] };

			//assert(entity.is_valid() && id::is_valid(entity.get_id()));

			if (entity.is_valid())
			{
				game_entity::remove_game_entity(entity);

				entities_.erase(entities_.begin() + index);

				//assert(!game_entity::is_alive(entity));
			}

			--count;
		}
	}

	void print_results()
	{
		std::cout << "Entities added: " << added_ << "\n";
		std::cout << "Entities deleted: " << remove_ << "\n";
	}

	util::vector<game_entity::entity> entities_{ };

	u32 added_{ 0 };
	u32 remove_{ 0 };
	u32 num_entries_{ 0 };
};
