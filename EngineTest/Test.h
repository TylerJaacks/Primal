#pragma once
#include <thread>

class test
{
public:
	virtual ~test() = default;
	virtual bool initialize() = 0;
	virtual void run() = 0;
	virtual void shutdown() = 0;
};
