#pragma once
#include "Common.h"
#include <vector>

namespace PbrRenderer
{
	enum class ShaderType
	{
		Vertex,
		Geometry,
		Pixel,
	};

	class RenderingSystem;

	class RenderingPipeline final
	{
	public:
		RenderingPipeline(RenderingSystem* rs) : renderingSystem(rs) {}
		~RenderingPipeline() {}

	public:
		void SetTopology(D3D11_PRIMITIVE_TOPOLOGY t)
		{
			topology = t;
		}

		void SetDefaultViewport();

		template <int N> void SetViewport(const D3D11_VIEWPORT(&vp)[N])
		{
			SetViewportInternal(vp, N);
		}

		template <int N> void SetInputLayout(const D3D11_INPUT_ELEMENT_DESC(&e)[N])
		{
			SetInputLayoutInternal(e, N);
		}

		void SetShaderFromFile(ShaderType shader, LPCWSTR filename);
		void SetConstantBuffer(ShaderType shader, int i, ID3D11Buffer* buffer);

	public:
		void SetRasterizer(const D3D11_RASTERIZER_DESC& desc);

	private:
		void SetViewportInternal(const D3D11_VIEWPORT* vp, int count);
		void SetInputLayoutInternal(const D3D11_INPUT_ELEMENT_DESC* desc, int count);

	public:
		void Attach(ID3D11DeviceContext* dc);
		void Detach(ID3D11DeviceContext* dc);

	private:
		RenderingSystem* const renderingSystem;
		D3D11_PRIMITIVE_TOPOLOGY topology = D3D_PRIMITIVE_TOPOLOGY_UNDEFINED;
		std::vector<D3D11_INPUT_ELEMENT_DESC> inputLayoutDesc;
		std::vector<D3D11_VIEWPORT> viewports;
		ComPtr<ID3D11VertexShader> vertexShader;
		ComPtr<ID3D11GeometryShader> geometryShader;
		ComPtr<ID3D11PixelShader> pixelShader;
		ComPtr<ID3D11InputLayout> inputLayout;

		ComPtr<ID3D11RasterizerState> rasterizer;

		struct ShaderConstantBufferInfo
		{
			ShaderType Shader;
			int Index;
			ComPtr<ID3D11Buffer> Buffer;
		};

		std::vector<ShaderConstantBufferInfo> constantBuffers;
	};
}
