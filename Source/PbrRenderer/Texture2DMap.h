#pragma once
#include "Common.h"

namespace PbrRenderer
{
	class RenderingSystem;

	class Texture2DMap final
	{
	public:
		Texture2DMap(RenderingSystem* rs, int w, int h, DXGI_FORMAT format);
		~Texture2DMap() {}

		ID3D11RenderTargetView* GetRTV() { return rtv.Get(); }
		ID3D11ShaderResourceView* GetSRV() { return srv.Get(); }

	private:
		ComPtr<ID3D11Texture2D> texture;
		ComPtr<ID3D11RenderTargetView> rtv;
		ComPtr<ID3D11ShaderResourceView> srv;
	};
}
