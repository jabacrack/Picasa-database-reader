using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace PicasaDatabaseReader.Core.Extensions
{
    public static class ObservableExtensions
    {
        public static IObservable<T> MatchNextItems<T>(this IObservable<T> source, string matchName, params T[] matches)
        {
            return Observable.Create<T>(observer =>
            {
                var counter = 0;

                return source.Subscribe(Observer.Create<T>(
                    onNext: actual =>
                    {
                        if (matches == null)
                        {
                            observer.OnNext(actual);
                        }
                        else
                        {
                            var expected = matches[counter++];

                            if (!actual.Equals(expected))
                            {
                                observer.OnError(new MatchNextItemsException<T>(matchName, actual, expected));
                                return;
                            }

                            if (matches.Length == counter)
                            {
                                matches = null;
                            }
                        }
                    },
                    onCompleted: () =>
                    {
                        if (matches != null)
                        {
                            var expected = matches[counter];
                            observer.OnError(new MatchSourceCompletedException<T>(matchName, expected));
                        }
                        else
                        {
                            observer.OnCompleted();
                        }
                    },
                    onError: observer.OnError));
            });
        }

        public static IObservable<T> MatchNextItems<T>(this IObservable<T> source, params T[] items)
        {
            return MatchNextItems(source, null, items);
        }

        public static IObservable<T> CaptureNextItems<T>(this IObservable<T> source, int count, Action<T[]> callback)
        {
            var items = new T[count];
            var index = 0;

            return source
                .SkipWhile(arg =>
                {
                    if (index == count) return false;

                    items[index++] = arg;

                    if (index == count)
                    {
                        callback(items);
                        items = null;
                    }

                    return true;
                });
        }

        public static IObservable<T[]> Chunk<T>(this IObservable<T> source, int size)
        {
            return Observable.Create<T[]>(observer =>
            {
                var counter = 0;
                var items = new T[size];

                return source.Subscribe(Observer.Create<T>(
                    onNext: b =>
                    {
                        items[counter % size] = b;

                        if ((counter + 1) % size == 0)
                        {
                            observer.OnNext(items.ToArray());
                        }

                        counter++;
                    },
                    onError: observer.OnError,
                    onCompleted: () =>
                    {
                        if (counter % size != 0)
                        {
                            observer.OnError(new Exception());
                        }
                        else
                        {
                            observer.OnCompleted();
                        }
                    }));
            });
        }
    }

    public class MatchNextItemsException<T> : Exception
    {
        public MatchNextItemsException(T actual, T expected) : base($"Actual: {actual} Expected: {expected}")
        {
        }

        public MatchNextItemsException(string match, T actual, T expected) : base($"MatchName: {match} Actual: {actual} Expected: {expected}")
        {
        }
    }

    public class MatchSourceCompletedException<T> : Exception
    {
        public MatchSourceCompletedException(T expected) : base($"Expected: {expected}")
        {
        }

        public MatchSourceCompletedException(string match, T expected) : base($"MatchName: {match} Expected: {expected}")
        {
        }
    }
}