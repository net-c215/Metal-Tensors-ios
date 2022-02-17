﻿using System;
using System.IO;
using MetalTensors;
using MetalTensors.Applications;
using NUnit.Framework;

using static Tests.Imaging;

namespace Tests
{
    public class Pix2pixApplicationTests
    {
        [Test]
        public void DefaultShapes ()
        {
            var pix2pix = new Pix2pixApplication ();

            Assert.AreEqual (256, pix2pix.Generator.Input.Shape[0]);
            Assert.AreEqual (256, pix2pix.Generator.Input.Shape[1]);
            Assert.AreEqual (3, pix2pix.Generator.Input.Shape[2]);

            Assert.AreEqual (3, pix2pix.Discriminator.Output.Shape.Length);
            Assert.AreEqual (1, pix2pix.Discriminator.Output.Shape[^1]);

            Assert.NotNull (pix2pix.Gan);

            Assert.AreEqual (3, pix2pix.Gan.Output.Shape.Length);
            Assert.AreEqual (1, pix2pix.Gan.Output.Shape[^1]);
        }

        [Test]
        public void DataSetHasImages ()
        {
            var data = GetDataSet ();
            var image = data.GetPairedRow (0);
            image.SaveImage (JpegUrl ());
        }

        [Test]
        public void DataSetHasLeftAndRight ()
        {
            var data = GetDataSet ();
            var (inputs, outputs) = data.GetRow (0, MetalExtensions.Current(null));
            inputs[0].SaveImage (JpegUrl ("Pix2pixLeft"));
            outputs[0].SaveImage (JpegUrl ("Pix2pixRight"));
        }

        [Test]
        public void GeneratorOutputsImages ()
        {
            var pix2pix = new Pix2pixApplication ();

            var data = GetDataSet ();
            var (inputs, outputs) = data.GetRow (0, pix2pix.Device);

            var generated = pix2pix.Generator.Predict (inputs[0], pix2pix.Device);

            generated.SaveImage (JpegUrl ());
        }

        //[Test]
        public void Train ()
        {
            var pix2pix = new Pix2pixApplication ();

            var data = GetDataSet ();

            var (imageCount, trainTime, dataTime) = pix2pix.Train (data, batchSize: 16, epochs: 0.1f, progress: p => {
                Console.WriteLine ($"Pix2Pix {Math.Round (p * 100, 2)}%");
            });

            var trainImagesPerSecond = imageCount / (trainTime.TotalSeconds);
            var dataImagesPerSecond = imageCount / (dataTime.TotalSeconds);
            var totalImagesPerSecond = imageCount / (trainTime.TotalSeconds + dataTime.TotalSeconds);

            Console.WriteLine ($"{imageCount} images in {trainTime + dataTime}");
            Console.WriteLine ($"{trainImagesPerSecond} TrainImages/sec");
            Console.WriteLine ($"{dataImagesPerSecond} DataImages/sec");
            Console.WriteLine ($"{totalImagesPerSecond} Images/sec");
            Console.WriteLine ($"{TimeSpan.FromSeconds (data.Count / totalImagesPerSecond)}/epoch");
        }

        static Pix2pixApplication.Pix2pixDataSet GetDataSet ()
        {
            var userDir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
            var dataDir = Path.Combine (userDir, "Data", "datasets", "facades");
            var trainDataDir = Path.Combine (dataDir, "train");
            var data = Pix2pixApplication.Pix2pixDataSet.LoadDirectory (trainDataDir);
            return data;
        }
    }
}
