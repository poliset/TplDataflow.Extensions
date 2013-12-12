using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Practices.TransientFaultHandling;

namespace Adform.TplDataflow.Extensions.Blocks
{
    public abstract class RetriableTransformManyBlock<TInput, TOutput> : IPropagatorBlock<TInput, TOutput>
    {
        private readonly IPropagatorBlock<TInput, TOutput> _block;

        protected RetriableTransformManyBlock(ExecutionDataflowBlockOptions options, RetryPolicy retryPolicy, bool throwOnException = true)
        {
            if (retryPolicy == null) throw new ArgumentNullException("retryPolicy");

            retryPolicy.Retrying += (sender, args) => OnRetry(args);

            _block = new TransformManyBlock<TInput, TOutput>(input =>
            {
                try
                {
                    return retryPolicy.ExecuteAction(() => Handle(input));
                }
                catch
                {
                    if (throwOnException) throw;

                    return Enumerable.Empty<TOutput>();
                }

            }, options);
        }

        internal RetriableTransformManyBlock(IPropagatorBlock<TInput, TOutput> block)
        {
            if (block == null) throw new ArgumentNullException("block");

            _block = block;
        }

        public abstract IEnumerable<TOutput> Handle(TInput input);

        protected virtual void OnRetry(RetryingEventArgs args)
        {
        }

        public Task Completion
        {
            get { return _block.Completion; }
        }

        public void Complete()
        {
            _block.Complete();
        }

        public void Fault(Exception exception)
        {
            _block.Fault(exception);
        }

        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
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

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source, bool consumeToAccept)
        {
            return _block.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }
    }
}