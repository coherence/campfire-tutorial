// Copyright (c) coherence ApS.
// For all coherence generated code, the coherence SDK license terms apply. See the license file in the coherence Package root folder for more information.

// <auto-generated>
// Generated file. DO NOT EDIT!
// </auto-generated>
namespace Coherence.Generated
{
    using System;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using Coherence.ProtocolDef;
    using Coherence.Serializer;
    using Coherence.SimulationFrame;
    using Coherence.Entities;
    using Coherence.Utils;
    using Coherence.Brook;
    using Coherence.Core;
    using Logger = Coherence.Log.Logger;
    using UnityEngine;
    using Coherence.Toolkit;

    public struct _ec47cd7906b7749f4853524abf78e799_432812263773753349 : ICoherenceComponentData
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct Interop
        {
            [FieldOffset(0)]
            public ByteArray holdingForUUID;
            [FieldOffset(16)]
            public System.Byte isObjectPresent;
            [FieldOffset(17)]
            public ByteArray syncConfigId;
        }

        public static unsafe _ec47cd7906b7749f4853524abf78e799_432812263773753349 FromInterop(IntPtr data, Int32 dataSize, InteropAbsoluteSimulationFrame* simFrames, Int32 simFramesCount)
        {
            if (dataSize != 33) {
                throw new Exception($"Given data size is not equal to the struct size. ({dataSize} != 33) " +
                    "for component with ID 191");
            }

            if (simFramesCount != 0) {
                throw new Exception($"Given simFrames size is not equal to the expected length. ({simFramesCount} != 0) " +
                    "for component with ID 191");
            }

            var orig = new _ec47cd7906b7749f4853524abf78e799_432812263773753349();

            var comp = (Interop*)data;

            orig.holdingForUUID = comp->holdingForUUID.Data != null ? System.Text.Encoding.UTF8.GetString((byte*)comp->holdingForUUID.Data, comp->holdingForUUID.Length) : null;
            orig.isObjectPresent = comp->isObjectPresent != 0;
            orig.syncConfigId = comp->syncConfigId.Data != null ? System.Text.Encoding.UTF8.GetString((byte*)comp->syncConfigId.Data, comp->syncConfigId.Length) : null;

            return orig;
        }


        public static uint holdingForUUIDMask => 0b00000000000000000000000000000001;
        public AbsoluteSimulationFrame holdingForUUIDSimulationFrame;
        public System.String holdingForUUID;
        public static uint isObjectPresentMask => 0b00000000000000000000000000000010;
        public AbsoluteSimulationFrame isObjectPresentSimulationFrame;
        public System.Boolean isObjectPresent;
        public static uint syncConfigIdMask => 0b00000000000000000000000000000100;
        public AbsoluteSimulationFrame syncConfigIdSimulationFrame;
        public System.String syncConfigId;

        public uint FieldsMask { get; set; }
        public uint StoppedMask { get; set; }
        public uint GetComponentType() => 191;
        public int PriorityLevel() => 100;
        public const int order = 0;
        public uint InitialFieldsMask() => 0b00000000000000000000000000000111;
        public bool HasFields() => true;
        public bool HasRefFields() => false;


        public long[] GetSimulationFrames() {
            return null;
        }

        public int GetFieldCount() => 3;


        
        public HashSet<Entity> GetEntityRefs()
        {
            return default;
        }

        public uint ReplaceReferences(Entity fromEntity, Entity toEntity)
        {
            return 0;
        }
        
        public IEntityMapper.Error MapToAbsolute(IEntityMapper mapper)
        {
            return IEntityMapper.Error.None;
        }

        public IEntityMapper.Error MapToRelative(IEntityMapper mapper)
        {
            return IEntityMapper.Error.None;
        }

        public ICoherenceComponentData Clone() => this;
        public int GetComponentOrder() => order;
        public bool IsSendOrdered() => false;


