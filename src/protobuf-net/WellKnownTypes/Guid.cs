﻿using System;
using System.Runtime.InteropServices;

namespace ProtoBuf.WellKnownTypes
{
    partial class WellKnownSerializer : IProtoSerializer<Guid>
    {
        private static
#if !DEBUG
            readonly
#endif
            bool s_guidOptimized = VerifyGuidLayout();

        internal static bool GuidOptimized
        {
            get => s_guidOptimized;
#if DEBUG
            set => s_guidOptimized = value && VerifyGuidLayout();
#endif
        }

        private const int FieldGuidLow = 1, FieldGuidHigh = 2;
        Guid IProtoSerializer<Guid>.Deserialize(ProtoReader reader, ref ProtoReader.State state, Guid value)
        {
            ulong low = 0, high = 0;
            int fieldNumber;
            while ((fieldNumber = reader.ReadFieldHeader(ref state)) > 0)
            {
                switch (fieldNumber)
                {
                    case FieldGuidLow: low = reader.ReadUInt64(ref state); break;
                    case FieldGuidHigh: high = reader.ReadUInt64(ref state); break;
                    default: reader.SkipField(ref state); break;
                }
            }

            if (low == 0 & high == 0) return default;
            if (s_guidOptimized)
            {
                var acc = new GuidAccessor(low, high);
                return acc.Guid;
            }
            else
            {
                uint a = (uint)(low >> 32), b = (uint)low, c = (uint)(high >> 32), d = (uint)high;
                return new Guid((int)b, (short)a, (short)(a >> 16),
                    (byte)d, (byte)(d >> 8), (byte)(d >> 16), (byte)(d >> 24),
                    (byte)c, (byte)(c >> 8), (byte)(c >> 16), (byte)(c >> 24));
            }
        }

        void IProtoSerializer<Guid>.Serialize(ProtoWriter writer, ref ProtoWriter.State state, Guid value)
        {
            if (value == Guid.Empty) { }
            else if (s_guidOptimized)
            {
                var obj = new GuidAccessor(value);
                ProtoWriter.WriteFieldHeader(FieldGuidLow, WireType.Fixed64, writer, ref state);
                ProtoWriter.WriteUInt64(obj.Low, writer, ref state);
                ProtoWriter.WriteFieldHeader(FieldGuidHigh, WireType.Fixed64, writer, ref state);
                ProtoWriter.WriteUInt64(obj.High, writer, ref state);
            }
            else
            {
                byte[] blob = value.ToByteArray();
                ProtoWriter.WriteFieldHeader(FieldGuidLow, WireType.Fixed64, writer, ref state);
                ProtoWriter.WriteBytes(blob, 0, 8, writer, ref state);
                ProtoWriter.WriteFieldHeader(FieldGuidHigh, WireType.Fixed64, writer, ref state);
                ProtoWriter.WriteBytes(blob, 8, 8, writer, ref state);
            }
        }

        /// <summary>
        /// Provides access to the inner fields of a Guid.
        /// Similar to Guid.ToByteArray(), but faster and avoids the byte[] allocation
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private readonly struct GuidAccessor
        {
            [FieldOffset(0)]
            public readonly Guid Guid;

            [FieldOffset(0)]
            public readonly ulong Low;

            [FieldOffset(8)]
            public readonly ulong High;

            public GuidAccessor(Guid value)
            {
                Low = High = default;
                Guid = value;
            }

            public GuidAccessor(ulong low, ulong high)
            {
                Guid = default;
                Low = low;
                High = high;
            }
        }
        private static bool VerifyGuidLayout()
        {
            try
            {
                if (!Guid.TryParse("12345678-2345-3456-4567-56789a6789ab", out var guid))
                    return false;

                var obj = new GuidAccessor(guid);
                var low = obj.Low;
                var high = obj.High;

                // check it the fast way against our known sentinels
                if (low != 0x3456234512345678 | high != 0xAB89679A78566745) return false;

                // and do it "for real"
                var expected = guid.ToByteArray();
                for (int i = 0; i < 8; i++)
                {
                    if (expected[i] != (byte)(low >> (8 * i))) return false;
                }
                for (int i = 0; i < 8; i++)
                {
                    if (expected[i + 8] != (byte)(high >> (8 * i))) return false;
                }
                return true;
            }
            catch { }
            return false;
        }
    }
}
