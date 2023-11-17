// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using PrimalEditor.Common;
using PrimalEditor.Utilities.RenderSurface;

namespace PrimalEditor.Utilities
{
    public partial class RenderSurfaceView : UserControl, IDisposable
    {
        private enum Win32Msg
        {
            WM_SIZING = 0x0214,
            WM_SIZE = 0x0005,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232
        }

        private RenderSurfaceHost _host = null;
        private bool _canResize = true;
        private bool _moved = false;

        public RenderSurfaceView()
        {
            InitializeComponent();

            Loaded += OnRenderSurfaceViewLoaded;
        }

        private void OnRenderSurfaceViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnRenderSurfaceViewLoaded;

            _host = new RenderSurfaceHost((int) ActualWidth, (int) ActualHeight);

            _host.MessageHook += new HwndSourceHook(HostMessageFilter);

            Content = _host;

            var window = this.FindVisualParent<Window>();

            Debug.Assert(window != null);

            var helper = new WindowInteropHelper(window);

            HwndSource.FromHwnd(helper.Handle)?.AddHook(HwndMessageHook);
        }

        private IntPtr HwndMessageHook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            switch ((Win32Msg)msg)
            {
                case Win32Msg.WM_SIZING:
                    _canResize = false;
                    _moved = false;
                    break;
                case Win32Msg.WM_ENTERSIZEMOVE:
                    _moved = true;
                    break;
                case Win32Msg.WM_EXITSIZEMOVE:
                    _canResize = true;

                    if (!_moved)
                    {
                        _host.Resize();
                    }

                    break;
                default:
                    break;
            }

            return IntPtr.Zero;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private IntPtr HostMessageFilter(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            switch ((Win32Msg) msg)
            {
                case Win32Msg.WM_SIZING: throw new Exception();
                case Win32Msg.WM_ENTERSIZEMOVE: throw new Exception();
                case Win32Msg.WM_EXITSIZEMOVE: throw new Exception();
                case Win32Msg.WM_SIZE:
                    if (_canResize)
                    {
                        _host.Resize();
                    }
                    break;
                default:
                    break;
            }

            return IntPtr.Zero;
        }


        #region IDisposable
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _host.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
