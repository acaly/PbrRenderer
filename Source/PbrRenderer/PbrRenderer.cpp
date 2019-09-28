#include "RenderingSystem.h"
#include "TestScene.h"

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow)
{
	PbrRenderer::RenderingSystem rs(800, 600);
	rs.Initialize();
	PbrRenderer::TestScene scene(&rs);
	scene.Initialize();
	rs.SetEventHandler(&scene);

	rs.ProcessMessage();
	while (rs.ProcessMessage())
	{
		scene.Render();
		rs.Present();
	}
	
	return 0;
}
