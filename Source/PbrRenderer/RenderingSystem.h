#pragma once
#include "Common.h"

namespace PbrRenderer
{
	class IWindowsEventHandler
	{
	public:
		virtual ~IWindowsEventHandler() {}
		virtual void OnEventBefore(HWND hwnd, UINT umessage, WPARAM wparam, LPARAM lparam) = 0;
	};

	class RenderingSystem final
	{
	public:
		RenderingSystem(int w, int h) : window_width(w), window_height(h) {}
		~RenderingSystem();

	public:
		const int window_width, window_height;
		HWND window = 0;

		ComPtr<IDXGIFactory1> factory;
		ComPtr<ID3D11Device> device;
		ComPtr<ID3D11DeviceContext> immediateContext;
		ComPtr<IDXGISwapChain> swapChain;
		ComPtr<ID3D11RenderTargetView> rendertargetView;
		ComPtr<ID3D11DepthStencilView> depthStencilView;

	private:
		IWindowsEventHandler* eventHandler = 0;

	public:
		void Initialize();
		void Present();
		bool ProcessMessage();
		void SetEventHandler(IWindowsEventHandler* r)
		{
			eventHandler = r;
		}

	private:
		void CreateMainWindow();
		void CreateDevice();
		void CreateFactory();
		ComPtr<IDXGIAdapter> SelectAdapter();
		static LRESULT CALLBACK WndProc(HWND hwnd, UINT umessage, WPARAM wparam, LPARAM lparam);
	};
}
