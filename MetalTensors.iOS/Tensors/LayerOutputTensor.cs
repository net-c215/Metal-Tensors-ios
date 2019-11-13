﻿using System;
using Metal;
using MetalPerformanceShaders;

namespace MetalTensors.Tensors
{
    public class LayerOutputTensor : Tensor
    {
        public override int[] Shape => Layer.GetOutputShape (LayerInputs);

        public Layer Layer { get; }
        public Tensor[] LayerInputs { get; }

        public LayerOutputTensor (Layer layer, params Tensor[] layerInputs)
        {
            Layer = layer;
            LayerInputs = layerInputs;
        }

        public override void Copy (Span<float> destination)
        {
            var device = MetalExtensions.Current (null);
            var computed = Layer.ExecuteAsync (LayerInputs, device).Result;
            computed.Copy (destination);
        }

        public override MPSNNImageNode GetMetalImageNode (IMTLDevice device)
        {
            return Layer.GetMetalImageNode (LayerInputs, device);
        }
    }
}
