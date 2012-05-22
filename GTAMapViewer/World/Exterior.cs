using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.World
{
    internal class ExteriorBlock : Cell
    {
        private List<Instance> myInstances;

        public readonly Vector2 CenterPos;

        public float Radius { get; private set; }
        public float Radius2 { get; private set; }

        public ExteriorBlock( Vector2 centerPos )
        {
            CenterPos = centerPos;
            Radius = Radius2 = 0;
        }

        protected override void OnFinalisePlacements( ICollection<InstPlacement> placements )
        {
            myInstances = new List<Instance>();
            foreach ( InstPlacement p in placements )
            {
                myInstances.Add( new Instance( p ) );
                Vector3 pos = p.Position;
                Vector2 diff = new Vector2( pos.X, pos.Z ) - CenterPos;
                float rad = diff.Length + p.Object.DrawDist;
                if ( rad > Radius )
                    Radius = rad;
            }
            Radius2 = Radius * Radius;
        }

        public override ICollection<Instance> GetInstances()
        {
            return myInstances;
        }

        public override void Render( ModelShader shader )
        {
            foreach ( Instance inst in myInstances )
                inst.Render( shader );
        }
    }

    internal class Exterior : Cell
    {
        public const float BlockSize = 256.0f;

        public static readonly float HBlockSize = BlockSize / 2.0f;
        public static readonly float BlockRadius = (float) ( Math.Sqrt( 2.0 ) * HBlockSize );
        public static readonly float BlockRadius2 = (float) ( Math.Sqrt( 2.0 ) * HBlockSize );

        private Vector4 myBounds;
        private int myGridWidth;
        private int myGridDepth;

        private ExteriorBlock[ , ] myInstGrid;

        public Exterior()
        {
            myBounds = new Vector4();
        }

        private int GetBlockX( float x )
        {
            return Tools.Clamp( (int) Math.Floor( ( x - myBounds.X ) / BlockSize ), 0, myGridWidth - 1 );
        }
        private int GetBlockZ( float z )
        {
            return Tools.Clamp( (int) Math.Floor( ( z - myBounds.Y ) / BlockSize ), 0, myGridDepth - 1 );
        }

        private Vector2 GetBlockPos( int x, int y )
        {
            return new Vector2( myBounds.X + x * BlockSize + HBlockSize, myBounds.Y + y * BlockSize + HBlockSize );
        }

        protected override void OnFinalisePlacements( ICollection<InstPlacement> placements )
        {
            Vector2 min = new Vector2();
            Vector2 max = new Vector2();

            foreach ( InstPlacement placement in placements )
            {
                if ( placement.Position.X < min.X )
                    min.X = placement.Position.X;
                if ( placement.Position.Z < min.Y )
                    min.Y = placement.Position.Z;

                if ( placement.Position.X > max.X )
                    max.X = placement.Position.X;
                if ( placement.Position.Z > max.Y )
                    max.Y = placement.Position.Z;
            }

            myBounds.X = min.X;
            myBounds.Y = min.Y;
            myBounds.Z = max.X;
            myBounds.W = max.Y;

            myGridWidth = (int) Math.Ceiling( ( max.X - min.X ) / BlockSize );
            myGridDepth = (int) Math.Ceiling( ( max.Y - min.Y ) / BlockSize );

            myInstGrid = new ExteriorBlock[ myGridWidth, myGridDepth ];
            for ( int x = 0; x < myGridWidth; ++x ) for ( int z = 0; z < myGridDepth; ++z )
                myInstGrid[ x, z ] = new ExteriorBlock( GetBlockPos( x, z ) );

            foreach ( InstPlacement placement in placements )
            {
                int x = GetBlockX( placement.Position.X );
                int z = GetBlockZ( placement.Position.Z );
                myInstGrid[ x, z ].AddPlacement( placement );
            }

            foreach ( ExteriorBlock block in myInstGrid )
                block.FinalisePlacements();
        }

        public override ICollection<Instance> GetInstances()
        {
            List<Instance> instances = new List<Instance>();

            foreach ( ExteriorBlock block in myInstGrid )
                instances.AddRange( block.GetInstances() );

            return instances;
        }

        public override void Render( ModelShader shader )
        {
            float rx, rz;
            for ( int x = 0; x < myGridWidth; ++x ) for ( int z = 0; z < myGridDepth; ++z )
            {
                ExteriorBlock blk = myInstGrid[ x, z ];

                rx = blk.CenterPos.X - shader.CameraPosition.X;
                rz = blk.CenterPos.Y - shader.CameraPosition.Z;

                if( ( rx * rx + rz * rz ) < shader.ViewRange2 + blk.Radius2 )
                    blk.Render( shader );
            }
        }
    }
}
