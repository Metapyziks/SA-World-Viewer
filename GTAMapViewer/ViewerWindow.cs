using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using GTAMapViewer.Scenes;

namespace GTAMapViewer
{
    internal class ViewerWindow : GameWindow
    {
        public Scene CurrentScene { get; private set; }

        public ViewerWindow()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 8 ), 16, 0 ), "GTA SA Model Viewer" )
        {
            VSync = VSyncMode.On;
            Context.SwapInterval = 1;

            WindowBorder = WindowBorder.Fixed;

            CurrentScene = null;
        }

        protected override void OnLoad( EventArgs e )
        {
            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonUp;
            Mouse.ButtonDown += OnMouseButtonDown;
            Mouse.WheelChanged += OnMouseWheelChanged;

            SetScene( new ModelViewScene( this ) );
        }

        protected override void OnResize( EventArgs e )
        {
            CurrentScene.OnResize();
        }

        public void SetScene( Scene newScene )
        {
            if ( CurrentScene != null )
                CurrentScene.OnExit();
            CurrentScene = newScene;
            if ( CurrentScene != null )
            {
                CurrentScene.OnEnter( CurrentScene.FirstTime );
                CurrentScene.FirstTime = false;
            }
        }

        protected override void OnUpdateFrame( FrameEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnUpdateFrame( e );
        }

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            if ( CurrentScene != null )
                CurrentScene.OnRenderFrame( e );

            SwapBuffers();
        }

        private void OnMouseMove( object sender, MouseMoveEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseMove( e );
        }

        private void OnMouseWheelChanged( object sender, MouseWheelEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseWheelChanged( e );
        }

        private void OnMouseButtonUp( object sender, MouseButtonEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseButtonUp( e );
        }

        private void OnMouseButtonDown( object sender, MouseButtonEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseButtonDown( e );
        }

        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnKeyPress( e );
        }

        protected override void OnMouseLeave( EventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseLeave( e );
        }

        protected override void OnMouseEnter( EventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseEnter( e );
        }

        public override void Dispose()
        {
            if ( CurrentScene != null )
                CurrentScene.Dispose();

            base.Dispose();
        }
    }
}
