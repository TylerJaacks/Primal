#pragma once

#pragma warning(disable: 4530) // disable exception warning

#include <stdint.h>
#include <assert.h>
#include <typeinfo>
#include <string>
#include <memory>

#if defined(_WIN64)
#include <DirectXMath.h>
#include <windows.h>
#endif

#include "../Utilities/Utilities.h"
#include "../Utilities/MathTypes.h"
#include "PrimitiveTypes.h"
#include "../Common/Id.h"

#ifdef _DEBUG
#define DEBUG_OP(x) x
#else
#define DEBUG_OP(x) (void(0))
#endif