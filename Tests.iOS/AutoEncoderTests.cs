﻿using System;
using System.IO;
using MetalPerformanceShaders;
using MetalTensors;
using MetalTensors.Applications;
using NUnit.Framework;

using static Tests.Imaging;

namespace Tests
{
    public class AutoEncodeTests
    {
        WeightsInit Glorot (int nIn, int nOut)
        {
            var scale = 1.0 / Math.Max (1.0, (nIn + nOut) / 2.0);
            var limit = 0.1*Math.Sqrt (3.0 * scale);
            Console.WriteLine ($"LIMIT {limit}");
            return WeightsInit.Uniform ((float)-limit, (float)limit);
        }

        Model MakeEncoder ()
        {
            var input = Tensor.InputImage ("image", 256, 256);
            var encoded =
                input
                .Conv (32, size: 4, stride: 2, weightsInit: Glorot(3,32))
                .LeakyReLU ()
                .Conv (64, size: 4, stride: 2, weightsInit: Glorot(32,64))
                .LeakyReLU ()
                .Conv (128, size: 4, stride: 2, weightsInit: Glorot(64,128))
                .LeakyReLU ()
                .Conv (256, size: 4, stride: 2, weightsInit: Glorot(128,256))
                .LeakyReLU ();
            return encoded.Model (input);
        }

        Model MakeDecoder ()
        {
            var input = Tensor.InputImage ("encoded", 16, 16, 256);
            var decoded =
                input
                //.ConvTranspose (128, size: 4, stride: 2)
                .Conv (128, size: 4, weightsInit: Glorot(256,128))
                .ReLU ()
                .Upsample ().Conv (128, size: 4, weightsInit: Glorot(128,128))
                //.BatchNorm ()
                .ReLU ()
                //.ConvTranspose (64, size: 4, stride: 2)
                .Upsample ().Conv (64, size: 4, weightsInit: Glorot(128,64))
                //.BatchNorm ()
                .ReLU ()
                //.ConvTranspose (32, size: 4, stride: 2)
                .Upsample ().Conv (32, size: 4, weightsInit: Glorot(64,32))
                //.BatchNorm ()
                .ReLU ()
                //.ConvTranspose (32, size: 4, stride: 2)
                .Upsample ().Conv (32, size: 4, weightsInit: Glorot(32,32))
                //.BatchNorm ()
                .ReLU ()
                .Conv (32, size: 4, weightsInit: Glorot(32,32))
                .ReLU ()
                .Conv (3, size: 4, weightsInit: Glorot(32,3))
                .Tanh ();
            return decoded.Model (input);
        }

        Model MakeAutoEncoder ()
        {
            var encoder = MakeEncoder ();
            var decoder = MakeDecoder ();
            var autoEncoder = decoder.Call (encoder);
            return autoEncoder;
        }

        //[Test]
        public void EncoderUntrained ()
        {
            var encoder = MakeEncoder ();
            var output = SaveModelJpeg (encoder, 0.5f, 0.5f);
            Assert.AreEqual (16, output.Shape[0]);
            Assert.AreEqual (16, output.Shape[1]);
            Assert.AreEqual (128, output.Shape[2]);
        }

        [Test]
        public void Train ()
        {
            var autoEncoder = MakeAutoEncoder ();
            autoEncoder.Compile (Loss.MeanAbsoluteError, 1e-3f);
            SaveModelJpeg (autoEncoder, 0.5f, 0.5f, "Untrained");
            var data = GetPix2pixDataSet ();
            var batchSize = 16;
            var batchesPerEpoch = data.Count / batchSize;
            var numEpochs = 5;
            var row = 0;
            for (var si = 0; si < numEpochs; si++) {
                for (var bi = 0; bi < batchesPerEpoch; bi++) {
                    var (ins, outs) = data.GetBatch (row, batchSize, autoEncoder.Device.Current ());
                    row = (row + batchSize) % data.Count;
                    var h = autoEncoder.Fit (ins, ins);
                    var aloss = h.AverageLoss;
                    Console.WriteLine ($"AUTOENCODER BATCH E{si+1} B{bi+1}/{batchesPerEpoch} LOSS {aloss}");
                    h.DisposeSourceImages ();
                    ins.Dispose ();
                    outs.Dispose ();
                }
                var output = SaveModelJpeg (autoEncoder, 0.5f, 0.5f, $"Trained{si+1}");
                Assert.AreEqual (256, output.Shape[0]);
                Assert.AreEqual (256, output.Shape[1]);
                Assert.AreEqual (3, output.Shape[2]);
                GC.Collect ();
                GC.WaitForPendingFinalizers ();
            }
        }
    }
}
