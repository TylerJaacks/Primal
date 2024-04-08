#pragma once
#include "D3D12CommonHeaders.h"

namespace primal::graphics::d3d12::core {
	bool initialize();
	void render();
	void shutdown();

	template<typename T> constexpr void release(T*& resource)
	{
		if (resource)
		{
			resource->Release();

			resource = nullptr;
		}
	}

	ID3D12Device* const device();
}