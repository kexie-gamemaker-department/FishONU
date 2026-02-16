using System;
using Mirror;
using R3;

namespace FishONU.Utils
{
    public static class ReactiveExtensions
    {
        public static Observable<(SyncList<T>.Operation, int, T)> OnChangeAsObservable<T>(this SyncList<T> syncList)
        {
            return Observable.Create<(SyncList<T>.Operation, int, T)>(observer =>
            {
                Action<SyncList<T>.Operation, int, T> handler = (op, index, value) =>
                    observer.OnNext((op, index, value));

                syncList.OnChange += handler;

                return Disposable.Create(() => { syncList.OnChange -= handler; });
            });
        }
    }
}