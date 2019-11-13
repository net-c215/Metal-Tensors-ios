﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Foundation;
using Metal;
using MetalPerformanceShaders;

namespace MetalTensors.Tensors
{
    public class ConstantTensor : Tensor
    {
        readonly int[] shape;

        public override int[] Shape => shape;

        public float ConstantValue { get; }

        readonly ConcurrentDictionary<IntPtr, MPSImage> deviceImages = new ConcurrentDictionary<IntPtr, MPSImage> ();

        public ConstantTensor (float constant, params int[] shape)
        {
            ConstantValue = constant;
            ValidateShape (shape);
            this.shape = shape;
        }

        public override void Copy (Span<float> destination)
        {
            var n = ValidateCopyDestination (destination);
            var c = ConstantValue;
            for (var i = 0; i < n; i++) {
                destination[i] = c;
            }
        }

        public override MPSImage GetMetalImage (IMTLDevice device)
        {
            var key = device.Handle;
            if (deviceImages.TryGetValue (key, out var image))
                return image;
            image = CreateConstantImage (Shape, ConstantValue);
            if (deviceImages.TryAdd (key, image))
                return image;
            return deviceImages[key];
        }
    }
}
