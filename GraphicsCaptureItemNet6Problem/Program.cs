// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.DirectX;
using Windows.Graphics;

namespace GraphicsCaptureItemNet6Problem;
internal class Program
{

	[DllImport(
			"d3d11.dll",
			EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice",
			SetLastError = true,
			CharSet = CharSet.Unicode,
			ExactSpelling = true,
			CallingConvention = CallingConvention.StdCall
			)]
	static extern UInt32 CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);

	static void Main(string[] args)
    {

		using var sharpDxDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport);
		IDirect3DDevice direct3dDevice = CreateDirect3DDeviceFromSharpDXDevice(sharpDxDevice);

		// this will throw internal cast exception 
	    using var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
							direct3dDevice,
							DirectXPixelFormat.B8G8R8A8UIntNormalized,
							2,
							new SizeInt32(64, 64));
	
	}

	private static IDirect3DDevice CreateDirect3DDeviceFromSharpDXDevice(SharpDX.Direct3D11.Device sharpDxDevice)
	{
		IDirect3DDevice device = null;
		using (var dxgiDevice = sharpDxDevice.QueryInterface<SharpDX.DXGI.Device3>())
		{
			uint hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.NativePointer, out IntPtr pUnknown);
			if (hr == 0)
			{
				// with NET 6 there should be something like 
				// IDirect3DDevice.FromAbi(pUnknown)
				// see here
				device = Marshal.GetObjectForIUnknown(pUnknown) as IDirect3DDevice;
				Marshal.Release(pUnknown);
			}
		}
		return device;
	}
}
