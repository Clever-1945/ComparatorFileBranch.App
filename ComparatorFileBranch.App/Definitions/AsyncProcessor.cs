using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ComparatorFileBranch.App.Definitions
{
    public class AsyncProcessor<TP, TR>
    {
        private Func<TP, TR> _processor;
        private Action<Exception> _catch;
        private Action<TR> _then;
        private Dispatcher _dispatcher;
        private TP _data;

        public AsyncProcessor(Dispatcher dispatcher)
        {
            this._dispatcher = dispatcher;
        }

        public AsyncProcessor<TP, TR> SetProcessor(Func<TP, TR> processor)
        {
            this._processor = processor;
            return this;
        }

        public AsyncProcessor<TP, TR> SetThen(Action<TR> then)
        {
            this._then = then;
            return this;
        }

        public AsyncProcessor<TP, TR> SetCatch(Action<Exception> _catch)
        {
            this._catch = _catch;
            return this;
        }

        public AsyncProcessor<TP, TR> SetData(TP data)
        {
            this._data = data;
            return this;
        }

        public AsyncProcessor<TP, TR> Start(Action onStart, Action onFinish)
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                onStart?.Invoke();
                try
                {
                    var result = default(TR);
                    if (_processor != null)
                    {
                        result = _processor.Invoke(_data);
                    }

                    _dispatcher.Invoke(() =>
                    {
                        this._then?.Invoke(result);
                    });
                }
                catch(Exception ex)
                {
                    this._dispatcher?.Invoke(() => 
                    {
                        this._catch?.Invoke(ex);
                    });
                }

                onFinish?.Invoke();
            });
            
            return this;
        }
    }


    public class AsyncProcessorFn<TR>
    {
        private AsyncProcessor<bool, TR> asyncProcessor = null;
        private Func<TR> _procesor;
        private Action<Exception> _exception;

        public AsyncProcessorFn(Func<TR> procesor, Dispatcher dispatcher, Action<Exception> exception)
        {
            asyncProcessor = new AsyncProcessor<bool, TR>(dispatcher);
            _procesor = procesor;
            _exception = exception;
            asyncProcessor.SetCatch(exception);
            asyncProcessor.SetProcessor((t) =>
            {
                return this._procesor();
            });
        }

        public AsyncProcessorFn<TR> Start(Action onStart, Action onFinish)
        {
            asyncProcessor.Start(onStart, onFinish);
            return this;
        }

        public AsyncProcessorFn<TR> Then(Action<TR> action)
        {
            asyncProcessor.SetThen(action);
            return this;
        }

        public AsyncProcessorFn<TR> Catch(Action<Exception> action)
        {
            asyncProcessor.SetCatch((ex) => 
            {
                this._exception?.Invoke(ex);
                action?.Invoke(ex);
            });
            return this;
        }
    }
}
