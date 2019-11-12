﻿using MetalPerformanceShaders;

namespace MetalTensors.Layers
{
    public class AddLayer : BinaryArithmeticLayer
    {
        protected override MPSNNFilterNode CreateFilterNode (MPSNNImageNode[] inputImageNodes)
        {
            return new MPSNNAdditionNode (inputImageNodes);
        }
    }
}
