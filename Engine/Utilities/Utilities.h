#pragma once

#define USE_STL_VECTOR 1
#define USE_STL_DEQUE 1

#ifdef USE_STL_VECTOR
#include <vector>

namespace primal::util
{
	template<typename T>
	using vector = typename std::vector<T>;
}
#endif

#ifdef USE_STL_DEQUE
#include <deque>

namespace primal::util
{
	template<typename T>
	using deque = typename std::deque<T>;
}
#endif

namespace primal::util
{
	// TODO: implement our own containers.
}