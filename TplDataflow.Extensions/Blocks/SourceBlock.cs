using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Adform.TplDataflow.Extensions.Blocks
{
    public abstract class SourceBlock<TOutput> : ISourceBlock<TOutput>, IDisposable
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private readonly IPropagatorBlock<TOutput, TOutput> _block;
        private readonly Task _task;

        protected SourceBlock(DataflowBlockOptions options)
            : this(new BufferBlock<TOutput>(options))
        {
        }

        internal SourceBlock(IPropagatorBlock<TOutput, TOutput> block)
        {
            if (block == null) throw new ArgumentNullException("block");

            _block = block;
            _task = new Task(() => Produce(_block, _tokenSource.Token), TaskCreationOptions.LongRunning);
            _task.ContinueWith(t => _block.Fault(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        protected abstract void Produce(IPropagatorBlock<TOutput, TOutput> block, CancellationToken token);

        public Task Completion
        {
            get { return _block.Completion; }
        }

        public void Complete()
        {
            if (!_block.Completion.IsCompleted)
            {
                _tokenSource.Cancel();

                if (_task.Status != TaskStatus.Created)
                {
                    _task.Wait();
                }

                _block.Complete();
            }
        }

        public void Fault(Exception exception)
        {
            _tokenSource.Cancel();

            _block.Fault(exception);
        }

        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            if (_task.Status == TaskStatus.Created)
            {
                _task.Start();
            }

            return _block.LinkTo(target, linkOptions);
        }

        public TOutput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            return _block.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            return _block.ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            _block.ReleaseReservation(messageHeader, target);
        }

        public virtual void Dispose()
        {
            if (!_block.Completion.IsCompleted)
            {
                Complete();
            }
        }
    }
}
