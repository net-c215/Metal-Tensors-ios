﻿using System;

namespace MetalTensors.Tensors
{
    public class LabelsTensor : PlaceholderTensor
    {
        public LabelsTensor (string label, params int[] shape)
            : base (label, shape)
        {
        }
    }
}
