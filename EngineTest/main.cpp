#define TEST_ENTITY_COMPONENTS 1

#pragma comment(lib, "Engine.lib")

#if TEST_ENTITY_COMPONENTS
#include "TestEntityComponent.h"
#else
#error On of the tests needs to be enabled.
#endif

#include "../Engine/Common/CommonHeaders.h"
#include "../Engine/Components/Entity.h"
#include "../Engine/Components/Transform.h"

using namespace primal;
using namespace primal::game_entity;

int main()
{
#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif

	test_entity_component test{ };

	if (test.initialize())
	{
		bool result = test.run();
	}

	return test.shutdown();
}
