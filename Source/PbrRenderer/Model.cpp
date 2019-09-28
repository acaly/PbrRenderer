#include "Model.h"
#include "RenderingSystem.h"

void PbrRenderer::Model::LoadDataRaw(const void* data, int vertexSize, int count)
{
	CD3D11_BUFFER_DESC bufferDesc(vertexSize * count, D3D11_BIND_VERTEX_BUFFER);
	bufferDesc.StructureByteStride = vertexSize;
	this->vertexSize = vertexSize;

	D3D11_SUBRESOURCE_DATA initData = { data, 0, 0 };
	CheckComError(renderingSystem->device->CreateBuffer(&bufferDesc, &initData, vertexBuffer.ReleaseAndGetAddressOf()));

	vertexCount = count;
}

void PbrRenderer::Model::LoadIndexRaw(const std::uint16_t* data, int count)
{
	CD3D11_BUFFER_DESC bufferDesc(sizeof(uint16_t) * count, D3D11_BIND_INDEX_BUFFER);
	bufferDesc.StructureByteStride = sizeof(uint16_t);

	D3D11_SUBRESOURCE_DATA initData = { data, 0, 0 };
	CheckComError(renderingSystem->device->CreateBuffer(&bufferDesc, &initData, indexBuffer.ReleaseAndGetAddressOf()));

	indexBufferFormat = DXGI_FORMAT_R16_UINT;
	indexCount = count;
}

void PbrRenderer::Model::Draw(ID3D11DeviceContext* dc)
{
	UINT offset = 0;
	dc->IASetVertexBuffers(0, 1, vertexBuffer.GetAddressOf(), &vertexSize, &offset);
	if (indexBuffer)
	{
		dc->IASetIndexBuffer(indexBuffer.Get(), indexBufferFormat, 0);
		dc->DrawIndexed(indexCount, 0, 0);
	}
	else
	{
		dc->Draw(vertexCount, 0);
	}
}