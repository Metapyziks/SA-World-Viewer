using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using GTAMapViewer.Graphics;
using GTAMapViewer.DFF;

namespace GTAMapViewer.Scenes
{
    internal class ModelViewScene : Scene
    {
        private ModelShader myShader;
        private Model myCurrentModel;

        private bool myIgnoreMouse;
        private bool myCaptureMouse;

        public ModelViewScene( ViewerWindow gameWindow )
            : base( gameWindow )
        {
            myIgnoreMouse = false;
            myCaptureMouse = true;
        }

        public override void OnEnter( bool firstTime )
        {
            base.OnEnter( firstTime );

            CursorVisible = !myCaptureMouse;

            if ( firstTime )
            {
                myShader = new ModelShader( Width, Height );
                myShader.CameraPosition = new Vector3( 0.0f, 0.0f, -8.0f );
                myCurrentModel = GameData.LoadModel( "des_farmhouse1_.dff" );
            }
        }

        public override void OnExit()
        {
            CursorVisible = true;
        }

        public override void OnKeyPress( KeyPressEventArgs e )
        {
            switch ( e.KeyChar )
            {
                case (char) 0x1B:
                    myCaptureMouse = !myCaptureMouse;
                    CursorVisible = !myCaptureMouse;
                    break;
            }
        }

        public override void OnUpdateFrame( OpenTK.FrameEventArgs e )
        {
            Vector3 movement = new Vector3( 0.0f, 0.0f, 0.0f );
            float angleY = myShader.CameraRotation.Y;
            float angleX = myShader.CameraRotation.X;

            if ( Keyboard[ Key.D ] )
            {
                movement.X += (float) Math.Cos( angleY );
                movement.Z += (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.A ] )
            {
                movement.X -= (float) Math.Cos( angleY );
                movement.Z -= (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.S ] )
            {
                movement.Z += (float) Math.Cos( angleY ) * (float) Math.Cos( angleX );
                movement.Y += (float) Math.Sin( angleX );
                movement.X -= (float) Math.Sin( angleY ) * (float) Math.Cos( angleX );
            }
            if ( Keyboard[ Key.W ] )
            {
                movement.Z -= (float) Math.Cos( angleY ) * (float) Math.Cos( angleX );
                movement.Y -= (float) Math.Sin( angleX );
                movement.X += (float) Math.Sin( angleY ) * (float) Math.Cos( angleX );
            }

            if ( movement.Length != 0 )
            {
                movement.Normalize();
                myShader.CameraPosition = myShader.CameraPosition + movement * 0.125f;
            }
        }

        public override void OnMouseMove( MouseMoveEventArgs e )
        {
            if ( myIgnoreMouse )
            {
                myIgnoreMouse = false;
                return;
            }

            if ( myCaptureMouse )
            {
                Vector2 rot = myShader.CameraRotation;

                rot.Y += e.XDelta / 180.0f;
                rot.X += e.YDelta / 180.0f;
                rot.X = Tools.Clamp( rot.X, (float) -Math.PI / 2.0f, (float) Math.PI / 2.0f );

                myShader.CameraRotation = rot;

                myIgnoreMouse = true;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point( Bounds.Left + Width / 2, Bounds.Top + Height / 2 );
            }
        }

        public override void OnRenderFrame( OpenTK.FrameEventArgs e )
        {
            myShader.StartBatch();
            myShader.Render( myCurrentModel );
            myShader.EndBatch();
        }
    }
}
