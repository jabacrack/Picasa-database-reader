using System;
using System.IO.Abstractions.TestingHelpers;
using System.Reactive.Linq;
using System.Threading;
using FluentAssertions;
using PicasaDatabaseReader.Core.Extensions;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests.Extensions
{
    public class FileSystemExtensionTests : UnitTestsBase<FileSystemExtensionTests>
    {
        public FileSystemExtensionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public void ShouldReadAllBytes()
        {
            var path = "c:\\blah.txt";

            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(path, new MockFileData(input));

            var autoResetEvent = new AutoResetEvent(false);

            byte[] result = null;
            FileSystemExtensions.ReadBytesObservable(mockFileSystem, path, 1024)
                .ToArray()
                .Subscribe(
                    values =>
                    {
                        result = values;
                    },
                    () => autoResetEvent.Set());

            autoResetEvent.WaitOne();

            result.Length.Should().Be(10);
            result.Should().BeEquivalentTo(input);
        }

        [Fact]
        public void ShouldReadSomeBytes()
        {
            var path = "c:\\blah.txt";

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(path, new MockFileData(new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}));

            var autoResetEvent = new AutoResetEvent(false);

            byte[] result = null;
            FileSystemExtensions.ReadBytesObservable(mockFileSystem, path, 1, 3)
                .ToArray()
                .Subscribe(
                    values => result = values,
                    () => autoResetEvent.Set());

            autoResetEvent.WaitOne();

            result.Length.Should().Be(3);
            result.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void ShouldReadSomeBytesStartingAtPoint()
        {
            var path = "c:\\blah.txt";

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(path, new MockFileData(new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}));

            var autoResetEvent = new AutoResetEvent(false);

            byte[] result = null;
            FileSystemExtensions.ReadBytesObservable(mockFileSystem, path, 1, 3, 2)
                .ToArray()
                .Subscribe(
                    values => result = values,
                    () => autoResetEvent.Set());

            autoResetEvent.WaitOne();

            result.Length.Should().Be(3);
            result.Should().BeEquivalentTo(3, 4, 5);
        }
    }
}