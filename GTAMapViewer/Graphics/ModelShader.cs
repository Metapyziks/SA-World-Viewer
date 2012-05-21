using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using GTAMapViewer.Resource;

namespace GTAMapViewer.Graphics
{
    internal class ModelShader : ShaderProgram
    {
        private Model myCurrentModel;

        private Matrix4 myViewMatrix;
        private int myViewMatrixLoc;

        private Vector3 myModelPos;
        private int myModelPosLoc;

        private Quaternion myModelRot;
        private int myModelRotLoc;

        private Color4 myColour;
        private int myColourLoc;

        private bool myAlphaMask;
        private int myAlphaMaskLoc;

        private Vector3 myCameraPosition;
        private Vector2 myCameraRotation;
        private Matrix4 myPerspectiveMatrix;

        private bool myPerspectiveChanged;
        private bool myViewChanged;

        private bool myBackfaceCulling;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Vector3 CameraPosition
        {
            get
            {
                return myCameraPosition;
            }
            set
            {
                myCameraPosition = value;
                myViewChanged = true;
            }
        }
        public Vector2 CameraRotation
        {
            get { return myCameraRotation; }
            set
            {
                myCameraRotation = value;
                myViewChanged = true;
            }
        }
        public Matrix4 PerspectiveMatrix
        {
            get { return myPerspectiveMatrix; }
            set
            {
                myPerspectiveMatrix = value;
                myPerspectiveChanged = true;
            }
        }
        public Vector3 ModelPos
        {
            get { return myModelPos; }
            set
            {
                myModelPos = value;
                GL.Uniform3( myModelPosLoc, value );
            }
        }
        public Quaternion ModelRot
        {
            get { return myModelRot; }
            set
            {
                myModelRot = value;
                GL.Uniform4( myModelRotLoc, value );
            }
        }
        public Color4 Colour
        {
            get { return myColour; }
            set
            {
                myColour = value;
                GL.Uniform4( myColourLoc, value );
            }
        }
        public bool AlphaMask
        {
            get { return myAlphaMask; }
            set
            {
                if ( value != myAlphaMask )
                {
                    myAlphaMask = value;
                    GL.Uniform1( myAlphaMaskLoc, value ? 1 : 0 );
                }
            }
        }

        public bool BackfaceCulling
        {
            get { return myBackfaceCulling; }
            set
            {
                if ( myBackfaceCulling != value )
                {
                    myBackfaceCulling = value;
                    if ( value )
                        GL.Enable( EnableCap.CullFace );
                    else
                        GL.Disable( EnableCap.CullFace );
                }
            }
        }

        public ModelShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddUniform( ShaderVarType.Vec3, "model_pos" );
            vert.AddUniform( ShaderVarType.Vec4, "model_rot" );
            vert.AddUniform( ShaderVarType.Vec4, "colour" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_position" );
            vert.AddAttribute( ShaderVarType.Vec2, "in_texcoord" );
            vert.AddAttribute( ShaderVarType.Vec4, "in_colour" );
            vert.AddVarying( ShaderVarType.Vec2, "var_texcoord" );
            vert.AddVarying( ShaderVarType.Vec4, "var_colour" );
            vert.Logic = @"
                vec3 qtransform( vec4 q, vec3 v )
                { 
	                return v + 2.0 * cross( cross( v, q.xyz ) + q.w * v, q.xyz );
	            }

                void main( void )
                {
                    gl_Position = view_matrix * vec4( qtransform( model_rot, in_position ) + model_pos, 1 );
                    var_texcoord = in_texcoord;
                    var_colour = colour * in_colour;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddUniform( ShaderVarType.Sampler2D, "tex_diffuse" );
            frag.AddUniform( ShaderVarType.Sampler2D, "tex_mask" );
            frag.AddUniform( ShaderVarType.Bool, "flag_mask" );
            frag.AddVarying( ShaderVarType.Vec2, "var_texcoord" );
            frag.AddVarying( ShaderVarType.Vec4, "var_colour" );
            frag.Logic = @"
                void main( void )
                {
                    if( var_colour.a == 0.0 )
                        discard;
                    vec3 clr = texture2D( tex_diffuse, var_texcoord ).rgb;
                    if( flag_mask && texture2D( tex_mask, var_texcoord ).a == 0.0 )
                        discard;
                    out_frag_colour = vec4( clr * var_colour.rgb, var_colour.a );
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.TriangleStrip;

            myCameraPosition = new Vector3();
            myCameraRotation = new Vector2( MathHelper.Pi * 30.0f / 180.0f, 0.0f );

            myColour = Color4.White;
            myAlphaMask = false;

            myPerspectiveChanged = true;
            myViewChanged = true;

            myBackfaceCulling = false;
        }

        public ModelShader( int width, int height )
            : this()
        {
            Create();
            SetScreenSize( width, height );
        }

        public void SetScreenSize( int width, int height )
        {
            ScreenWidth = width;
            ScreenHeight = height;
            UpdatePerspectiveMatrix();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_position", 3 );
            AddAttribute( "in_texcoord", 2 );
            AddAttribute( "in_colour", 4 );

            AddTexture( "tex_diffuse", TextureUnit.Texture0 );
            AddTexture( "tex_mask", TextureUnit.Texture1 );

            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
            myModelPosLoc = GL.GetUniformLocation( Program, "model_pos" );
            myModelRotLoc = GL.GetUniformLocation( Program, "model_rot" );
            myColourLoc = GL.GetUniformLocation( Program, "colour" );
            myAlphaMaskLoc = GL.GetUniformLocation( Program, "flag_mask" );

            ModelPos = new Vector3();
            Colour = Color4.White;
            AlphaMask = false;
        }

        private void UpdatePerspectiveMatrix()
        {
            PerspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView( (float) Math.PI * ( 60.0f / 180.0f ),
                (float) ScreenWidth / (float) ScreenHeight, 0.125f, 4096.0f );
            UpdateViewMatrix();

            myPerspectiveChanged = false;
        }

        private void UpdateViewMatrix()
        {
            Matrix4 yRot = Matrix4.CreateRotationY( myCameraRotation.Y );
            Matrix4 xRot = Matrix4.CreateRotationX( myCameraRotation.X );
            Matrix4 trns = Matrix4.CreateTranslation( -myCameraPosition );

            myViewMatrix = Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( trns, yRot ), xRot ), myPerspectiveMatrix );
            GL.UniformMatrix4( myViewMatrixLoc, false, ref myViewMatrix );

            myViewChanged = false;
        }

        protected override void OnStartBatch()
        {
            if ( myPerspectiveChanged )
                UpdatePerspectiveMatrix();
            else if ( myViewChanged )
                UpdateViewMatrix();

            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.Blend );
            GL.Enable( EnableCap.PrimitiveRestart );

            GL.CullFace( CullFaceMode.Back );
            GL.BlendFunc( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
            GL.PrimitiveRestartIndex( 0xffff );

            myCurrentModel = null;
        }

        public void Render( Model model )
        {
            if ( model != myCurrentModel )
            {
                if ( myCurrentModel != null )
                    myCurrentModel.VertexBuffer.EndBatch( this );
                model.VertexBuffer.StartBatch( this );
                myCurrentModel = model;
            }

            model.Render( this );
        }

        protected override void OnEndBatch()
        {
            if ( myCurrentModel != null )
                myCurrentModel.VertexBuffer.EndBatch( this );

            GL.Disable( EnableCap.DepthTest );
            GL.Disable( EnableCap.Blend );
            GL.Disable( EnableCap.PrimitiveRestart );
        }
    }
}
