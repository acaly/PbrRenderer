#include "TestScene.h"
#include "RenderingSystem.h"
#include "ResourceDataLoader.h"
#include <d3dcompiler.h>
#include <fstream>

using namespace DirectX;

namespace
{
	using namespace PbrRenderer;

	std::unique_ptr<Model> MakeBoxModel(RenderingSystem* rs)
	{
		auto model = std::make_unique<Model>(rs);

		static const SimpleVertex vertices[] =
		{
			{ XMFLOAT3(-1.0f, 1.0f, -1.0f), XMFLOAT3(0.0f, 1.0f, 0.0f), XMFLOAT2(0.0f, 0.0f) },
			{ XMFLOAT3(1.0f, 1.0f, -1.0f), XMFLOAT3(0.0f, 1.0f, 0.0f), XMFLOAT2(1.0f, 0.0f) },
			{ XMFLOAT3(1.0f, 1.0f, 1.0f), XMFLOAT3(0.0f, 1.0f, 0.0f), XMFLOAT2(1.0f, 1.0f) },
			{ XMFLOAT3(-1.0f, 1.0f, 1.0f), XMFLOAT3(0.0f, 1.0f, 0.0f), XMFLOAT2(0.0f, 1.0f) },

			{ XMFLOAT3(-1.0f, -1.0f, -1.0f), XMFLOAT3(0.0f, -1.0f, 0.0f), XMFLOAT2(1.0f, 1.0f) },
			{ XMFLOAT3(1.0f, -1.0f, -1.0f), XMFLOAT3(0.0f, -1.0f, 0.0f), XMFLOAT2(1.0f, 0.0f) },
			{ XMFLOAT3(1.0f, -1.0f, 1.0f), XMFLOAT3(0.0f, -1.0f, 0.0f), XMFLOAT2(0.0f, 0.0f) },
			{ XMFLOAT3(-1.0f, -1.0f, 1.0f), XMFLOAT3(0.0f, -1.0f, 0.0f), XMFLOAT2(0.0f, 1.0f) },

			{ XMFLOAT3(-1.0f, -1.0f, 1.0f), XMFLOAT3(-1.0f, 0.0f, 0.0f), XMFLOAT2(1.0f, 0.0f) },
			{ XMFLOAT3(-1.0f, -1.0f, -1.0f), XMFLOAT3(-1.0f, 0.0f, 0.0f), XMFLOAT2(1.0f, 1.0f) },
			{ XMFLOAT3(-1.0f, 1.0f, -1.0f), XMFLOAT3(-1.0f, 0.0f, 0.0f), XMFLOAT2(0.0f, 1.0f) },
			{ XMFLOAT3(-1.0f, 1.0f, 1.0f), XMFLOAT3(-1.0f, 0.0f, 0.0f), XMFLOAT2(0.0f, 0.0f) },

			{ XMFLOAT3(1.0f, -1.0f, 1.0f), XMFLOAT3(1.0f, 0.0f, 0.0f), XMFLOAT2(1.0f, 0.0f) },
			{ XMFLOAT3(1.0f, -1.0f, -1.0f), XMFLOAT3(1.0f, 0.0f, 0.0f), XMFLOAT2(0.0f, 0.0f) },
			{ XMFLOAT3(1.0f, 1.0f, -1.0f), XMFLOAT3(1.0f, 0.0f, 0.0f), XMFLOAT2(0.0f, 1.0f) },
			{ XMFLOAT3(1.0f, 1.0f, 1.0f), XMFLOAT3(1.0f, 0.0f, 0.0f), XMFLOAT2(1.0f, 1.0f) },

			{ XMFLOAT3(-1.0f, -1.0f, -1.0f), XMFLOAT3(0.0f, 0.0f, -1.0f), XMFLOAT2(1.0f, 1.0f) },
			{ XMFLOAT3(1.0f, -1.0f, -1.0f), XMFLOAT3(0.0f, 0.0f, -1.0f), XMFLOAT2(0.0f, 1.0f) },
			{ XMFLOAT3(1.0f, 1.0f, -1.0f), XMFLOAT3(0.0f, 0.0f, -1.0f), XMFLOAT2(0.0f, 0.0f) },
			{ XMFLOAT3(-1.0f, 1.0f, -1.0f), XMFLOAT3(0.0f, 0.0f, -1.0f), XMFLOAT2(1.0f, 0.0f) },

			{ XMFLOAT3(-1.0f, -1.0f, 1.0f), XMFLOAT3(0.0f, 0.0f, 1.0f), XMFLOAT2(0.0f, 0.0f) },
			{ XMFLOAT3(1.0f, -1.0f, 1.0f), XMFLOAT3(0.0f, 0.0f, 1.0f), XMFLOAT2(0.0f, 1.0f) },
			{ XMFLOAT3(1.0f, 1.0f, 1.0f), XMFLOAT3(0.0f, 0.0f, 1.0f), XMFLOAT2(1.0f, 1.0f) },
			{ XMFLOAT3(-1.0f, 1.0f, 1.0f), XMFLOAT3(0.0f, 0.0f, 1.0f), XMFLOAT2(1.0f, 0.0f) },
		};
		model->LoadData(vertices);

		static const uint16_t indices[] =
		{
			3, 1, 0, 2, 1, 3,
			6, 4, 5, 7, 4, 6,
			11, 9, 8, 10, 9, 11,
			14, 12, 13, 15, 12, 14,
			19, 17, 16, 18, 17, 19,
			22, 20, 21, 23, 20, 22,
		};
		model->LoadIndex(indices);

		return model;
	}

