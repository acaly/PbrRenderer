#include "RenderingPipeline.h"
#include "RenderingSystem.h"
#include <d3dcompiler.h>

void PbrRenderer::RenderingPipeline::SetDefaultViewport()
{
	D3D11_VIEWPORT vp[1] = { { 0.0f, 0.0f, float(renderingSystem->window_width), float(renderingSystem->window_height), 0, 1 } };
	SetViewport(vp);
}

void PbrRenderer::RenderingPipeline::SetShaderFromFile(ShaderType shader, LPCWSTR filename)
{
	ComPtr<ID3DBlob> buffer;
	CheckComError(D3DReadFileToBlob(filename, buffer.GetAddressOf()));

	switch (shader)
	{
	case ShaderType::Vertex:
		CheckComError(renderingSystem->device->CreateVertexShader(buffer->GetBufferPointer(), buffer->GetBufferSize(), 0,
			vertexShader.ReleaseAndGetAddressOf()));
		CheckComError(renderingSystem->device->CreateInputLayout(inputLayoutDesc.data(), inputLayoutDesc.size(),
			buffer->GetBufferPointer(), buffer->GetBufferSize(), inputLayout.ReleaseAndGetAddressOf()));
		break;
	case ShaderType::Geometry:
		CheckComError(renderingSystem->device->CreateGeometryShader(buffer->GetBufferPointer(), buffer->GetBufferSize(), 0,
			geometryShader.ReleaseAndGetAddressOf()));
		break;
	case ShaderType::Pixel:
		CheckComError(renderingSystem->device->CreatePixelShader(buffer->GetBufferPointer(), buffer->GetBufferSize(), 0,
			pixelShader.ReleaseAndGetAddressOf()));
		break;
	}
}

void PbrRenderer::RenderingPipeline::SetConstantBuffer(ShaderType shader, int i, ID3D11Buffer* buffer)
{
	constantBuffers.push_back({ shader, i, buffer });
}

void PbrRenderer::RenderingPipeline::SetRasterizer(const D3D11_RASTERIZER_DESC& desc)
{
	CheckComError(renderingSystem->device->CreateRasterizerState(&desc, rasterizer.ReleaseAndGetAddressOf()));
}

void PbrRenderer::RenderingPipeline::SetViewportInternal(const D3D11_VIEWPORT* vp, int count)
{
	viewports.clear();
	viewports.insert(viewports.end(), vp, vp + count);
}

void PbrRenderer::RenderingPipeline::SetInputLayoutInternal(const D3D11_INPUT_ELEMENT_DESC* desc, int count)
{
	inputLayoutDesc.clear();
	inputLayoutDesc.insert(inputLayoutDesc.end(), desc, desc + count);
}

void PbrRenderer::RenderingPipeline::Attach(ID3D11DeviceContext* dc)
{
	dc->RSSetState(rasterizer.Get());
	dc->RSSetViewports(viewports.size(), viewports.data());

	dc->IASetInputLayout(inputLayout.Get());
	dc->IASetPrimitiveTopology(topology);

	dc->VSSetShader(vertexShader.Get(), 0, 0);
	dc->GSSetShader(geometryShader.Get(), 0, 0);
	dc->PSSetShader(pixelShader.Get(), 0, 0);

	for (auto& k : constantBuffers)
	{
		switch (k.Shader)
		{
		case ShaderType::Vertex:
			dc->VSSetConstantBuffers(k.Index, 1, k.Buffer.GetAddressOf());
			break;
		case ShaderType::Geometry:
			dc->GSSetConstantBuffers(k.Index, 1, k.Buffer.GetAddressOf());
			break;
		case ShaderType::Pixel:
			dc->PSSetConstantBuffers(k.Index, 1, k.Buffer.GetAddressOf());
			break;
		}
	}
}

void PbrRenderer::RenderingPipeline::Detach(ID3D11DeviceContext* dc)
{
	ID3D11Buffer* buffer = nullptr;
	for (auto& k : constantBuffers)
	{
		switch (k.Shader)
		{
		case ShaderType::Vertex:
			dc->VSSetConstantBuffers(k.Index, 1, &buffer);
			break;
		case ShaderType::Geometry:
			dc->GSSetConstantBuffers(k.Index, 1, &buffer);
			break;
		case ShaderType::Pixel:
			dc->PSSetConstantBuffers(k.Index, 1, &buffer);
			break;
		}
	}
}

