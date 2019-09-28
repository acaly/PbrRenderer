#include "Camera.h"
#include <Windows.h>
#include <Windowsx.h>
#include <cmath>

using namespace DirectX;

void PbrRenderer::Camera::RecalcOffset()
{
	static const XMVECTORF32 vectorY = { 0, 1, 0, 0 };
	static const XMVECTORF32 vectorNX = { -1, 0, 0, 0 };

	XMMATRIX cYawMatrix = XMMatrixRotationZ(yaw);
	XMStoreFloat4x4(&yawMatrix, cYawMatrix);

	XMVECTOR cCameraX = XMVector3Transform(vectorY, cYawMatrix);
	XMStoreFloat3(&cameraX, cCameraX);
	XMMATRIX cPitchMatrix = XMMatrixRotationAxis(cCameraX, -pitch);
	XMStoreFloat4x4(&pitchMatrix, cPitchMatrix);

	XMVECTOR frontHorizontal = XMVector3Transform(vectorNX, cYawMatrix);
	XMVECTOR cCameraZ = XMVector3Transform(frontHorizontal, cPitchMatrix);
	XMStoreFloat3(&cameraZ, cCameraZ);
	XMVECTOR cOffset = XMVectorScale(cCameraZ, length);
	XMStoreFloat3(&offset, cOffset);

	XMVECTOR cCameraY = XMVector3Cross(cCameraX, cCameraZ);
	XMStoreFloat3(&cameraY, cCameraY);
}

void PbrRenderer::Camera::RecalcProj()
{
	XMStoreFloat4x4(&mProj, XMMatrixPerspectiveFovRH(XM_PIDIV4, 4.f / 3, 0.1f, 1000.f));
}

void PbrRenderer::Camera::Step(float time)
{
}

void PbrRenderer::Camera::HandleWindowEvent(HWND hwnd, UINT umessage, WPARAM wparam, LPARAM lparam)
{
	switch (umessage)
	{
	case WM_MBUTTONDOWN:
		if (drag != 0) break;
		SetCapture(hwnd);
		drag = 2;
		lastX = GET_X_LPARAM(lparam);
		lastY = GET_Y_LPARAM(lparam);
		ctrlKey = wparam & MK_CONTROL;
		break;
	case WM_RBUTTONDOWN:
		if (drag != 0) break;
		SetCapture(hwnd);
		drag = 3;
		lastX = GET_X_LPARAM(lparam);
		lastY = GET_Y_LPARAM(lparam);
		ctrlKey = wparam & MK_CONTROL;
		break;
	case WM_MBUTTONUP:
		if (drag == 2)
		{
			ReleaseCapture();
		}
		break;
	case WM_RBUTTONUP:
		if (drag == 3)
		{
			ReleaseCapture();
		}
		break;
	case WM_CAPTURECHANGED:
		drag = 0;
		break;
	case WM_MOUSEMOVE:
	{
		int x = GET_X_LPARAM(lparam);
		int y = GET_Y_LPARAM(lparam);
		int dx = x - lastX;
		int dy = y - lastY;
		switch (drag)
		{
		case 3:
			if (ctrlKey)
			{
				auto oldOffset = XMLoadFloat3(&offset);
				yaw += dx * 0.003f; //no recalc
				SetPitch(pitch - dy * 0.003f); //trigger recalc (and also clamp pitch range)
				auto newTarget = XMVectorAdd(XMLoadFloat3(&target), XMVectorSubtract(XMLoadFloat3(&offset), oldOffset));
				XMStoreFloat3(&target, newTarget);
				RecalcOffset();
			}
			else
			{
				yaw -= dx * 0.005f; //no recalc
				SetPitch(pitch + dy * 0.005f); //trigger recalc
			}
			break;
		case 2:
		{
			auto cx = XMLoadFloat3(&cameraX);
			auto cy = XMLoadFloat3(&cameraY);
			auto dx2 = -dx * length * 0.002f;
			auto dy2 = dy * length * 0.002f;
			auto dd = XMVectorAdd(XMVectorScale(cx, dx2), XMVectorScale(cy, dy2));
			XMStoreFloat3(&target, XMVectorAdd(XMLoadFloat3(&target), dd));
			break;
		}
		default:
			break;
		}
		lastX = x;
		lastY = y;
		break;
	}
	case WM_MOUSEWHEEL:
	{
		int delta = GET_WHEEL_DELTA_WPARAM(wparam);
		int deltaDir = delta > 0 ? 1 : delta == 0 ? 0 : -1;
		if (ctrlKey)
		{
			XMStoreFloat3(&target,
				XMVectorAdd(XMLoadFloat3(&target), XMVectorScale(XMLoadFloat3(&cameraZ), deltaDir * length * 0.1f)));
		}
		else
		{
			if (delta < 0)
			{
				SetLength(length * 1.1f);
			}
			else if (delta > 0)
			{
				SetLength(length / 1.1f);
			}
		}
		break;
	}
	}
}

XMMATRIX PbrRenderer::Camera::GetViewMatrix() const
{
	static const XMVECTORF32 vectorZ = { 0, 0, 1, 0 };
	XMVECTOR cTarget = XMLoadFloat3(&target);
	XMVECTOR cOffset = XMLoadFloat3(&offset);
	return XMMatrixLookAtRH(XMVectorSubtract(cTarget, cOffset), cTarget, vectorZ);
}
