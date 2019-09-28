#include "RenderingSystem.h"
#include <dxgi1_6.h>
#include <DirectXMath.h>
#include <DirectXColors.h>
#include <unordered_map>

namespace
{
	using namespace PbrRenderer;
	thread_local std::unordered_map<HWND, RenderingSystem*> systemPtr;
}

PbrRenderer::RenderingSystem::~RenderingSystem()
{
	systemPtr.erase(window);
}

void PbrRenderer::RenderingSystem::Initialize()
{
	CreateMainWindow();
	CreateFactory();
	CreateDevice();
}

void PbrRenderer::RenderingSystem::Present()
{
	swapChain->Present(1, 0);
}

bool PbrRenderer::RenderingSystem::ProcessMessage()
{
	MSG msg = {};
	while (PeekMessage(&msg, 0, 0, 0, PM_REMOVE))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
		if (msg.message == WM_QUIT)
		{
			return false;
		}
	}
	return true;
}

void PbrRenderer::RenderingSystem::CreateMainWindow()
{
	WNDCLASSEX wc;
	int posX, posY;

	HMODULE m_hinstance = GetModuleHandle(NULL);

	LPCWSTR m_applicationName = L"PBR Renderer";

	wc.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
	wc.lpfnWndProc = WndProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = m_hinstance;
	wc.hIcon = LoadIcon(NULL, IDI_WINLOGO);
	wc.hIconSm = wc.hIcon;
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
	wc.lpszMenuName = NULL;
	wc.lpszClassName = m_applicationName;
	wc.cbSize = sizeof(WNDCLASSEX);

	RegisterClassEx(&wc);

	posX = (GetSystemMetrics(SM_CXSCREEN) - window_width) / 2;
	posY = (GetSystemMetrics(SM_CYSCREEN) - window_height) / 2;

	HWND hwnd = CreateWindowEx(WS_EX_APPWINDOW, m_applicationName, m_applicationName,
		WS_OVERLAPPED | WS_SYSMENU | WS_MINIMIZEBOX,
		posX, posY, window_width, window_height, NULL, NULL, m_hinstance, NULL);
	systemPtr[hwnd] = this;

	ShowWindow(hwnd, SW_SHOW);
	SetForegroundWindow(hwnd);
	SetFocus(hwnd);

	window = hwnd;
}

void PbrRenderer::RenderingSystem::CreateDevice()
{
	auto pAdapter = SelectAdapter();

	DXGI_SWAP_CHAIN_DESC swapChainDesc;

	ZeroMemory(&swapChainDesc, sizeof(swapChainDesc));
	swapChainDesc.BufferCount = 1;
	swapChainDesc.BufferDesc.Width = window_width;
	swapChainDesc.BufferDesc.Height = window_height;
	swapChainDesc.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	swapChainDesc.BufferDesc.RefreshRate.Numerator = 0;
	swapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
	swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	swapChainDesc.OutputWindow = window;

	swapChainDesc.SampleDesc.Count = 1;
	swapChainDesc.SampleDesc.Quality = 0;

	if (0)
	{
		swapChainDesc.Windowed = false;
	}
	else
	{
		swapChainDesc.Windowed = true;
	}

	swapChainDesc.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
	swapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;

	swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	swapChainDesc.Flags = 0;

	D3D_FEATURE_LEVEL fli = D3D_FEATURE_LEVEL_11_0;
	D3D_FEATURE_LEVEL fl;

	auto r = D3D11CreateDeviceAndSwapChain(pAdapter.Get(), pAdapter ? D3D_DRIVER_TYPE_UNKNOWN : D3D_DRIVER_TYPE_HARDWARE,
		0, D3D11_CREATE_DEVICE_DEBUG, &fli, 1, D3D11_SDK_VERSION, &swapChainDesc,
		swapChain.GetAddressOf(), device.GetAddressOf(), &fl, immediateContext.GetAddressOf());
	CheckComError(r);

	ComPtr<ID3D11Texture2D> rtbuffer;
	CheckComError(swapChain->GetBuffer(0, IID_PPV_ARGS(rtbuffer.GetAddressOf())));
	CheckComError(device->CreateRenderTargetView(rtbuffer.Get(), 0, rendertargetView.GetAddressOf()));

	CD3D11_TEXTURE2D_DESC depthStencilDesc(DXGI_FORMAT_D32_FLOAT, window_width, window_height, 1, 1, D3D11_BIND_DEPTH_STENCIL);
	CD3D11_DEPTH_STENCIL_VIEW_DESC depthStencilViewDesc(D3D11_DSV_DIMENSION_TEXTURE2D);
	ComPtr<ID3D11Texture2D> depthBuffer;
	CheckComError(device->CreateTexture2D(&depthStencilDesc, 0, depthBuffer.GetAddressOf()));
	CheckComError(device->CreateDepthStencilView(depthBuffer.Get(), &depthStencilViewDesc, depthStencilView.GetAddressOf()));
}

void PbrRenderer::RenderingSystem::CreateFactory()
{
	CheckComError(CreateDXGIFactory1(IID_PPV_ARGS(factory.GetAddressOf())));
}

ComPtr<IDXGIAdapter> PbrRenderer::RenderingSystem::SelectAdapter()
{
	ComPtr<IDXGIAdapter1> adapter;

	SIZE_T mem = 0;
	ComPtr<IDXGIAdapter1> selectedAdapter;

	for (UINT adapterIndex = 0;
		SUCCEEDED(factory->EnumAdapters1(
			adapterIndex,
			adapter.ReleaseAndGetAddressOf()));
		adapterIndex++)
	{
		DXGI_ADAPTER_DESC1 desc;
		CheckComError(adapter->GetDesc1(&desc));

		if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
		{
			continue;
		}

		if (mem < desc.DedicatedVideoMemory || !selectedAdapter)
		{
			selectedAdapter.Swap(adapter);
		}
	}

	return selectedAdapter;
}

LRESULT CALLBACK PbrRenderer::RenderingSystem::WndProc(HWND hwnd, UINT umessage, WPARAM wparam, LPARAM lparam)
{
	auto i = systemPtr.find(hwnd);
	if (i != systemPtr.end())
	{
		auto pthis = i->second;
		if (pthis->eventHandler)
		{
			pthis->eventHandler->OnEventBefore(hwnd, umessage, wparam, lparam);
		}
	}
	switch (umessage)
	{
	case WM_DESTROY:
	case WM_CLOSE:
		PostQuitMessage(0);
		return 0;
	default:
		return DefWindowProc(hwnd, umessage, wparam, lparam);
	}
}
