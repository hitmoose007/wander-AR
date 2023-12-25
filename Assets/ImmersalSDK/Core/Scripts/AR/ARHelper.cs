/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Immersal.AR
{
	public class ARHelper {
		public static Matrix4x4 SwitchHandedness(Matrix4x4 b)
		{
			Matrix4x4 D = Matrix4x4.identity;
			D.m00 = -1;
			return D * b * D;
		}

		public static Quaternion SwitchHandedness(Quaternion b)
		{
			Matrix4x4 m = SwitchHandedness(Matrix4x4.Rotate(b));
			return m.rotation;
		}

		public static Vector3 SwitchHandedness(Vector3 b)
		{
			Matrix4x4 m = SwitchHandedness(Matrix4x4.TRS(b, Quaternion.identity, Vector3.one));
			return m.GetColumn(3);
		}

        public static void DoubleQuaternionToDoubleMatrix3x3(out double[] m, double[] q)
        {
            m = new double [] {1, 0, 0, 0, 1, 0, 0, 0, 1}; //identity matrix
            
            // input quaternion should be in WXYZ order
			double w = q[0];
			double x = q[1];
			double y = q[2];
			double z = q[3];
			
			double ww = w * w;
            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            
            double xy = x * y;
            double zw = z * w;
			double xz = x * z;
            double yw = y * w;
			double yz = y * z;
            double xw = x * w;

			double inv = 1.0 / (xx + yy + zz + ww);

            m[0] = ( xx - yy - zz + ww) * inv;
            m[1] = 2.0 * (xy - zw) * inv;
            m[2] = 2.0 * (xz + yw) * inv;
            m[3] = 2.0 * (xy + zw) * inv;
            m[4] = (-xx + yy - zz + ww) * inv;
            m[5] = 2.0 * (yz - xw) * inv;
            m[6] = 2.0 * (xz - yw) * inv;
            m[7] = 2.0 * (yz + xw) * inv;
            m[8] = (-xx - yy + zz + ww) * inv;
        }

		public static void GetIntrinsics(out Vector4 intrinsics)
        {
            intrinsics = Vector4.zero;
			XRCameraIntrinsics intr;
			ARCameraManager manager = ImmersalSDK.Instance?.cameraManager;

			if (manager != null && manager.TryGetIntrinsics(out intr))
			{
				intrinsics.x = intr.focalLength.x;
				intrinsics.y = intr.focalLength.y;
				intrinsics.z = intr.principalPoint.x;
				intrinsics.w = intr.principalPoint.y;
            }
        }

		public static void GetRotation(ref Quaternion rot)
		{
			float angle = 0f;
			switch (Screen.orientation)
			{
				case ScreenOrientation.Portrait:
					angle = 90f;
					break;
				case ScreenOrientation.LandscapeLeft:
					angle = 180f;
					break;
				case ScreenOrientation.LandscapeRight:
					angle = 0f;
					break;
				case ScreenOrientation.PortraitUpsideDown:
					angle = -90f;
					break;
				default:
					angle = 0f;
					break;
			}

			rot *= Quaternion.Euler(0f, 0f, angle);
		}

		public static void GetPlaneDataFast(ref IntPtr pixels, XRCpuImage image)
		{
			XRCpuImage.Plane plane = image.GetPlane(0);	// use the Y plane
			int width = image.width, height = image.height;

			if (width == plane.rowStride)
			{
				unsafe
				{
					pixels = (IntPtr)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(plane.data);
				}
			}
			else
			{
				byte[] data = new byte[width * height];

				unsafe
				{
					fixed (byte* dstPtr = data)
					{
						byte* srcPtr = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(plane.data);
						if (width > 0 && height > 0) {
							UnsafeUtility.MemCpyStride(dstPtr, width, srcPtr, plane.rowStride, width, height);
						}
						pixels = (IntPtr)dstPtr;
					}
				}
			}
		}
		
		public static void GetPlaneData(out byte[] pixels, XRCpuImage image)
		{
			XRCpuImage.Plane plane = image.GetPlane(0);	// use the Y plane
			int width = image.width, height = image.height;
			pixels = new byte[width * height];

			if (width == plane.rowStride)
			{
				plane.data.CopyTo(pixels);
			}
			else
			{
				unsafe
				{
					fixed (byte* dstPtr = pixels)
					{
						byte* srcPtr = (byte*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(plane.data);
						if (width > 0 && height > 0) {
							UnsafeUtility.MemCpyStride(dstPtr, width, srcPtr, plane.rowStride, width, height);
						}
					}
				}
			}
		}

		public static void GetPlaneDataRGB(out byte[] pixels, XRCpuImage image)
		{
			var conversionParams = new XRCpuImage.ConversionParams
			{
				inputRect = new RectInt(0, 0, image.width, image.height),
				outputDimensions = new Vector2Int(image.width, image.height),
				outputFormat = TextureFormat.RGB24,
				transformation = XRCpuImage.Transformation.None
			};

			int size = image.GetConvertedDataSize(conversionParams);
			pixels = new byte[size];
			GCHandle bufferHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			image.Convert(conversionParams, bufferHandle.AddrOfPinnedObject(), pixels.Length);
			bufferHandle.Free();
		}

		public static bool TryGetTrackingQuality(out int quality)
		{
			quality = default;

			if (ImmersalSDK.Instance?.arSession == null)
				return false;
						
			var arSubsystem = ImmersalSDK.Instance?.arSession.subsystem;
			
			if (arSubsystem != null && arSubsystem.running)
			{
				switch (arSubsystem.trackingState)
				{
					case TrackingState.Tracking:
						quality = 4;
						break;
					case TrackingState.Limited:
						quality = 1;
						break;
					case TrackingState.None:
						quality = 0;
						break;
				}
			}

			return true;
		}
	}
}
