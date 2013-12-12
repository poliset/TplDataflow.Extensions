using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Adform.TplDataflow.Extensions.Blocks
{
    public abstract class OptionalTransformBlock<TInput, TOutput> : IPropagatorBlock<TInput, TOutput>
    {
        private readonly IPropagatorBlock<TInput, TOutput> _block;

        protected OptionalTransformBlock(ExecutionDataflowBlockOptions options, bool throwOnException = true)
        {
            _block = new TransformManyBlock<TInput, TOutput>(input =>
            {
                try
                {
                    var output = Handle(input);
                    var emptyOutput = EqualityComparer<TOutput>.Default.Equals(output, default(TOutput));
                    return emptyOutput ? Enumerable.Empty<TOutput>() : new[] { output };
                }
                catch
                {
                    if (throwOnException) throw;

                    return Enumerable.Empty<TOutput>();
                }

            }, options);
        }

        internal OptionalTransformBlock(IPropagatorBlock<TInput, TOutput> block)
        {
            if (block == null) throw new ArgumentNullException("block");

            _block = block;
        }

        public abstract TOutput Handle(TInput input);

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