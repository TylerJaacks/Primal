using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace PrimalEditor.Utilities.RenderSurface;

class RenderSurfaceHost : HwndHost
{
    // ReSharper disable MemberInitializerValueIgnored
    private readonly int VK_LBUTTON = 0x1;
    private readonly int _width = 800;
    private readonly int _height = 600;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public static int SurfaceId { get; private set; } = ID.INVALID_ID;
    private IntPtr _renderWindowHandle = IntPtr.Zero;

    private readonly DelayEventTimer _resizeTimer;

    public RenderSurfaceHost(double width, double height)
    {
        _width = (int) width;
        _height = (int) height;

        _resizeTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250.0));
        _resizeTimer.Triggered += Resize;

        SizeChanged += (s, e) => _resizeTimer.Trigger();
    }

    private void Resize(object sender, DelayEventTimerArgs e)
    {
        e.RepeatEvent = GetAsyncKeyState(vKey: VK_LBUTTON) < 0;

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