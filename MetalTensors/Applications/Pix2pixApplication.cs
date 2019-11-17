﻿using System;

namespace MetalTensors.Applications
{
    public class Pix2pixApplication : GanApplication
    {
        public Pix2pixApplication (int height = 256, int width = 256, int featureChannels = 3, int outputFeatureChannels = 3)
            : base (MakeGenerator (height, width, featureChannels, outputFeatureChannels),
                    MakeDiscriminator (height, width, outputFeatureChannels))
        {
        }

        private static Model MakeGenerator (int height, int width, int featureChannels, int outputFeatureChannels)
        {
            var x = Tensor.InputImage ("image", height, width, featureChannels);

            return x.Model ();
        }

        private static Model MakeDiscriminator (int height, int width, int outputFeatureChannels)
        {
            var x = Tensor.InputImage ("image", height, width, outputFeatureChannels);

            x = CFirstD (64, x);
            x = C (128, x);
            x = C (256, x);
            x = C (512, x);

            // Need to average to get 1x1x1

            return x.Model ();
        }

        static Tensor C (int channels, Tensor input)
        {
            return input.Conv (channels, stride: 2).ReLU ();
        }

        static Tensor CFirstD (int channels, Tensor input)
        {
            return input.Conv (channels, stride: 2).ReLU ();
        }
    }
}
