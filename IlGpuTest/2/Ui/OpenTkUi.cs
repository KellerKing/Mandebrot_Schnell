using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace IlGpuTest._2.Ui
{
    internal class OpenTkUi : GameWindow
    {
        public OpenTkUi(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        public OpenTkUi(int width, int height, string titel) : 
            base(GameWindowSettings.Default, new NativeWindowSettings { ClientSize = (width, height), Title = titel})
        {

        }

        //https://opentk.net/learn/chapter1/2-hello-triangle.html?tabs=onload-opentk4%2Conrender-opentk4%2Cresize-opentk4

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
                Close();
        }
    }
}
