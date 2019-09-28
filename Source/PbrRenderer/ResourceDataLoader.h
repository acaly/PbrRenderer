#pragma once
#include "Common.h"
#include <iostream>

namespace PbrRenderer
{
	struct ResourceDataSegmentInfo
	{
		std::uint32_t Offset;
		std::uint16_t Width;
		std::uint16_t Height;
		std::uint16_t Depth; //Used for cube texture (=6) and texture3d
		std::uint16_t Stride;
		std::uint16_t Slice;
		//For buffer, totalsize=stride*width
	};

	struct ResourceDataHeader
	{
		static const std::uint32_t MagicT0D = 0x30445253;
		static const std::uint32_t MagicT1D = 0x31445253;
		static const std::uint32_t MagicT2D = 0x32445253;
		static const std::uint32_t MagicT3D = 0x33445253;

		std::uint32_t Magic;
		std::uint32_t Format;
		std::uint16_t MipLevel;
		std::uint16_t ArraySize;
	};

	enum class ResourceDataLoadingOption
	{
		ImmutableSRV,
		ImmutableVB,
		ImmutableIB,
	};

	class ResourceDataLoader final
	{
		ResourceDataLoader() {}

	public:
		static void LoadBuffer(ID3D11Device* device, std::istream& stream, ResourceDataLoadingOption option,
			ID3D11Buffer** buffer, ID3D11ShaderResourceView** srv);
		static void LoadTexture2D(ID3D11Device* device, std::istream& stream, ResourceDataLoadingOption option,
			ID3D11Texture2D** buffer, ID3D11ShaderResourceView** srv);
		//TODO LoadTexture1D LoadTextureCube LoadTexture3D
	};
}
