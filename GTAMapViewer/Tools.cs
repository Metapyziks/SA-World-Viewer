using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;

namespace GTAMapViewer
{
    internal static class Tools
    {
        public static bool DoesExtend( this Type self, Type type )
        {
            return self.BaseType == type || ( self.BaseType != null && self.BaseType.DoesExtend( type ) );
        }

        public static byte[] ReadBytes( this Stream self, int count )
        {
            byte[] data = new byte[ count ];
            for ( int i = 0; i < count; ++i )
            {
                int bt = self.ReadByte();
                if ( bt == -1 )
                    throw new EndOfStreamException();

                data[ i ] = (byte) bt;
            }

            return data;
        }

        #region Clamps
        public static Byte Clamp( Byte value, Byte min, Byte max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static UInt16 Clamp( UInt16 value, UInt16 min, UInt16 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static UInt32 Clamp( UInt32 value, UInt32 min, UInt32 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static UInt64 Clamp( UInt64 value, UInt64 min, UInt64 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static SByte Clamp( SByte value, SByte min, SByte max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Int16 Clamp( Int16 value, Int16 min, Int16 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Int32 Clamp( Int32 value, Int32 min, Int32 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Int64 Clamp( Int64 value, Int64 min, Int64 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Single Clamp( Single value, Single min, Single max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Double Clamp( Double value, Double min, Double max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }
        #endregion Clamps

        #region MinMax
        public static int Min( params int[] values )
        {
            int min = values[ 0 ];
            foreach ( int val in values )
                if ( val < min )
                    min = val;

            return min;
        }

        public static double Min( params double[] values )
        {
            double min = values[ 0 ];
            foreach ( double val in values )
                if ( val < min )
                    min = val;

            return min;
        }

        public static int Max( params int[] values )
        {
            int max = values[ 0 ];
            foreach ( int val in values )
                if ( val > max )
                    max = val;

            return max;
        }

        public static double Max( params double[] values )
        {
            double max = values[ 0 ];
            foreach ( double val in values )
                if ( val > max )
                    max = val;

            return max;
        }
        #endregion MinMax

        public static int FloorDiv( int numer, int denom )
        {
            return ( numer / denom ) - ( numer < 0 && ( numer % denom ) != 0 ? 1 : 0 );
        }

        public static String ApplyWordWrap( this String text, float charWidth, float wrapWidth )
        {
            if ( wrapWidth <= 0.0f )
                return text;

            String newText = "";
            int charsPerLine = (int) ( wrapWidth / charWidth );
            int x = 0, i = 0;
            while ( i < text.Length )
            {
                String word = "";
                while ( i < text.Length && !char.IsWhiteSpace( text[ i ] ) )
                    word += text[ i++ ];

                if ( x + word.Length > charsPerLine )
                {
                    if ( x == 0 )
                    {
                        newText += word.Substring( 0, charsPerLine ) + "\n" + word.Substring( charsPerLine );
                        x = word.Length - charsPerLine;
                    }
                    else
                    {
                        newText += "\n" + word;
                        x = word.Length;
                    }
                }
                else
                {
                    newText += word;
                    x += word.Length;
                }

                if ( i < text.Length )
                {
                    newText += text[ i ];
                    x++;

                    if ( text[ i++ ] == '\n' )
                        x = 0;
                }
            }

            return newText;
        }

        public static int QuickLog2( int value )
        {
            int i = 0;
            while ( ( value >>= 1 ) != 0 )
                ++i;

            return i;
        }

        public static float WrapAngle( float ang )
        {
            return ang - (float) Math.Floor( ang / ( MathHelper.TwoPi ) + 0.5f ) * MathHelper.TwoPi;
        }

        public static float WrapAngle( float ang, float basis )
        {
            return WrapAngle( ang - basis ) + basis;
        }

        public static float AngleDif( float angA, float angB )
        {
            return WrapAngle( angA - angB );
        }

        public static String NextTexture( this Random rand, String prefix, int max )
        {
            if ( !prefix.EndsWith( "_" ) )
                prefix += "_";

            return prefix + rand.Next( max ).ToString( "X" ).ToLower();
        }

        public static String NextTexture( this Random rand, String prefix, int min, int max )
        {
            if ( !prefix.EndsWith( "_" ) )
                prefix += "_";

            return prefix + rand.Next( min, max ).ToString( "X" ).ToLower();
        }

        internal static Vector2 ReadVector2( this BinaryReader reader )
        {
            return new Vector2
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle()
            };
        }

        internal static Vector3 ReadVector3( this BinaryReader reader )
        {
            return new Vector3
            {
                X = -reader.ReadSingle(),
                Z = reader.ReadSingle(),
                Y = reader.ReadSingle()
            };
        }

        internal static String ReadString( this BinaryReader reader, int length )
        {
            byte[] bytes = reader.ReadBytes( length );
            return Encoding.UTF8.GetString( bytes ).TrimNullChars();
        }

        internal static String TrimNullChars( this String str )
        {
            for ( int i = 0; i < str.Length; ++i )
                if ( str[ i ] == '\0' )
                    return str.Substring( 0, i );

            return str;
        }
    }
}
