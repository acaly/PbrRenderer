#pragma once
#include "Common.h"
#include "RenderingSystem.h"
#include "RenderTargetGroup.h"
#include "RenderingPipeline.h"
#include "Model.h"
#include "Camera.h"

namespace PbrRenderer
{
	class TestScene final : public IWindowsEventHandler
	{
	public:
		TestScene(RenderingSystem* rs) : renderingSystem(rs), camera({}) {}
		~TestScene() {}

	public:
		void Initialize();
		void Render();

		virtual void OnEventBefore(HWND hwnd, UINT umessage, WPARAM wparam, LPARAM lparam) override;

	public:
		struct ConstantBuffer
		{
			DirectX::XMMATRIX worldMatrix;
			DirectX::XMMATRIX viewProjMatrix;
			DirectX::XMVECTOR viewPos;
		};

	private:
		RenderingSystem* const renderingSystem;
		RenderTargetGroup defaultRenderTarget;
		Camera camera;

		std::unique_ptr<RenderingPipeline> pipeline;
		std::unique_ptr<Model> modelBox;
		std::unique_ptr<Model> modelGround;

		ComPtr<ID3D11Buffer> constantBuffer;
		DirectX::XMFLOAT4X4 worldMatrix;

		ComPtr<ID3D11ShaderResourceView> testTexture;

		std::unique_ptr<Model> modelSphere;

		ComPtr<ID3D11ShaderResourceView> diffuseCubeTexture;
		ComPtr<ID3D11ShaderResourceView> specularCubeTexture;

		ComPtr<ID3D11SamplerState> sampler;
	};
}
