using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace PrimalEditor.Utilities.RenderSurface;

public class RenderSurfaceHost : HwndHost
{
    private readonly int _width = 800;
    private readonly int _height = 600;

    public int SurfaceId { get; private set; } = ID.INVALID_ID;
    private IntPtr _renderWindowHandle = IntPtr.Zero;

    public RenderSurfaceHost(int width, int height)
    {
        _width = width;
        _height = height;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        SurfaceId = EngineAPI.CreateRenderSurface(hwndParent.Handle, _width, _height);

        Debug.Assert(ID.IsValid(SurfaceId));

        _renderWindowHandle = EngineAPI.GetWindowHandle(SurfaceId);

        Debug.Assert(_renderWindowHandle != IntPtr.Zero);

        return new HandleRef(this, _renderWindowHandle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        EngineAPI.RemoveRenderSurface(SurfaceId);
        
        SurfaceId = ID.INVALID_ID;

        _renderWindowHandle = IntPtr.Zero;
    }
}