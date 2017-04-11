using System;
using System.Threading;
using System.Threading.Tasks;
using StompNet.Helpers;
using StompNet.Models;
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// Observer to manage hearbeat
    /// </summary>
    public class StompHeartbeatManager : IDisposable
    {

        private IStompClient client;
        private IStompFrameObservable frameObservable;

        private CancellationTokenSource keepAliveToken;
        private CancellationTokenSource sendHBToken;
        private bool shouldStartSendingHeartbeat;

        /// <summary>
        /// Gets the last activity time (received message)
        /// </summary>
        public DateTime LastActivityTime { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets margin of acceptance to send and receive heartbeat
        /// because of timing inaccuracies, the receiver SHOULD be tolerant and take into account an error margin
        /// </summary>
        public static int MarginOfAcceptance { get; set; } = 1000;

        /// <summary>
        /// Create an observer to <paramref name="frameObservable"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="frameObservable"></param>
        public StompHeartbeatManager(IStompClient client, IStompFrameObservable frameObservable)
        {
            this.client = client;
            this.frameObservable = frameObservable;

            if (this.frameObservable == null)
                throw new ArgumentNullException("frameObservable");

            this.frameObservable.SubscribeEx(this.OnNext);
        }

        private void OnNext(Frame frame)
        {
            if (frame.Command == StompCommands.Connected)
            {
                var connectedFrame = StompInterpreter.Interpret(frame) as ConnectedFrame;
                if (connectedFrame.Heartbeat.Incoming > 0)
                {
                    CheckKeepAlive(connectedFrame.Heartbeat.Incoming + MarginOfAcceptance);
                }

                if (connectedFrame.Heartbeat.Outgoing > 0)
                {
                    SendHeartBeat((int)(Math.Max(connectedFrame.Heartbeat.Outgoing - MarginOfAcceptance, MarginOfAcceptance)));
                }
            }

            LastActivityTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if expected server heartbeat is received
        /// </summary>
        /// <param name="interval"></param>
        private async void CheckKeepAlive(int interval)
        {
            keepAliveToken = new CancellationTokenSource();
            while (!keepAliveToken.IsCancellationRequested)
            {
                var timeToWait = interval - (DateTime.UtcNow - LastActivityTime).TotalMilliseconds;
                if (timeToWait < 0)
                {
                    client.Dispose();
                }
                else
                {
                    await Task.Delay((int)timeToWait, keepAliveToken.Token).ContinueWith(t => { });
                }
            }
        }

        /// <summary>
        /// Sends heartbeat periodically
        /// </summary>
        /// <param name="interval"></param>
        private async void SendHeartBeat(int interval)
        {
            sendHBToken = new CancellationTokenSource();
            while (!sendHBToken.IsCancellationRequested)
            {
                if (shouldStartSendingHeartbeat)
                {
                    await client.WriteHearbeatAsync();
                }
                else
                {
                    shouldStartSendingHeartbeat = true;
                }
                await Task.Delay((int)Math.Max(0, interval - (DateTime.UtcNow - LastActivityTime).TotalMilliseconds), sendHBToken.Token)
                    .ContinueWith(t => { }); ;
            }
        }

        public void Dispose()
        {
            if (keepAliveToken != null && !keepAliveToken.IsCancellationRequested)
            {
                keepAliveToken.Cancel();
            }
            
            if (sendHBToken != null && !sendHBToken.IsCancellationRequested)
            {
                sendHBToken.Cancel();
            }           
        }
    }
}
