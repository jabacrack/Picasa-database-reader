using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace PicasaDatabaseReader.Core.Fields
{
    public class StringField : FieldBase
    {
        public StringField(string name, string path, uint count, IFileSystem fileSystem)
            : base(typeof(string), name, path, count, fileSystem)
        {
        }

        public override IObservable<object> GetValues()
        {
            return Observable.Create<object>(observer =>
            {
                var bytes = new List<byte>();

                string GetString()
                {
                    var result = Encoding.ASCII.GetString(bytes.ToArray());
                    bytes.Clear();
                    return result;
                }

                var observable = GetObservable();
                return observable.Subscribe(Observer.Create<byte>(
                    onNext: b =>
                    {
                        if (b != 0x0)
                        {
                            bytes.Add(b);
                        }
                        else
                        {
                            observer.OnNext(GetString());
                        }
                    },
                    onError: observer.OnError, 
                    onCompleted: observer.OnCompleted));
            });
        }
    }
}