﻿using System;
namespace MetalTensors.Layers
{
    public abstract class BinopLayer : Layer
    {
        public override int InputCount => 2;

        public override int[] GetOutputShape (params Tensor[] inputs)
        {
            return inputs[0].Shape;
        }
    }
}