	std::unique_ptr<Model> MakeGroundModel(RenderingSystem* rs)
	{
		auto model = std::make_unique<Model>(rs);

		static const SimpleVertex vertices[] =
		{
			{ XMFLOAT3(-10.0f, -10.0f, -1.2f), XMFLOAT3(0.0f, 0.0f, 1.0f), XMFLOAT2(0.0f, 0.0f) },
			{ XMFLOAT3(10.0f, -10.0f, -1.2f), XMFLOAT3(0.0f, 0.0f, 1.0f), XMFLOAT2(0.0f, 1.0f) },
			{ XMFLOAT3(10.0f, 10.0f, -1.2f), XMFLOAT3(0.0f, 0.0f, 1.0f), XMFLOAT2(1.0f, 1.0f) },
			{ XMFLOAT3(-10.0f, 10.0f, -1.2f), XMFLOAT3(0.0f, 0.0f, 1.0f), XMFLOAT2(1.0f, 0.0f) },
		};
		model->LoadData(vertices);

		static const uint16_t indices[] =
		{
			2, 0, 1, 3, 0, 2,
		};
		model->LoadIndex(indices);

		return model;
	}
}

void PbrRenderer::TestScene::Initialize()
{
	pipeline = std::make_unique<RenderingPipeline>(renderingSystem);
	pipeline->SetTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
	pipeline->SetDefaultViewport();

	const D3D11_INPUT_ELEMENT_DESC inputElementDesc[] =
	{
		{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		{ "NORMAL", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		{ "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 24, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	};
	pipeline->SetInputLayout(inputElementDesc);

	pipeline->SetShaderFromFile(ShaderType::Vertex, PRECOMPILED_SHADER_PATH("TestScene_TestVS.cso"));
	pipeline->SetShaderFromFile(ShaderType::Pixel, PRECOMPILED_SHADER_PATH("TestScene_TestPS.cso"));
	pipeline->SetRasterizer({ D3D11_FILL_SOLID, D3D11_CULL_FRONT });

	CD3D11_BUFFER_DESC bufferDesc(sizeof(ConstantBuffer), D3D11_BIND_CONSTANT_BUFFER, D3D11_USAGE_DYNAMIC, D3D11_CPU_ACCESS_WRITE);
	CheckComError(renderingSystem->device->CreateBuffer(&bufferDesc, nullptr, constantBuffer.ReleaseAndGetAddressOf()));
	XMStoreFloat4x4(&worldMatrix, XMMatrixIdentity());
	pipeline->SetConstantBuffer(ShaderType::Vertex, 0, constantBuffer.Get());

	modelBox = MakeBoxModel(renderingSystem);
	modelGround = MakeGroundModel(renderingSystem);

	defaultRenderTarget.SetDefaultTargets(renderingSystem);

	std::ifstream texture_file(TEST_SCENE_PATH("Compiled/473-free-hdri-skies-com.srd"), std::ios::in | std::ios::binary);
	ComPtr<ID3D11Texture2D> unused_ptr;
	ResourceDataLoader::LoadTexture2D(renderingSystem->device.Get(), texture_file, ResourceDataLoadingOption::ImmutableSRV,
		unused_ptr.GetAddressOf(), testTexture.GetAddressOf());
}

void PbrRenderer::TestScene::Render()
{
	camera.Step(0.016f);

	auto& context = renderingSystem->immediateContext;

	defaultRenderTarget.ClearAll(context.Get());
	defaultRenderTarget.Apply(context.Get());

	pipeline->Attach(context.Get());

	context->PSSetShaderResources(0, 1, testTexture.GetAddressOf());

	ConstantBuffer sceneParameters =
	{
		XMLoadFloat4x4(&worldMatrix),
		XMMatrixTranspose(XMMatrixMultiply(camera.GetViewMatrix(), camera.GetProjectionMatrix())),
	};

	D3D11_MAPPED_SUBRESOURCE mapped;
	CheckComError(context->Map(constantBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped));
	memcpy(mapped.pData, &sceneParameters, sizeof(ConstantBuffer));
	context->Unmap(constantBuffer.Get(), 0);

	modelGround->Draw(context.Get());
	modelBox->Draw(context.Get());
}

void PbrRenderer::TestScene::OnEventBefore(HWND hwnd, UINT umessage, WPARAM wparam, LPARAM lparam)
{
	camera.HandleWindowEvent(hwnd, umessage, wparam, lparam);
}