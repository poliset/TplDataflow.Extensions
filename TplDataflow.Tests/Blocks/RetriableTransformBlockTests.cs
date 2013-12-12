using System;
using System.Threading.Tasks.Dataflow;
using Adform.TplDataflow.Extensions.Blocks;
using Microsoft.Practices.TransientFaultHandling;
using Moq;
using NUnit.Framework;

namespace TplDataflow.Tests.Blocks
{
    [TestFixture]
    public class RetriableTransformBlockTests : TransformBlockBaseTests
    {
        public override IPropagatorBlock<int, int> CreateBlock(IPropagatorBlock<int, int> internalBlock)
        {
            return new RetriableTransformBlockImpl(internalBlock);
        }

        [Test]
        public void Retries_On_Exception()
        {
            var fakeFunc = new Mock<Func<int, int>>(MockBehavior.Strict);
            fakeFunc.Setup(f => f(It.IsAny<int>())).ReturnsInOrder(new Exception(), new Exception(), 1, 2, 3);

            var retryPolicy = new RetryPolicy<HandleAnyException>(new FixedInterval(3, TimeSpan.Zero));

            var block = new RetriableTransformBlockImpl(new ExecutionDataflowBlockOptions(), retryPolicy, fakeFunc.Object);
            var outputBlock = new ActionBlock<int>(_ => block.Complete());

            block.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

            block.SendAsync(-1).Wait();
            outputBlock.Completion.Wait();

            fakeFunc.Verify(f => f(It.IsAny<int>()), Times.Exactly(3));
            Assert.That(block.Retried, Is.EqualTo(2));
        }

        private class RetriableTransformBlockImpl : RetriableTransformBlock<int, int>
        {
            private readonly Func<int, int> _handle;

            public RetriableTransformBlockImpl(IPropagatorBlock<int, int> block)
                : base(block)
            {
            }

            public RetriableTransformBlockImpl(ExecutionDataflowBlockOptions options, RetryPolicy retryPolicy, Func<int, int> handle)
                : base(options, retryPolicy)
            {
                _handle = handle;
            }

            public int Retried { get; private set; }

            public override int Handle(int input)
            {
                return _handle(input);
            }

            protected override void OnRetry(RetryingEventArgs args)
            {
                Retried++;
            }
        }
    }
}