#pragma once
#include "Common.h"
#include <vector>

namespace PbrRenderer
{
	class RenderingSystem;

	class RenderTargetGroup final
	{
	public:
		RenderTargetGroup() {}
		~RenderTargetGroup() {}

	public:
		void ResetGroup()
		{
			rtv.clear();
			dsv.Reset();
		}

		void SetDefaultTargets(RenderingSystem* rs);

		void AddRenderTarget(const ComPtr<ID3D11RenderTargetView>& pRTV, DirectX::XMVECTORF32 color)
		{
			rtv.push_back(pRTV);
			colors.push_back(color);
		}

		void SetDepthStencil(const ComPtr<ID3D11DepthStencilView>& pDSV)
		{
			dsv = pDSV;
		}

		void Apply(ID3D11DeviceContext* dc)
		{
			dc->OMSetRenderTargets(rtv.size(), rtv[0].GetAddressOf(), dsv.Get());
		}

		void ClearAll(ID3D11DeviceContext* dc)
		{
			ClearColor(dc);
			ClearDepthStencil(dc);
		}

		void ClearColor(ID3D11DeviceContext* dc)
		{
			for (std::size_t i = 0; i < rtv.size(); ++i)
			{
				dc->ClearRenderTargetView(rtv[i].Get(), colors[i]);
			}
		}

		void ClearDepth(ID3D11DeviceContext* dc)
		{
			if (dsv)
			{
				dc->ClearDepthStencilView(dsv.Get(), D3D11_CLEAR_DEPTH, 1, 0);
			}
		}

		void ClearStencil(ID3D11DeviceContext* dc)
		{
			if (dsv)
			{
				dc->ClearDepthStencilView(dsv.Get(), D3D11_CLEAR_STENCIL, 1, 0);
			}

		}

		void ClearDepthStencil(ID3D11DeviceContext* dc)
		{
			if (dsv)
			{
				dc->ClearDepthStencilView(dsv.Get(), D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL, 1, 0);
			}
		}

	private:
		std::vector<ComPtr<ID3D11RenderTargetView>> rtv;
		std::vector<DirectX::XMVECTORF32> colors;
		ComPtr<ID3D11DepthStencilView> dsv;
	};
}
