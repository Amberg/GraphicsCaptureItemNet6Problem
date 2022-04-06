// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.DirectX;
using System.Diagnostics;

namespace GraphicsCaptureItemNet6Problem;
internal class Program
{
	static readonly Guid GraphicsCaptureItemGuid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");

	[ComImport]
	[Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	interface IGraphicsCaptureItemInterop
	{
		IntPtr CreateForWindow(
				[In] IntPtr window,
				[In] ref Guid iid);
	}

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
        Console.WriteLine("Hello World!");
		var mon = MonitorEnumerationHelper.GetMonitors().Where(m => m.IsPrimary).First();

		using var sharpDxDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport);
		IDirect3DDevice direct3dDevice = CreateDirect3DDeviceFromSharpDXDevice(sharpDxDevice);

		var process = Process.Start(new ProcessStartInfo 
		{	FileName = "explorer.exe",
			WindowStyle= ProcessWindowStyle.Minimized
		});
		try
		{


			var grapicsCaptureItem = CreateItemForHwnd(process.MainWindowHandle);


			using var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
							direct3dDevice,
							DirectXPixelFormat.B8G8R8A8UIntNormalized,
							2,
							grapicsCaptureItem.Size);
		}
		finally
		{
			process.Kill();
		}
	}


	private static GraphicsCaptureItem CreateItemForHwnd(IntPtr hwnd)
	{
		var factory = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
		var itemPointer = factory.CreateForWindow(hwnd, GraphicsCaptureItemGuid);
		var item = GraphicsCaptureItem.FromAbi(itemPointer);
		Marshal.Release(itemPointer);
		return item;
	}

	private static IDirect3DDevice CreateDirect3DDeviceFromSharpDXDevice(SharpDX.Direct3D11.Device sharpDxDevice)
	{
		IDirect3DDevice device = null;
		using (var dxgiDevice = sharpDxDevice.QueryInterface<SharpDX.DXGI.Device3>())
		{
			uint hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.NativePointer, out IntPtr pUnknown);
			if (hr == 0)
			{
				device = Marshal.GetObjectForIUnknown(pUnknown) as IDirect3DDevice;
				Marshal.Release(pUnknown);
			}
		}
		return device;
	}
}
