// Copyright (c) coherence ApS.
// For all coherence generated code, the coherence SDK license terms apply. See the license file in the coherence Package root folder for more information.

// <auto-generated>
// Generated file. DO NOT EDIT!
// </auto-generated>
namespace Coherence.Generated
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Coherence.Toolkit;
    using Coherence.Toolkit.Bindings;
    using Coherence.Entities;
    using Coherence.ProtocolDef;
    using Coherence.Brook;
    using Coherence.Toolkit.Bindings.ValueBindings;
    using Coherence.Toolkit.Bindings.TransformBindings;
    using Coherence.Connection;
    using Coherence.SimulationFrame;
    using Coherence.Interpolation;
    using Coherence.Log;
    using Logger = Coherence.Log.Logger;
    using UnityEngine.Scripting;
    
    [UnityEngine.Scripting.Preserve]
    public class Binding_ec47cd7906b7749f4853524abf78e799_060eae84e0a5489aa3f32d7c800cf950 : PositionBinding
    {   
        private global::UnityEngine.Transform CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::UnityEngine.Transform)UnityComponent;
        }

        public override string CoherenceComponentName => "WorldPosition";
        public override uint FieldMask => 0b00000000000000000000000000000001;

        public override UnityEngine.Vector3 Value
        {
            get { return (UnityEngine.Vector3)(coherenceSync.coherencePosition); }
            set { coherenceSync.coherencePosition = (UnityEngine.Vector3)(value); }
        }

        protected override (UnityEngine.Vector3 value, AbsoluteSimulationFrame simFrame) ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((WorldPosition)coherenceComponent).value;
            if (!coherenceSync.HasParentWithCoherenceSync) { value += floatingOriginDelta; }

            var simFrame = ((WorldPosition)coherenceComponent).valueSimulationFrame;
            
            return (value, simFrame);
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, AbsoluteSimulationFrame simFrame)
        {
            var update = (WorldPosition)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.value = Value;
            }
            else
            {
                update.value = GetInterpolatedAt(simFrame / InterpolationSettings.SimulationFramesPerSecond);
            }

            update.valueSimulationFrame = simFrame;
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new WorldPosition();
        }    
    }
    
    [UnityEngine.Scripting.Preserve]
    public class Binding_ec47cd7906b7749f4853524abf78e799_750f70418bae4108b1e74427fb548aae : RotationBinding
    {   
        private global::UnityEngine.Transform CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::UnityEngine.Transform)UnityComponent;
        }

        public override string CoherenceComponentName => "WorldOrientation";
        public override uint FieldMask => 0b00000000000000000000000000000001;

        public override UnityEngine.Quaternion Value
        {
            get { return (UnityEngine.Quaternion)(coherenceSync.coherenceRotation); }
            set { coherenceSync.coherenceRotation = (UnityEngine.Quaternion)(value); }
        }

        protected override (UnityEngine.Quaternion value, AbsoluteSimulationFrame simFrame) ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((WorldOrientation)coherenceComponent).value;

            var simFrame = ((WorldOrientation)coherenceComponent).valueSimulationFrame;
            
            return (value, simFrame);
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, AbsoluteSimulationFrame simFrame)
        {
            var update = (WorldOrientation)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.value = Value;
            }
            else
            {
                update.value = GetInterpolatedAt(simFrame / InterpolationSettings.SimulationFramesPerSecond);
            }

            update.valueSimulationFrame = simFrame;
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new WorldOrientation();
        }    
    }
    
    [UnityEngine.Scripting.Preserve]
    public class Binding_ec47cd7906b7749f4853524abf78e799_189625fefb07446aac7880b2b0dbf320 : StringBinding
    {   
        private global::ObjectAnchor CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::ObjectAnchor)UnityComponent;
        }

        public override string CoherenceComponentName => "_ec47cd7906b7749f4853524abf78e799_432812263773753349";
        public override uint FieldMask => 0b00000000000000000000000000000001;

        public override System.String Value
        {
            get { return (System.String)(CastedUnityComponent.holdingForUUID); }
            set { CastedUnityComponent.holdingForUUID = (System.String)(value); }
        }

        protected override (System.String value, AbsoluteSimulationFrame simFrame) ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent).holdingForUUID;

            var simFrame = ((_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent).holdingForUUIDSimulationFrame;
            
            return (value, simFrame);
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, AbsoluteSimulationFrame simFrame)
        {
            var update = (_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.holdingForUUID = Value;
            }
            else
            {
                update.holdingForUUID = GetInterpolatedAt(simFrame / InterpolationSettings.SimulationFramesPerSecond);
            }

            update.holdingForUUIDSimulationFrame = simFrame;
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new _ec47cd7906b7749f4853524abf78e799_432812263773753349();
        }    
    }
    
    [UnityEngine.Scripting.Preserve]
    public class Binding_ec47cd7906b7749f4853524abf78e799_120b91f3d4bd4bc3ae37cce0009abd40 : BoolBinding
    {   
        private global::ObjectAnchor CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::ObjectAnchor)UnityComponent;
        }

        public override string CoherenceComponentName => "_ec47cd7906b7749f4853524abf78e799_432812263773753349";
        public override uint FieldMask => 0b00000000000000000000000000000010;

        public override System.Boolean Value
        {
            get { return (System.Boolean)(CastedUnityComponent.isObjectPresent); }
            set { CastedUnityComponent.isObjectPresent = (System.Boolean)(value); }
        }

        protected override (System.Boolean value, AbsoluteSimulationFrame simFrame) ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent).isObjectPresent;

            var simFrame = ((_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent).isObjectPresentSimulationFrame;
            
            return (value, simFrame);
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, AbsoluteSimulationFrame simFrame)
        {
            var update = (_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.isObjectPresent = Value;
            }
            else
            {
                update.isObjectPresent = GetInterpolatedAt(simFrame / InterpolationSettings.SimulationFramesPerSecond);
            }

            update.isObjectPresentSimulationFrame = simFrame;
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new _ec47cd7906b7749f4853524abf78e799_432812263773753349();
        }    
    }
    
    [UnityEngine.Scripting.Preserve]
    public class Binding_ec47cd7906b7749f4853524abf78e799_74c49d2eed9146be872d431e635e3816 : StringBinding
    {   
        private global::ObjectAnchor CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::ObjectAnchor)UnityComponent;
        }

        public override string CoherenceComponentName => "_ec47cd7906b7749f4853524abf78e799_432812263773753349";
        public override uint FieldMask => 0b00000000000000000000000000000100;

        public override System.String Value
        {
            get { return (System.String)(CastedUnityComponent.syncConfigId); }
            set { CastedUnityComponent.syncConfigId = (System.String)(value); }
        }

        protected override (System.String value, AbsoluteSimulationFrame simFrame) ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent).syncConfigId;

            var simFrame = ((_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent).syncConfigIdSimulationFrame;
            
            return (value, simFrame);
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, AbsoluteSimulationFrame simFrame)
        {
            var update = (_ec47cd7906b7749f4853524abf78e799_432812263773753349)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.syncConfigId = Value;
            }
            else
            {
                update.syncConfigId = GetInterpolatedAt(simFrame / InterpolationSettings.SimulationFramesPerSecond);
            }

            update.syncConfigIdSimulationFrame = simFrame;
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new _ec47cd7906b7749f4853524abf78e799_432812263773753349();
        }    
    }

    [UnityEngine.Scripting.Preserve]
    public class CoherenceSync_ec47cd7906b7749f4853524abf78e799 : CoherenceSyncBaked
    {
        private Entity entityId;
        private Logger logger = Coherence.Log.Log.GetLogger<CoherenceSync_ec47cd7906b7749f4853524abf78e799>();
        
        private global::ObjectAnchor _ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d_CommandTarget;
        
        
        private IClient client;
        private CoherenceBridge bridge;
        
        private readonly Dictionary<string, Binding> bakedValueBindings = new Dictionary<string, Binding>()
        {
            ["060eae84e0a5489aa3f32d7c800cf950"] = new Binding_ec47cd7906b7749f4853524abf78e799_060eae84e0a5489aa3f32d7c800cf950(),
            ["750f70418bae4108b1e74427fb548aae"] = new Binding_ec47cd7906b7749f4853524abf78e799_750f70418bae4108b1e74427fb548aae(),
            ["189625fefb07446aac7880b2b0dbf320"] = new Binding_ec47cd7906b7749f4853524abf78e799_189625fefb07446aac7880b2b0dbf320(),
            ["120b91f3d4bd4bc3ae37cce0009abd40"] = new Binding_ec47cd7906b7749f4853524abf78e799_120b91f3d4bd4bc3ae37cce0009abd40(),
            ["74c49d2eed9146be872d431e635e3816"] = new Binding_ec47cd7906b7749f4853524abf78e799_74c49d2eed9146be872d431e635e3816(),
        };
        
        private Dictionary<string, Action<CommandBinding, CommandsHandler>> bakedCommandBindings = new Dictionary<string, Action<CommandBinding, CommandsHandler>>();
        
        public CoherenceSync_ec47cd7906b7749f4853524abf78e799()
        {
            bakedCommandBindings.Add("d9e25df655c0450f92da43abe92a930d", BakeCommandBinding__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d);
        }
        
        public override Binding BakeValueBinding(Binding valueBinding)
        {
            if (bakedValueBindings.TryGetValue(valueBinding.guid, out var bakedBinding))
            {
                valueBinding.CloneTo(bakedBinding);
                return bakedBinding;
            }
            
            return null;
        }
        
        public override void BakeCommandBinding(CommandBinding commandBinding, CommandsHandler commandsHandler)
        {
            if (bakedCommandBindings.TryGetValue(commandBinding.guid, out var commandBindingBaker))
            {
                commandBindingBaker.Invoke(commandBinding, commandsHandler);
            }
        }
    
        private void BakeCommandBinding__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d(CommandBinding commandBinding, CommandsHandler commandsHandler)
        {
            _ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d_CommandTarget = (global::ObjectAnchor)commandBinding.UnityComponent;
            commandsHandler.AddBakedCommand("ObjectAnchor.ChangeLinkedObjectStateAuth", "(System.Boolean)", SendCommand__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d, ReceiveLocalCommand__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d, MessageTarget.AuthorityOnly, _ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d_CommandTarget, false);
        }
        
        private void SendCommand__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d(MessageTarget target, object[] args)
        {
            var command = new _ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d();
            
            int i = 0;
            command.newState = (System.Boolean)args[i++];
        
            client.SendCommand(command, target, entityId);
        }
        
        private void ReceiveLocalCommand__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d(MessageTarget target, object[] args)
        {
            var command = new _ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d();
            
            int i = 0;
            command.newState = (System.Boolean)args[i++];
            
            ReceiveCommand__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d(command);
        }

        private void ReceiveCommand__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d(_ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d command)
        {
            var target = _ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d_CommandTarget;
            
            target.ChangeLinkedObjectStateAuth((System.Boolean)(command.newState));
        }
        
        public override void ReceiveCommand(IEntityCommand command)
        {
            switch (command)
            {
                case _ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d castedCommand:
                    ReceiveCommand__ec47cd7906b7749f4853524abf78e799_d9e25df655c0450f92da43abe92a930d(castedCommand);
                    break;
                default:
                    logger.Warning($"CoherenceSync_ec47cd7906b7749f4853524abf78e799 Unhandled command: {command.GetType()}.");
                    break;
            }
        }
        
        public override List<ICoherenceComponentData> CreateEntity(bool usesLodsAtRuntime, string archetypeName, AbsoluteSimulationFrame simFrame)
        {
            if (!usesLodsAtRuntime)
            {
                return null;
            }
            
            if (Archetypes.IndexForName.TryGetValue(archetypeName, out int archetypeIndex))
            {
                var components = new List<ICoherenceComponentData>()
                {
                    new ArchetypeComponent
                    {
                        index = archetypeIndex,
                        indexSimulationFrame = simFrame,
                        FieldsMask = 0b1
                    }
                };

                return components;
            }
    
            logger.Warning($"Unable to find archetype {archetypeName} in dictionary. Please, bake manually (coherence > Bake)");
            
            return null;
        }
        
        public override void Dispose()
        {
        }
        
        public override void Initialize(Entity entityId, CoherenceBridge bridge, IClient client, CoherenceInput input, Logger logger)
        {
            this.logger = logger.With<CoherenceSync_ec47cd7906b7749f4853524abf78e799>();
            this.bridge = bridge;
            this.entityId = entityId;
            this.client = client;        
        }
    }

}