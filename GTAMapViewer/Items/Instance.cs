using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.Items
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

        public Instance( UInt32 objID )
        {
            Object = ItemManager.GetObject( objID );

            Position = new Vector3();
            Rotation = new Quaternion();
            LOD = null;
            IsLOD = false;
        }

        public void Place( Vector3 pos, Quaternion rot, Instance lod = null )
        {
            Position = pos;
            Rotation = rot;
            LOD = lod;

            if ( HasLOD )
                LOD.IsLOD = true;
        }

        public void Render( ModelShader shader )
        {
            if ( Object.ModelLoaded )
            {
                shader.ModelPos = Position;
                shader.ModelRot = Rotation;
                shader.Render( Object.Model );
            }
        }
    }
}
