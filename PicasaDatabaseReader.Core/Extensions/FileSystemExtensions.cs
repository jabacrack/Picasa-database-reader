using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Linq;

namespace PicasaDatabaseReader.Core.Extensions
{
    public static class FileSystemExtensions
    {
        public static IObservable<byte> ReadBytesObservable(IFileSystem fileSystem, string path, int bufferSize, long? maxRead = null, long? startAt = null)
        {
            long? leftCount = null;
            if (maxRead.HasValue)
            {
                leftCount = maxRead;
            }

            return Observable.Using(
                () => fileSystem.FileStream.Create(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true),
                stream => Observable.Create<byte[]>(async (observer, token) =>
                {
                    if (startAt.HasValue)
                    {
                        stream.Seek(startAt.Value, SeekOrigin.Begin);
                    }

                    int read;
                    do
                    {
                        token.ThrowIfCancellationRequested();

                        var currentBufferSize = leftCount.HasValue && leftCount.Value < int.MaxValue
                            ? Math.Min(bufferSize, (int) leftCount.Value)
                            : bufferSize;

                        var buffer = new byte[currentBufferSize];

                        read = await stream.ReadAsync(buffer, 0, currentBufferSize, token);

                        if(read > 0)
                        {
                            if (read < currentBufferSize)
                            {
                                buffer = buffer.Take(read).ToArray();
                            }

                            if (leftCount.HasValue)
                            {
                                leftCount -= read;
                            }

                            observer.OnNext(buffer);
                        }

                    } while (read != 0 && (!leftCount.HasValue || leftCount.Value > 0));
                })).SelectMany(bytes => bytes);
        }
    }
}