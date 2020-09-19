using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StompNet.Tests.Helpers
{
    /// <summary>
    /// MemoryStream that simulates a 'live' stream (e.g. NetworkStream) which does not signal end of stream. When
    /// the end of stream has been reached and ReadAsync(byte[], int, int, CancellationToken) is called, it waits for
    /// the cancellation token to be canceled.
    /// 
    /// It also supports providing a chunk size (in constructor) to simulate reading in chunks when using
    /// ReadAsync(byte[], int, int, CancellationToken).
    /// </summary>
    internal class NoEndChunkedMemoryStream : MemoryStream
    {
        private readonly int _chunkSize;

        public NoEndChunkedMemoryStream(int chunkSize = 0)
        {
            _chunkSize = chunkSize;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            int chunkSize = _chunkSize > 0 ? Math.Min(_chunkSize, count) : count;
            int bytesRead = await base.ReadAsync(buffer, offset, chunkSize, cancellationToken);
            if (bytesRead == 0)
            {
                cancellationToken.WaitHandle.WaitOne();
                cancellationToken.ThrowIfCancellationRequested();
            }
            return bytesRead;
        }
    }
}