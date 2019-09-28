#pragma once
#include "Common.h"

namespace PbrRenderer
{
	class Camera final
	{
	public:
		Camera(DirectX::XMFLOAT3 targetPos) : target(targetPos)
		{
			RecalcOffset();
			RecalcProj();
		}

		~Camera() {}

	public:
		DirectX::XMFLOAT3 target;

		float GetYaw() const { return yaw; }
		float GetPitch() const { return pitch; }
		float GetLength() const { return length; }

		void SetYaw(float val)
		{
			yaw = val;
			RecalcOffset();
		}

		void SetPitch(float val)
		{
			const float PitchRange = 1.5707f;
			pitch = DirectX::XMMax(DirectX::XMMin(val, PitchRange), -PitchRange);
			RecalcOffset();
		}

		void SetLength(float val)
		{
			length = val;
			RecalcOffset();
		}

	private:
		void RecalcOffset();
		void RecalcProj();

	public:
		void Step(float time);
		void HandleWindowEvent(HWND hwnd, UINT umessage, WPARAM wparam, LPARAM lparam);

		DirectX::XMMATRIX GetViewMatrix() const;
		DirectX::XMMATRIX GetProjectionMatrix() const
		{
			return DirectX::XMLoadFloat4x4(&mProj);
		}

	private:
		float yaw = 0, pitch = 0, length = 10;

		DirectX::XMFLOAT3 offset = {}, cameraX = {}, cameraY = {}, cameraZ = {};
		DirectX::XMFLOAT4X4 yawMatrix, pitchMatrix;

		DirectX::XMFLOAT4X4 mProj;
		DirectX::XMFLOAT4X4 mView;

		int drag = 0, lastX = 0, lastY = 0;
		bool ctrlKey = false;
	};
}
