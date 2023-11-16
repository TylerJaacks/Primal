using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
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
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private IntPtr HostMessageFilter(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            switch ((Win32Msg) msg)
            {
                case Win32Msg.WM_SIZING:
                case Win32Msg.WM_ENTERSIZEMOVE:
                case Win32Msg.WM_EXITSIZEMOVE:
                    throw new Exception();
                case Win32Msg.WM_SIZE:
                    _host.Resize();
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
