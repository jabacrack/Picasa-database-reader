using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PicasaDatabaseReader.Core.Scheduling;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace PicasaDatabaseReader.Core.Tests
{
    public class DatabaseReaderTests : UnitTestsBase<DatabaseReaderTests>
    {
        protected internal readonly TestScheduleProvider TestScheduleProvider = new TestScheduleProvider();

        public DatabaseReaderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ShouldGetTableNames()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Distinct()
                .Select(name =>
                {
                    var filename = $"{name}_0";
                    var path = Path.Combine(directoryPath, filename);
                    return new { name, filename, path };
                })
                .ToArray();

            var mockFileData = new MockFileData(BitConverter.GetBytes(DatabaseReader.TableFileHeader));
            var mockFiles = args.ToDictionary(arg => arg.path, _ => mockFileData);

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = this.CreateDatabaseReader(mockFileSystem, directoryPath, TestScheduleProvider);

            var tableNames = databaseReader
                .GetTableNames();

            var autoResetEvent = new AutoResetEvent(false);

            tableNames.Subscribe(_ => { }, () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }

        [Fact]
        public async Task ShouldGetFieldFiles()
        {
            Logger.LogInformation("ShouldGetFieldFiles");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Distinct()
                .Select(name =>
                {
                    var filename = $"TestTable_{name}.pmp";
                    var path = Path.Combine(directoryPath, filename);
                    return new { name, filename, path };
                })
                .ToArray();

            var mockFileData = new MockFileData(string.Empty);
            var mockFiles = args.ToDictionary(arg => arg.path, _ => mockFileData);

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = this.CreateDatabaseReader(mockFileSystem, directoryPath, TestScheduleProvider);

            var tableNames = await databaseReader
                .GetFieldFilePaths("TestTable")
                .ToArray()
                .FirstAsync();

            tableNames.Should().BeEquivalentTo(args.Select(arg => arg.path));
        }

        [Fact]
        public void ShouldNotGetThumbIndex()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var mockFileSystem = new MockFileSystem();

            var databaseReader = this.CreateDatabaseReader(mockFileSystem, directoryPath, TestScheduleProvider);
            var thumbIndex = databaseReader.GetThumbIndex();

            var autoResetEvent = new AutoResetEvent(false);

            thumbIndex.Subscribe(_ => { }, (ex) => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }


        [Fact]
        public void ShouldGetThumbIndex()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var fileCount = Faker.Random.Int(4, 10);

            var thumbIndexContent = new List<byte> { 0x66, 0x66, 0x46, 0x40 };
            thumbIndexContent.AddRange(BitConverter.GetBytes(fileCount));

            var fileInputs = Enumerable.Repeat(Unit.Default, fileCount)
                .Select(_ => (index: Faker.Random.UInt(0, 100), word: Faker.Random.Word()))
                .ToArray();

            foreach (var fileInput in fileInputs)
            {
                thumbIndexContent.AddRange(Encoding.ASCII.GetBytes(fileInput.word));
                thumbIndexContent.Add(0x00);
                thumbIndexContent.AddRange(Enumerable.Repeat<byte>(0x00, 26));
                thumbIndexContent.AddRange(BitConverter.GetBytes(fileInput.index));
            }

            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Path.Combine(directoryPath, "thumbindex.db"), new MockFileData(thumbIndexContent.ToArray()) }
            });

            var databaseReader = this.CreateDatabaseReader(mockFileSystem, directoryPath, TestScheduleProvider);
            var thumbIndex = databaseReader.GetThumbIndex().ToArray();

            var autoResetEvent = new AutoResetEvent(false);

            IndexData[] indexData = null;
            thumbIndex.Subscribe(Observer.Create<IndexData[]>(
                onNext: datas =>
                {
                    indexData = datas;
                },
                onError: exception =>
                {
                },
                onCompleted: () =>
                {
                    autoResetEvent.Set();
                }));

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();

            indexData.Should().NotBeNull();
            indexData.Should().HaveCount(fileCount);

            for (int i = 0; i < indexData.Length; i++)
            {
                indexData[i].Content.Should().Be(fileInputs[i].word);
                indexData[i].Index.Should().Be(fileInputs[i].index);
            }
        }
    }
}
