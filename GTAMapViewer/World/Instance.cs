using System;

using OpenTK;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.World
{
    internal class Instance
    {
        public ObjectDefinition Object
        {
            get;
            private set;
        }

        public Vector3 Position
        {
            get;
            private set;
        }

        public Quaternion Rotation
        {
            get;
            private set;
        }

        public Instance LOD
        {
            get;
            private set;
        }
        public bool HasLOD
        {
            get { return LOD != null; }
        }
        public bool IsLOD
        {
            get;
            private set;
        }

        public Instance( InstPlacement placement )
        {
            Object = placement.Object;

            Position = placement.Position;
            Rotation = placement.Rotation;
            LOD = ( placement.HasLOD ? new Instance( placement.LODPlacement ) : null );
            IsLOD = placement.IsLOD;
        }

        public void Render( ModelShader shader )
        {
            float dist2 = ( shader.CameraPosition - Position ).LengthSquared;

            if ( ( Object.DrawDist >= 300.0f && !HasLOD && dist2 < shader.ViewRange2 ) ||
                dist2 < Object.DrawDist2 )
            {
                if ( !Object.Loaded )
                    Object.Load();

                if ( Object.Model != null )
                {
                    shader.ModelPos = Position;
                    shader.ModelRot = Rotation;
                    shader.BackfaceCulling = !Object.HasFlags( ObjectFlag.NoBackCull );

                    shader.Render( Object.Model );
                }
            }
            else
            {
                if ( Object.Loaded )
                    Object.Unload();

                if ( HasLOD )
                    LOD.Render( shader );
            }
        }
    }
}
