using System;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;

namespace PicasaDatabaseReader.Core.Fields
{
    public class DateTimeField : FieldBase
    {
        public DateTimeField(string name, string filepath, uint count, IFileSystem fileSystem)
            : base(typeof(DateTime), name, filepath, count, fileSystem)
        {
        }

        public override IObservable<object> GetValues()
        {
            return Observable.Create<object>(observer =>
            {
                var counter = 0;
                var bytes = new byte[8];

                DateTime GetValue()
                {
                    var d = BitConverter.ToDouble(bytes, 0);
                    return DateTime.FromOADate(d);
                }

                var observable = GetObservable();
                return observable.Subscribe(Observer.Create<byte>(
                    onNext: b =>
                    {
                        bytes[counter % 8] = b;

                        if ((counter + 1) % 8 == 0)
                        {
                            observer.OnNext(GetValue());
                        }

                        counter++;
                    },
                    onError: observer.OnError,
                    onCompleted: () =>
                    {
                        if ((counter + 1) % 8 == 0)
                        {
                            observer.OnNext(GetValue());
                        }

                        observer.OnCompleted();
                    }));
            });
        }
    }
}
