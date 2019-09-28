#pragma once

#define _CRT_SECURE_NO_WARNINGS 1
#define NOMINMAX

#include <exception>
#include <cstdio>
#include <cstdint>
#include <memory>

#include <d3d11.h>
#include <wrl/client.h>
#include <DirectXMath.h>

//TODO better way of finding this (probably through debugging argument, or through project C++ preprocessor macro)
#define PRECOMPILED_SHADER_PATH(x) (L"" x)
#define TEST_SCENE_PATH(x) (L"../../Example/TestScene/" x)

namespace PbrRenderer
{
	template <typename T>
	using ComPtr = Microsoft::WRL::ComPtr<T>;

	class com_exception : public std::exception
	{
	public:
		com_exception(HRESULT hr) : result(hr) {}

		virtual const char* what() const override
		{
			static char s_str[64] = {};
			sprintf(s_str, "Failure with HRESULT of %08X", static_cast<unsigned int>(result));
			return s_str;
		}

	private:
		HRESULT result;
	};

	inline void ThrowComError(HRESULT hr)
	{
		throw com_exception(hr);
	}

	inline void CheckComError(HRESULT hr)
	{
		if (!SUCCEEDED(hr))
		{
			ThrowComError(hr);
		}
	}
}
