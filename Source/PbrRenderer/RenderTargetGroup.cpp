#include "RenderTargetGroup.h"
#include "RenderingSystem.h"
#include <DirectXColors.h>

void PbrRenderer::RenderTargetGroup::SetDefaultTargets(RenderingSystem* rs)
{
	rtv.clear();
	colors.clear();

	AddRenderTarget(rs->rendertargetView, DirectX::Colors::Black);
	SetDepthStencil(rs->depthStencilView);
}
