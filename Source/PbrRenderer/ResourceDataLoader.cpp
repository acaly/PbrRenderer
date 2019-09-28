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
	//CD3D11_TEXTURE2D_DESC desc((DXGI_FORMAT)header->Format, segments[0].Width, segments[0].Height, header->ArraySize, header->MipLevel);
	CD3D11_BUFFER_DESC desc((UINT)(segments->Width * segments->Stride), 0, D3D11_USAGE_IMMUTABLE, 0, 0); //immutable
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
			srvDesc = CD3D11_SHADER_RESOURCE_VIEW_DESC(D3D_SRV_DIMENSION_BUFFEREX, DXGI_FORMAT_UNKNOWN, 0, segments->Width);
		}
		else
		{
			srvDesc = CD3D11_SHADER_RESOURCE_VIEW_DESC(D3D_SRV_DIMENSION_BUFFER, (DXGI_FORMAT)header->Format, 0, segments->Width);
		}
		break;
	case ResourceDataLoadingOption::ImmutableVB:
		desc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
		break;
	case ResourceDataLoadingOption::ImmutableIB:
		desc.BindFlags = D3D11_BIND_INDEX_BUFFER;
		break;
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
	CD3D11_TEXTURE2D_DESC desc((DXGI_FORMAT)header->Format, segments[0].Width, segments[0].Height, header->ArraySize, header->MipLevel);
	switch (option)
	{
	case ResourceDataLoadingOption::ImmutableSRV:
		break;
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
