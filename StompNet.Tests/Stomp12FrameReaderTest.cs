using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using StompNet.Tests.Helpers;
using StompNet.IO;
using StompNet.Models.Frames;
using StompNet.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StompNet.Tests
{
    [TestClass]
    public class Stomp12FrameReaderTest
    {
        private readonly string[] LineSeparators = new[] { "\n", "\r\n" };
        private readonly string[] InputCommands = new[] {
            StompCommands.Connected, StompCommands.Message, StompCommands.Receipt, StompCommands.Error };

        [TestMethod]
        public void ValidHeartbeatFrame_SuccessfullyRead()
        {
            foreach (var lineSeparator in LineSeparators)
            {
                using (MemoryStream inStream = new MemoryStream())
                {
                    inStream.Write(lineSeparator);
                    inStream.Seek(0, SeekOrigin.Begin);

                    IStompFrameReader reader = new Stomp12FrameReader(inStream);
                    Frame outFrame = reader.ReadFrameAsync().Result;

                    Assert.AreEqual(StompCommands.Heartbeat, outFrame.Command);
                    Assert.IsTrue(!outFrame.Headers.Any());
                    Assert.IsTrue(!outFrame.Body.Any());
                }
            }
        }

        [TestMethod]
        public void FramesWithCommandOnly_SuccessfullyRead()
        {
            foreach (var lineSeparator in LineSeparators)
            {
                foreach (var inCmd in InputCommands)
                {
                    // Connected command does NOT support EOL as "\r\n"
                    if (inCmd == StompCommands.Connected && lineSeparator == "\r\n")
                    {
                        continue;
                    }

                    using (MemoryStream inStream = new MemoryStream())
                    {
                        inStream.Write(inCmd + lineSeparator + lineSeparator + "\0");
                        inStream.Seek(0, SeekOrigin.Begin);

                        IStompFrameReader reader = new Stomp12FrameReader(inStream);
                        Frame outFrame = reader.ReadFrameAsync().Result;

                        Assert.AreEqual(inCmd, outFrame.Command);
                        Assert.IsTrue(!outFrame.Headers.Any());
                        Assert.IsTrue(!outFrame.Body.Any());
                    }
                }
            }
        }

        [TestMethod]
        public void FrameWithHeadersAndContent_SuccessfullyRead()
        {
            string inFrame = StompCommands.Message + "\n" + "H0:V0\n HA :VA\nH1: V1 \nH2:V2_1\nH2:V2_2\n\n12345ABC\0";
            using (MemoryStream inStream = new MemoryStream())
            {
                inStream.Write(inFrame);
                inStream.Seek(0, SeekOrigin.Begin);

                IStompFrameReader reader = new Stomp12FrameReader(inStream);

                Frame outFrame = reader.ReadFrameAsync().Result;
                Assert.AreEqual(StompCommands.Message, outFrame.Command);
                Assert.AreEqual(5, outFrame.Headers.Count());
                Assert.AreEqual("V0", outFrame.GetAllHeaderValues("H0").First());
                Assert.AreEqual("VA", outFrame.GetAllHeaderValues(" HA ").First());
                Assert.AreEqual(" V1 ", outFrame.GetAllHeaderValues("H1").First());
                Assert.AreEqual(2, outFrame.GetAllHeaderValues("H2").Count());
                Assert.IsTrue(outFrame.GetAllHeaderValues("H2").Contains("V2_1"));
                Assert.IsTrue(outFrame.GetAllHeaderValues("H2").Contains("V2_1"));
                Assert.AreEqual("12345ABC", outFrame.GetBodyAsString());
            }
        }

        [TestMethod]
        public void FrameWithContentLengthHeader_BodySuccessfullyRead()
        {
            string inFrame1 = StompCommands.Message + "\n" + "content-length:0\n\n\0";
            using (MemoryStream inStream = new MemoryStream())
            {
                inStream.Write(inFrame1);
                inStream.Seek(0, SeekOrigin.Begin);

                IStompFrameReader reader = new Stomp12FrameReader(inStream);

                Frame outFrame = reader.ReadFrameAsync().Result;
                Assert.AreEqual(string.Empty, outFrame.GetBodyAsString());
            }

            string inFrame2 = StompCommands.Receipt + "\n" + "content-length:10\n\n1234567890\0";
            using (MemoryStream inStream = new MemoryStream())
            {
                inStream.Write(inFrame2);
                inStream.Seek(0, SeekOrigin.Begin);

                IStompFrameReader reader = new Stomp12FrameReader(inStream);

                Frame outFrame = reader.ReadFrameAsync().Result;
                Assert.AreEqual("1234567890", outFrame.GetBodyAsString());
            }

            string inFrame3 = StompCommands.Error + "\n" + "content-length: 4 \n\n\r\n\0\t\0";
            using (MemoryStream inStream = new MemoryStream())
            {
                inStream.Write(inFrame3);
                inStream.Seek(0, SeekOrigin.Begin);

                IStompFrameReader reader = new Stomp12FrameReader(inStream);

                Frame outFrame = reader.ReadFrameAsync().Result;
                Assert.AreEqual("\r\n\0\t", outFrame.GetBodyAsString());
            }
        }


        [TestMethod]
        public void FrameInChunks_SuccessfullyRead()
        {
            string inFrame = StompCommands.Message + "\r\nH0:V0\r\ncontent-length:20\r\n\r\n12345678901234567890\0";

            using (MemoryStream inStream = new NoEndChunkedMemoryStream(chunkSize: 4))
            {
                inStream.Write(inFrame);
                inStream.Seek(0, SeekOrigin.Begin);

                IStompFrameReader reader = new Stomp12FrameReader(inStream);
                Frame outFrame = reader.ReadFrameAsync().Result;
                Assert.AreEqual(StompCommands.Message, outFrame.Command);
                Assert.AreEqual(2, outFrame.Headers.Count());
                Assert.AreEqual("V0", outFrame.GetAllHeaderValues("H0").First());
                Assert.AreEqual("20", outFrame.GetAllHeaderValues("content-length").First());
                Assert.AreEqual("12345678901234567890", outFrame.GetBodyAsString());
            }
        }

        [TestMethod]
        public void CanceledWhileReading_TaskCanceledExceptionThrown()
        {
            string inFrame = StompCommands.Error + "\r\nH1:V1\nH2:V2\n\r\nABC\0";

            for (int i = 0; i < inFrame.Length; i++)
            {
                using (MemoryStream inStream = new NoEndChunkedMemoryStream())
                {
                    inStream.Write(inFrame.Substring(0, i));
                    inStream.Seek(0, SeekOrigin.Begin);

                    IStompFrameReader reader = new Stomp12FrameReader(inStream);

                    try
                    {
                        CancellationToken cToken = new CancellationTokenSource(10).Token;
                        Frame outFrame = reader.ReadFrameAsync(cToken).Result;
                        Assert.Fail("AggregateException(TaskCanceledException) expected.");
                    }
                    catch (AggregateException e)
                    {
                        Assert.IsTrue(e.InnerException is TaskCanceledException);
                    }
                }
            }

        }

        [TestMethod]
        public void EndOfStreamWhileReading_EndOfStreamExceptionThrown()
        {
            string inFrame = StompCommands.Error + "\r\nH1:V1\nH2:V2\n\r\nABC\0";

            for (int i = 0; i < inFrame.Length; i++)
            {
                using (MemoryStream inStream = new MemoryStream())
                {
                    inStream.Write(inFrame.Substring(0, i));
                    inStream.Seek(0, SeekOrigin.Begin);

                    IStompFrameReader reader = new Stomp12FrameReader(inStream);

                    try
                    {
                        Frame outFrame = reader.ReadFrameAsync().Result;
                        Assert.Fail("AggregateException(EndOfStreamException) expected.");
                    }
                    catch (AggregateException e)
                    {
                        Assert.IsTrue(e.InnerException is EndOfStreamException);
                    }
                }
            }

        }

        [TestMethod]
        public void InvalidFrames_InvalidDataExceptionsThrown()
        {
            string[] inputInvalidFrames =
            {
                StompCommands.Connected + "\n" + "Problem: ConnectedWithCarriageReturn\n\r\n" + "\0",
                StompCommands.Receipt + "\n" + "ProblemIsHeaderWithoutValue\n",
                StompCommands.Receipt + "\n" + ":ProblemIsHeaderWithoutName\n",
                StompCommands.Error + "\n" + "Problem: Value:WithSemicolon\n",
                StompCommands.Error + "\n" + "Problem: Value\rWithCarriageReturn\n",
                StompCommands.Message + "\n" + "Problem: NonNullTerminated\ncontent-length: 0\n\n1",
                StompCommands.Message + "\n" + "Problem: NonNullTerminated\ncontent-length: 1\n\n12",
            };

            foreach (var inFrame in inputInvalidFrames)
            {
                using (MemoryStream inStream = new MemoryStream())
                {
                    inStream.Write(StompCommands.Connected + "\r\n" + "\r\n" + "\0");
                    inStream.Seek(0, SeekOrigin.Begin);

                    IStompFrameReader reader = new Stomp12FrameReader(inStream);

                    try
                    {
                        Frame outFrame = reader.ReadFrameAsync().Result;
                        Assert.Fail("AggregateException(InvalidDataException) expected.");
                    }
                    catch (AggregateException e)
                    {
                        Assert.IsTrue(e.InnerException is InvalidDataException);
                    }
                }
            }
        }
    }
}
