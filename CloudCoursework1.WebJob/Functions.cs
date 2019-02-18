using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using NAudio.Wave;
using NLayer.NAudioSupport;
using System;
using System.IO;

namespace CloudCoursework1.WebJob
{
    public class Functions
    {
        public static void GenerateSample(
            [QueueTrigger("soundqueue")] string message,
            [Blob("soundblob/sounds/{queueTrigger}")] CloudBlockBlob inputBlob,
            [Blob("soundblob/samples/{queueTrigger}")] CloudBlockBlob outputBlob,
            TextWriter log)
        {
            log.WriteLine("Generating sample for: " + message);

            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                const int seconds = 20;
                CreateSample(input, output, seconds);

                // Copy title from input to output
                string title = inputBlob.Metadata["Title"];
                outputBlob.Metadata["Title"] = title;
            }
        }

        private static void CreateSample(Stream input, Stream output, int duration)
        {
            using (Mp3FileReader reader = new Mp3FileReader(input, wave => new Mp3FrameDecompressor(wave)))
            {
                Mp3Frame frame = reader.ReadNextFrame();
                int frameTimeLength = (int)(frame.SampleCount / (double)frame.SampleRate * 1000.0);
                int framesRequired = (int)(duration / (double)frameTimeLength * 1000.0);

                int frameNumber = 0;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    frameNumber++;

                    if (frameNumber <= framesRequired)
                    {
                        output.Write(frame.RawData, 0, frame.RawData.Length);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}

