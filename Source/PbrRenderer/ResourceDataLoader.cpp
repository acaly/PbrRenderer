#include "ResourceDataLoader.h"

namespace
{
	auto read_all_bytes(std::istream& stream)
	{
		stream.ignore(std::numeric_limits<std::streamsize>::max());
		auto len = (std::size_t)stream.gcount();
		stream.clear();   //  Since ignore will have set eof.
		stream.seekg(0, std::ios_base::beg);

		auto ret = std::make_unique<byte[]>(len);
		stream.seekg(0);
		stream.read((char*)(ret.get()), len);
		return ret;
	}
}

void PbrRenderer::ResourceDataLoader::LoadBuffer(ID3D11Device* device, std::istream& stream, ResourceDataLoadingOption option,
	ID3D11Buffer** buffer, ID3D11ShaderResourceView** srv)
{
	*buffer = 0;
	*srv = 0;
	auto data = read_all_bytes(stream);
	auto header = (ResourceDataHeader*)data.get();
	auto segments = (ResourceDataSegmentInfo*)(data.get() + 12);
	if (header->Magic != ResourceDataHeader::MagicT0D)
	{
		throw std::exception("invalid Texture2D file");
	}
	if (header->ArraySize != 1 || header->MipLevel != 1)
	{
		throw std::exception("invalid Buffer file");
	}
	UINT count = segments->Width | segments->Height << 16;
	CD3D11_BUFFER_DESC desc((UINT)(count * segments->Stride), 0, D3D11_USAGE_IMMUTABLE, 0, 0); //immutable
	desc.StructureByteStride = segments->Stride;
	CD3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
	switch (option)
	{
	case ResourceDataLoadingOption::ImmutableSRV:
		desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
		if (header->Format == 0)
		{
			//structured buffer
			desc.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
			srvDesc = CD3D11_SHADER_RESOURCE_VIEW_DESC(D3D_SRV_DIMENSION_BUFFEREX, DXGI_FORMAT_UNKNOWN, 0, count);
		}
		else
		{
			srvDesc = CD3D11_SHADER_RESOURCE_VIEW_DESC(D3D_SRV_DIMENSION_BUFFER, (DXGI_FORMAT)header->Format, 0, count);
		}
		break;
	case ResourceDataLoadingOption::ImmutableVB:
		desc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
		break;
	case ResourceDataLoadingOption::ImmutableIB:
		desc.BindFlags = D3D11_BIND_INDEX_BUFFER;
		break;
	default:
		throw std::exception("invalid resource loading option");
	}
	D3D11_SUBRESOURCE_DATA data_desc = { data.get() + segments->Offset, 0, 0 };
	ComPtr<ID3D11Buffer> texPtr;
	CheckComError(device->CreateBuffer(&desc, &data_desc, texPtr.GetAddressOf()));
	ComPtr<ID3D11ShaderResourceView> srvPtr;
	if (option == ResourceDataLoadingOption::ImmutableSRV)
	{
		CheckComError(device->CreateShaderResourceView(texPtr.Get(), &srvDesc, srvPtr.GetAddressOf()));
	}
	*buffer = texPtr.Detach();
	*srv = srvPtr.Detach();
}

void PbrRenderer::ResourceDataLoader::LoadTexture2D(ID3D11Device* device, std::istream& stream, ResourceDataLoadingOption option,
	ID3D11Texture2D** buffer, ID3D11ShaderResourceView** srv)
{
	*buffer = 0;
	*srv = 0;
	auto data = read_all_bytes(stream);
	auto header = (ResourceDataHeader*)data.get();
	auto segments = (ResourceDataSegmentInfo*)(data.get() + 12);
	if (header->Magic != ResourceDataHeader::MagicT2D)
	{
		throw std::exception("invalid Texture2D file");
	}
	if (header->MipLevel != 1 || header->ArraySize != 1)
	{
		throw "not implemented";
	}
	CD3D11_TEXTURE2D_DESC desc((DXGI_FORMAT)header->Format, segments[0].Width, segments[0].Height, header->ArraySize, header->MipLevel);
	switch (option)
	{
	case ResourceDataLoadingOption::ImmutableSRV:
		break;
	default:
		throw std::exception("invalid resource loading option");
	}
	auto subresource_count = header->ArraySize * header->MipLevel;
	auto data_desc = std::make_unique<D3D11_SUBRESOURCE_DATA[]>(subresource_count);
	for (int i = 0; i < subresource_count; ++i)
	{
		data_desc[i].pSysMem = data.get() + segments[i].Offset;
		data_desc[i].SysMemPitch = segments[i].Stride;
	}
	ComPtr<ID3D11Texture2D> texPtr;
	CheckComError(device->CreateTexture2D(&desc, data_desc.get(), texPtr.GetAddressOf()));
	ComPtr<ID3D11ShaderResourceView> srvPtr;
	CheckComError(device->CreateShaderResourceView(texPtr.Get(), nullptr, srvPtr.GetAddressOf()));
	*buffer = texPtr.Detach();
	*srv = srvPtr.Detach();
}

void PbrRenderer::ResourceDataLoader::LoadTextureCube(ID3D11Device* device, std::istream& stream, ResourceDataLoadingOption option,
	ID3D11Texture2D** buffer, ID3D11ShaderResourceView** srv)
{
	//TODO check the order of array elements, mipmaps, and cube map faces
	//Here we assume single element, single mipmap
	*buffer = 0;
	*srv = 0;
	auto data = read_all_bytes(stream);
	auto header = (ResourceDataHeader*)data.get();
	auto segments = (ResourceDataSegmentInfo*)(data.get() + 12);
	if (header->Magic != ResourceDataHeader::MagicT2D)
	{
		throw std::exception("invalid TextureCube file");
	}
	if (header->MipLevel != 1 || header->ArraySize != 1)
	{
		throw "not implemented";
	}
	CD3D11_TEXTURE2D_DESC desc((DXGI_FORMAT)header->Format, segments[0].Width, segments[0].Height, 6 * header->ArraySize, header->MipLevel);
	desc.MiscFlags |= D3D11_RESOURCE_MISC_TEXTURECUBE;
	CD3D11_SHADER_RESOURCE_VIEW_DESC srvDesc(D3D11_SRV_DIMENSION_TEXTURECUBE, (DXGI_FORMAT)header->Format); //TODO we need more arguments for array and mipmap
	switch (option)
	{
	case ResourceDataLoadingOption::ImmutableSRV:
		break;
	default:
		throw std::exception("invalid resource loading option");
	}
	auto subresource_count = header->ArraySize * header->MipLevel * 6;
	auto data_desc = std::make_unique<D3D11_SUBRESOURCE_DATA[]>(subresource_count);
	for (int i = 0; i < subresource_count / 6; ++i)
	{
		for (int j = 0; j < 6; ++j)
		{
			data_desc[i * 6 + j].pSysMem = data.get() + segments[i].Offset + segments[i].Slice * j;
			data_desc[i * 6 + j].SysMemPitch = segments[i].Stride;
		}
	}
	ComPtr<ID3D11Texture2D> texPtr;
	CheckComError(device->CreateTexture2D(&desc, data_desc.get(), texPtr.GetAddressOf()));
	ComPtr<ID3D11ShaderResourceView> srvPtr;
	CheckComError(device->CreateShaderResourceView(texPtr.Get(), &srvDesc, srvPtr.GetAddressOf()));
	*buffer = texPtr.Detach();
	*srv = srvPtr.Detach();
}
