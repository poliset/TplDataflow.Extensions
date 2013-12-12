using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Adform.TplDataflow.Extensions.Blocks;
using Moq;
using NUnit.Framework;

namespace TplDataflow.Tests.Blocks
{
    [TestFixture]
    public class SourceBlockTests
    {
        private List<int> _items;
        private ExecutionDataflowBlockOptions _options;
        private DataflowLinkOptions _linkOptions;
        private Mock<IPropagatorBlock<int, int>> _fakeBlock;
        private ActionBlock<int> _outputBlock;
        private SourceBlock<int> _sourceBlock;

        [SetUp]
        public void SetUp()
        {
            _items = new List<int>();

            _options = new ExecutionDataflowBlockOptions { BoundedCapacity = 1 };
            _linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            _outputBlock = new ActionBlock<int>(i => _items.Add(i), _options);
            
            _fakeBlock = MakeFakePropogatorBlock();
            _sourceBlock = new SourceBlockImpl(_fakeBlock.Object);
            _sourceBlock.LinkTo(_outputBlock, _linkOptions);
        }

        [TearDown]
        public void Cleanup()
        {
            _sourceBlock.Dispose();
        }

        [Test]
        public void Can_Process()
        {
            _sourceBlock = new SourceBlockImpl(_options);
            _sourceBlock.LinkTo(_outputBlock, _linkOptions);

            for (var i = 0; i < 5; i++)
            {
                if (_items.Count > 0) break;

                // wait while block is initialized
                _sourceBlock.Completion.Wait(TimeSpan.FromMilliseconds(50));
            }
            
            _sourceBlock.Complete();
            _outputBlock.Completion.Wait();

            Assert.That(_items.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Can_Complete_Source()
        {
            _sourceBlock = new SourceBlockImpl(_options);
            _sourceBlock.LinkTo(_outputBlock, _linkOptions);

            for (var i = 0; i < 5; i++)
            {
                if (_items.Count > 0) break;

                _sourceBlock.Completion.Wait(TimeSpan.FromMilliseconds(10));
            }

            _sourceBlock.Complete();
            _outputBlock.Completion.Wait();

            Assert.That(_outputBlock.Completion.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }

        [Test]
        public void Completion_Returns_Underlying_DataFlow_Block_Task()
        {
            Assert.That(_sourceBlock.Completion, Is.EqualTo(_fakeBlock.Object.Completion));
        }

        [Test]
        public void Complete_Calls_Underlying_DataFlow_Block_Method()
        {
            _sourceBlock.Complete();

            _fakeBlock.Verify(b => b.Complete(), Times.Once());
        }

        [Test]
        public void Fault_Calls_Underlying_Target_Block_Method()
        {
            _sourceBlock.Fault(new Exception());

            _fakeBlock.Verify(b => b.Fault(It.IsAny<Exception>()), Times.Once());
        }

        [Test]
        public void LinkTo_Calls_Underlying_Target_Block_Method()
        {
            _sourceBlock = new SourceBlockImpl(_options);            
            _sourceBlock.LinkTo(_outputBlock, new DataflowLinkOptions());

            _fakeBlock.Verify(b => b.LinkTo(It.IsAny<ITargetBlock<int>>(), It.IsAny<DataflowLinkOptions>()), Times.Once());
        }

        [Test]
        public void ConsumeMessage_Calls_Underlying_Target_Block_Method()
        {
            bool consumed;
            _sourceBlock.ConsumeMessage(new DataflowMessageHeader(1), _outputBlock, out consumed);

            _fakeBlock.Verify(b => b.ConsumeMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>(), out consumed), Times.Once());
        }

        [Test]
        public void ReserveMessage_Calls_Underlying_Target_Block_Method()
        {
            _sourceBlock.ReserveMessage(new DataflowMessageHeader(1), _fakeBlock.Object);

            _fakeBlock.Verify(b => b.ReserveMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>()), Times.Once());
        }

        [Test]
        public void ReleaseReservation_Calls_Underlying_Target_Block_Method()
        {
            _sourceBlock.ReleaseReservation(new DataflowMessageHeader(1), _fakeBlock.Object);

            _fakeBlock.Verify(b => b.ReleaseReservation(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>()), Times.Once());
        }

        private static Mock<IPropagatorBlock<int, int>> MakeFakePropogatorBlock()
        {
            var fakeBlock = new Mock<IPropagatorBlock<int, int>>(MockBehavior.Strict);

            fakeBlock.Setup(b => b.Completion).Returns(new Task(() => { }));
            fakeBlock.Setup(b => b.Complete());
            fakeBlock.Setup(b => b.Fault(It.IsAny<Exception>()));
            var disposable = new Mock<IDisposable>().Object;
            fakeBlock.Setup(b => b.LinkTo(It.IsAny<ITargetBlock<int>>(), It.IsAny<DataflowLinkOptions>())).Returns(disposable);
            bool consumed;
            fakeBlock.Setup(b => b.ConsumeMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>(), out consumed)).Returns(It.IsAny<int>());
            fakeBlock.Setup(b => b.ReserveMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>())).Returns(It.IsAny<bool>());
            fakeBlock.Setup(b => b.ReleaseReservation(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>()));
            fakeBlock.Setup(b => b.OfferMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<int>(), It.IsAny<ISourceBlock<int>>(), It.IsAny<bool>())).Returns(DataflowMessageStatus.Accepted);

            return fakeBlock;
        }

        private class SourceBlockImpl : SourceBlock<int>
        {
            private int _counter;

            public SourceBlockImpl(DataflowBlockOptions options)
                : base(options)
            {
            }

            public SourceBlockImpl(IPropagatorBlock<int, int> block)
                : base(block)
            {
            }

            protected override void Produce(IPropagatorBlock<int, int> block, CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        block.SendAsync(_counter++, token).Wait();
                        Thread.Sleep(10);
                    }
                    catch (AggregateException ae)
                    {
                        var inner = ae.Flatten().InnerException;
                        if (!(inner is OperationCanceledException)) throw;
                    }
                }
            }
        }
    }
}
