using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace PrimalEditor.Utilities.RenderSurface;

public class RenderSurfaceHost : HwndHost
{
    // ReSharper disable MemberInitializerValueIgnored
    private readonly int _width = 800;
    private readonly int _height = 600;

    public static int SurfaceId { get; private set; } = ID.INVALID_ID;
    private IntPtr _renderWindowHandle = IntPtr.Zero;

    private readonly DelayEventTimer _resizeTimer;

    public RenderSurfaceHost(int width, int height)
    {
        _width = width;
        _height = height;

        _resizeTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250.0));
        _resizeTimer.Triggered += Resize;
    }

    public void Resize()
    {
        _resizeTimer.Trigger();
    }
    private static void Resize(object sender, DelayEventTimerArgs e)
    {
        e.RepeatEvent = Mouse.LeftButton == MouseButtonState.Pressed;

        if (!e.RepeatEvent)
        {
            EngineAPI.ResizeRenderSurface(SurfaceId);
        }
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
        
        // TODO: Check to see if this correct.

        //SurfaceId = ID.INVALID_ID;

        //_renderWindowHandle = IntPtr.Zero;
    }
}