        public AbsoluteSimulationFrame? GetMinSimulationFrame()
        {
            AbsoluteSimulationFrame? min = null;


            return min;
        }

        public ICoherenceComponentData MergeWith(ICoherenceComponentData data)
        {
            var other = (_ec47cd7906b7749f4853524abf78e799_432812263773753349)data;
            var otherMask = other.FieldsMask;

            FieldsMask |= otherMask;
            StoppedMask &= ~(otherMask);

            if ((otherMask & 0x01) != 0)
            {
                holdingForUUIDSimulationFrame = other.holdingForUUIDSimulationFrame;
                holdingForUUID = other.holdingForUUID;
            }

            otherMask >>= 1;
            if ((otherMask & 0x01) != 0)
            {
                isObjectPresentSimulationFrame = other.isObjectPresentSimulationFrame;
                isObjectPresent = other.isObjectPresent;
            }

            otherMask >>= 1;
            if ((otherMask & 0x01) != 0)
            {
                syncConfigIdSimulationFrame = other.syncConfigIdSimulationFrame;
                syncConfigId = other.syncConfigId;
            }

            otherMask >>= 1;
            StoppedMask |= other.StoppedMask;

            return this;
        }

        public uint DiffWith(ICoherenceComponentData data)
        {
            throw new System.NotSupportedException($"{nameof(DiffWith)} is not supported in Unity");
        }

        public static uint Serialize(_ec47cd7906b7749f4853524abf78e799_432812263773753349 data, bool isRefSimFrameValid, AbsoluteSimulationFrame referenceSimulationFrame, IOutProtocolBitStream bitStream, Logger logger)
        {
            if (bitStream.WriteMask(data.StoppedMask != 0))
            {
                bitStream.WriteMaskBits(data.StoppedMask, 3);
            }

            var mask = data.FieldsMask;

            if (bitStream.WriteMask((mask & 0x01) != 0))
            {


                var fieldValue = data.holdingForUUID;



                bitStream.WriteShortString(fieldValue);
            }

            mask >>= 1;
            if (bitStream.WriteMask((mask & 0x01) != 0))
            {


                var fieldValue = data.isObjectPresent;



                bitStream.WriteBool(fieldValue);
            }

            mask >>= 1;
            if (bitStream.WriteMask((mask & 0x01) != 0))
            {


                var fieldValue = data.syncConfigId;



                bitStream.WriteShortString(fieldValue);
            }

            mask >>= 1;

            return mask;
        }

        public static _ec47cd7906b7749f4853524abf78e799_432812263773753349 Deserialize(AbsoluteSimulationFrame referenceSimulationFrame, InProtocolBitStream bitStream)
        {
            var stoppedMask = (uint)0;
            if (bitStream.ReadMask())
            {
                stoppedMask = bitStream.ReadMaskBits(3);
            }

            var val = new _ec47cd7906b7749f4853524abf78e799_432812263773753349();
            if (bitStream.ReadMask())
            {

                val.holdingForUUID = bitStream.ReadShortString();
                val.FieldsMask |= holdingForUUIDMask;
            }
            if (bitStream.ReadMask())
            {

                val.isObjectPresent = bitStream.ReadBool();
                val.FieldsMask |= isObjectPresentMask;
            }
            if (bitStream.ReadMask())
            {

                val.syncConfigId = bitStream.ReadShortString();
                val.FieldsMask |= syncConfigIdMask;
            }

            val.StoppedMask = stoppedMask;

            return val;
        }


        public override string ToString()
        {
            return $"_ec47cd7906b7749f4853524abf78e799_432812263773753349(" +
                $" holdingForUUID: { holdingForUUID }" +
                $" isObjectPresent: { isObjectPresent }" +
                $" syncConfigId: { syncConfigId }" +
                $" Mask: { System.Convert.ToString(FieldsMask, 2).PadLeft(3, '0') }, " +
                $"Stopped: { System.Convert.ToString(StoppedMask, 2).PadLeft(3, '0') })";
        }
    }


}