#include "TestScene.h"
#include "RenderingSystem.h"
#include "ResourceDataLoader.h"
#include <d3dcompiler.h>
#include <fstream>
#include <DirectXColors.h>

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

	void CreateR32G32B32A32RTV(RenderingSystem* rs, ComPtr<ID3D11Texture2D>& tex, ComPtr<ID3D11RenderTargetView>& rtv)
	{
		CD3D11_TEXTURE2D_DESC desc = CD3D11_TEXTURE2D_DESC {
			DXGI_FORMAT_R32G32B32A32_FLOAT,
			(UINT)rs->window_width, (UINT)rs->window_height,
			1, 0,
			D3D11_BIND_RENDER_TARGET,
		};
		CheckComError(rs->device->CreateTexture2D(&desc, 0, tex.GetAddressOf()));
		CheckComError(rs->device->CreateRenderTargetView(tex.Get(), 0, rtv.GetAddressOf()));
	}
}

void PbrRenderer::TestScene::Initialize()
{
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
		pipeline->SetRasterizer({ D3D11_FILL_SOLID, D3D11_CULL_NONE });
	}

	{
		CD3D11_BUFFER_DESC bufferDesc(sizeof(ConstantBuffer), D3D11_BIND_CONSTANT_BUFFER, D3D11_USAGE_DYNAMIC, D3D11_CPU_ACCESS_WRITE);
		CheckComError(renderingSystem->device->CreateBuffer(&bufferDesc, nullptr, constantBuffer.ReleaseAndGetAddressOf()));
		XMStoreFloat4x4(&worldMatrix, XMMatrixIdentity());
		pipeline->SetConstantBuffer(ShaderType::Vertex, 0, constantBuffer.Get());
	}

	modelBox = MakeBoxModel(renderingSystem);
	modelGround = MakeGroundModel(renderingSystem);

	defaultRenderTarget.SetDefaultTargets(renderingSystem);

	{
		std::ifstream texture_file(TEST_SCENE_PATH("Compiled/473-free-hdri-skies-com.srd"), std::ios::in | std::ios::binary);
		ComPtr<ID3D11Texture2D> unused_ptr;
		ResourceDataLoader::LoadTexture2D(renderingSystem->device.Get(), texture_file, ResourceDataLoadingOption::ImmutableSRV,
			unused_ptr.GetAddressOf(), testTexture.GetAddressOf());
	}

	modelSphere = Model::LoadFromFile(renderingSystem, TEST_SCENE_PATH("Compiled/sphere.vb"), TEST_SCENE_PATH("Compiled/sphere.ib"));
	modelBunny = Model::LoadFromFile(renderingSystem, TEST_SCENE_PATH("Compiled/bunny.vb"), TEST_SCENE_PATH("Compiled/bunny.ib"));

	{
		std::ifstream cube_texture_file(TEST_SCENE_PATH("Compiled/sphere_cube_diffuse.srd"), std::ios::in | std::ios::binary);
		ComPtr<ID3D11Texture2D> unused_buffer;
		ResourceDataLoader::LoadTextureCube(renderingSystem->device.Get(), cube_texture_file, ResourceDataLoadingOption::ImmutableSRV,
			unused_buffer.GetAddressOf(), diffuseCubeTexture.GetAddressOf());
	}
	{
		std::ifstream cube_texture_file(TEST_SCENE_PATH("Compiled/sphere_cube_specular.srd"), std::ios::in | std::ios::binary);
		ComPtr<ID3D11Texture2D> unused_buffer;
		ResourceDataLoader::LoadTextureCube(renderingSystem->device.Get(), cube_texture_file, ResourceDataLoadingOption::ImmutableSRV,
			unused_buffer.GetAddressOf(), specularCubeTexture.GetAddressOf());
	}

	{
		D3D11_SAMPLER_DESC desc = {
			D3D11_FILTER_ANISOTROPIC,
			D3D11_TEXTURE_ADDRESS_CLAMP, D3D11_TEXTURE_ADDRESS_CLAMP, D3D11_TEXTURE_ADDRESS_CLAMP,
		};
		CheckComError(renderingSystem->device->CreateSamplerState(&desc, sampler.GetAddressOf()));
	}

	{
		CreateR32G32B32A32RTV(renderingSystem, deferredNormalTexture, deferredNormal);
		CreateR32G32B32A32RTV(renderingSystem, deferredViewDirTexture, deferredViewDir);

		CD3D11_TEXTURE2D_DESC desc = CD3D11_TEXTURE2D_DESC
		{
			DXGI_FORMAT_R32G32B32A32_FLOAT,
			(UINT)renderingSystem->window_width, (UINT)renderingSystem->window_height,
			1, 0,
			0,
			D3D11_USAGE_STAGING,
			D3D11_CPU_ACCESS_READ,
		};
		CheckComError(renderingSystem->device->CreateTexture2D(&desc, 0, deferredStaging.GetAddressOf()));

		normalMapPipeline = std::make_unique<RenderingPipeline>(renderingSystem);
		normalMapPipeline->SetTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
		normalMapPipeline->SetDefaultViewport();

		const D3D11_INPUT_ELEMENT_DESC inputElementDesc[] =
		{
			{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
			{ "NORMAL", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
			{ "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 24, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		};
		normalMapPipeline->SetInputLayout(inputElementDesc);

		normalMapPipeline->SetShaderFromFile(ShaderType::Vertex, PRECOMPILED_SHADER_PATH("TestScene_NormalMapVS.cso"));
		normalMapPipeline->SetShaderFromFile(ShaderType::Pixel, PRECOMPILED_SHADER_PATH("TestScene_NormalMapPS.cso"));
		normalMapPipeline->SetRasterizer({ D3D11_FILL_SOLID, D3D11_CULL_NONE });
		normalMapPipeline->SetConstantBuffer(ShaderType::Vertex, 0, constantBuffer.Get());

		normalMapRenderTarget.SetDepthStencil(renderingSystem->depthStencilView);
		normalMapRenderTarget.AddRenderTarget(deferredNormal, DirectX::Colors::Black);
		normalMapRenderTarget.AddRenderTarget(deferredViewDir, DirectX::Colors::Black);
	}
}

void PbrRenderer::TestScene::Render()
{
	camera.Step(0.016f);

	auto& context = renderingSystem->immediateContext;

	defaultRenderTarget.ClearAll(context.Get());
	defaultRenderTarget.Apply(context.Get());

	pipeline->Attach(context.Get());

	context->PSSetShaderResources(0, 1, testTexture.GetAddressOf());
	context->PSSetShaderResources(1, 1, diffuseCubeTexture.GetAddressOf());
	context->PSSetShaderResources(2, 1, specularCubeTexture.GetAddressOf());
	context->PSSetSamplers(0, 1, sampler.GetAddressOf());

	ConstantBuffer sceneParameters =
	{
		XMLoadFloat4x4(&worldMatrix),
		XMMatrixTranspose(XMMatrixMultiply(camera.GetViewMatrix(), camera.GetProjectionMatrix())),
		camera.GetEyePosition(),
	};
	D3D11_MAPPED_SUBRESOURCE mapped;
	CheckComError(context->Map(constantBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped));
	memcpy(mapped.pData, &sceneParameters, sizeof(ConstantBuffer));
	context->Unmap(constantBuffer.Get(), 0);

	//modelGround->Draw(context.Get());
	//modelBox->Draw(context.Get());
	//
	//sceneParameters.worldMatrix = XMMatrixTranspose(XMMatrixTranslation(0, 1.5f, 0));
	//CheckComError(context->Map(constantBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped));
	//memcpy(mapped.pData, &sceneParameters, sizeof(ConstantBuffer));
	//context->Unmap(constantBuffer.Get(), 0);

	modelBunny->Draw(context.Get());
}

void PbrRenderer::TestScene::OnEventBefore(HWND hwnd, UINT umessage, WPARAM wparam, LPARAM lparam)
{
	camera.HandleWindowEvent(hwnd, umessage, wparam, lparam);
	if (umessage == WM_KEYDOWN)
	{
		switch (wparam)
		{
		case '1':
			camera.target = XMFLOAT3(-0.5f, 0.0f, 1.1f);
			camera.SetYaw(1.6f);
			camera.SetPitch(0.4f);
			camera.SetLength(6.21f);
			break;
		case '2':
			CaptureNormalMap();
			break;
		default:
			break;
		}
	}
}

void PbrRenderer::TestScene::CaptureNormalMap()
{
	auto& context = renderingSystem->immediateContext;

	normalMapRenderTarget.ClearAll(context.Get());
	normalMapRenderTarget.Apply(context.Get());

	normalMapPipeline->Attach(context.Get());

	ConstantBuffer sceneParameters =
	{
		XMLoadFloat4x4(&worldMatrix),
		XMMatrixTranspose(XMMatrixMultiply(camera.GetViewMatrix(), camera.GetProjectionMatrix())),
		camera.GetEyePosition(),
	};

	D3D11_MAPPED_SUBRESOURCE mapped;
	CheckComError(context->Map(constantBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped));
	memcpy(mapped.pData, &sceneParameters, sizeof(ConstantBuffer));
	context->Unmap(constantBuffer.Get(), 0);

	modelBunny->Draw(context.Get());

	context->CopyResource(deferredStaging.Get(), deferredNormalTexture.Get());
	CheckComError(context->Map(deferredStaging.Get(), 0, D3D11_MAP_READ, 0, &mapped));
	{
		std::ofstream output(TEST_SCENE_PATH("Compiled/OutputNormal.raw"), std::ios::binary);
		output.write((char*)mapped.pData, mapped.RowPitch * (UINT)renderingSystem->window_height);
	}
	context->Unmap(deferredStaging.Get(), 0);
	context->CopyResource(deferredStaging.Get(), deferredViewDirTexture.Get());
	CheckComError(context->Map(deferredStaging.Get(), 0, D3D11_MAP_READ, 0, &mapped));
	{
		std::ofstream output(TEST_SCENE_PATH("Compiled/OutputViewDir.raw"), std::ios::binary);
		output.write((char*)mapped.pData, mapped.RowPitch * (UINT)renderingSystem->window_height);
	}
	context->Unmap(deferredStaging.Get(), 0);
}
