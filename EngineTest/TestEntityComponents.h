#pragma once

#include "Test.h"
#include "../Engine/Components/Entity.h"
#include "../Engine/Components/Transform.h"

#include <iostream>
#include <ctime>

using namespace primal;

class engine_test : public test
{
public:
	bool initialize() override
	{
		srand(static_cast<u32>(time(nullptr)));
		return true;
	}

	void run() override
	{
		do
		{
			for (u32 i{0}; i < 10000; ++i)
			{
				create_random();
				remove_random();
				num_entities_ = static_cast<u32>(entities_.size());
			}
			print_results();
		}
		while (getchar() != 'q');
	}

	void shutdown() override
	{
	}

private:
	void create_random()
	{
		u32 count = rand() % 20;
		if (entities_.empty()) count = 1000;
		transform::init_info transform_info{};
		const game_entity::entity_info entity_info
		{
			&transform_info,
		};

		while (count > 0)
		{
			++added_;
			game_entity::entity entity{game_entity::create(entity_info)};
			assert(entity.is_valid() && id::is_valid(entity.get_id()));
			entities_.push_back(entity);
			assert(game_entity::is_alive(entity.get_id()));
			--count;
		}
	}

	void remove_random()
	{
		u32 count = rand() % 20;
		if (entities_.size() < 1000) return;
		while (count > 0)
		{
			const u32 index{static_cast<u32>(rand()) % static_cast<u32>(entities_.size())};
			const game_entity::entity entity{entities_[index]};
			assert(entity.is_valid() && id::is_valid(entity.get_id()));
			if (entity.is_valid())
			{
				game_entity::remove(entity.get_id());
				entities_.erase(entities_.begin() + index);
				assert(!game_entity::is_alive(entity.get_id()));
				++removed_;
			}
			--count;
		}
	}

	void print_results() const
	{
		std::cout << "Entities created: " << added_ << "\n";
		std::cout << "Entities deleted: " << removed_ << "\n";
	}

	utl::vector<game_entity::entity> entities_;

	u32 added_{0};
	u32 removed_{0};
	u32 num_entities_{0};
};
