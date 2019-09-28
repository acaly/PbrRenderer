#pragma once
#include "Common.h"

namespace PbrRenderer
{
	struct SimpleVertex
	{
		DirectX::XMFLOAT3 Position;
		DirectX::XMFLOAT3 Normal;
		DirectX::XMFLOAT2 TexCoord;
	};

	class RenderingSystem;

	class Model final
	{
	public:
		Model(RenderingSystem* rs) : renderingSystem(rs) {}
		~Model() {}

	public:
		template <typename T, int N>
		void LoadData(const T(&data)[N])
		{
			LoadDataRaw(data, sizeof(T), N);
		}

		void LoadDataRaw(const void* data, int vertexSize, int count);

		void SetData(ComPtr<ID3D11Buffer>&& data, int stride)
		{
			D3D11_BUFFER_DESC desc;
			data->GetDesc(&desc);

			vertexBuffer.Reset();
			vertexBuffer.Swap(data);
			this->vertexSize = stride;
			this->vertexCount = desc.ByteWidth / stride;
		}

		template <int N>
		void LoadIndex(const std::uint16_t(&data)[N])
		{
			LoadIndexRaw(data, N);
		}

		void LoadIndexRaw(const std::uint16_t* data, int count);

		void SetIndex(ComPtr<ID3D11Buffer>&& data, int stride)
		{
			D3D11_BUFFER_DESC desc;
			data->GetDesc(&desc);

			indexBuffer.Reset();
			indexBuffer.Swap(data);
			indexBufferFormat = stride == 2 ? DXGI_FORMAT_R16_UINT : DXGI_FORMAT_R32_UINT;
			indexCount = desc.ByteWidth	 / stride;
		}

		void Draw(ID3D11DeviceContext* dc);

	private:
		RenderingSystem* const renderingSystem;
		ComPtr<ID3D11Buffer> vertexBuffer;
		ComPtr<ID3D11Buffer> indexBuffer;
		UINT vertexSize = 0;
		UINT vertexCount = 0;
		DXGI_FORMAT indexBufferFormat = DXGI_FORMAT_UNKNOWN;
		UINT indexCount = 0;
	};
}
