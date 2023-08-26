#pragma once

#pragma warning(disable: 4530) // disable exception warning

#include <stdint.h>
#include <assert.h>
#include <typeinfo>
#include <memory>

#if defined(_WIN64)
#include <DirectXMath.h>
#include <windows.h>
#endif

#include "../Utilities/Utilities.h"
#include "../Utilities/MathTypes.h"
#include "PrimitiveTypes.h"