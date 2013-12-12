using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Moq;
using NUnit.Framework;

namespace TplDataflow.Tests.Blocks
{
    public abstract class TransformBlockBaseTests
    {
        private Mock<IPropagatorBlock<int, int>> _fakeBlock;
        private Mock<ITargetBlock<int>> _fakeTarget;
        private IPropagatorBlock<int, int> _block;

        [SetUp]
        public void SetUp()
        {
            _fakeBlock = MakeFakePropogatorBlock();
            _fakeTarget = new Mock<ITargetBlock<int>>();

            _block = CreateBlock(_fakeBlock.Object);
        }

        public abstract IPropagatorBlock<int, int> CreateBlock(IPropagatorBlock<int, int> internalBlock);

        [Test]
        public void Completion_Returns_Underlying_DataFlow_Block_Task()
        {
            Assert.That(_block.Completion, Is.EqualTo(_fakeBlock.Object.Completion));
        }

        [Test]
        public void Complete_Calls_Underlying_DataFlow_Block_Method()
        {
            _block.Complete();

            _fakeBlock.Verify(b => b.Complete(), Times.Once());
        }

        [Test]
        public void Fault_Calls_Underlying_Target_Block_Method()
        {
            _block.Fault(new Exception());

            _fakeBlock.Verify(b => b.Fault(It.IsAny<Exception>()), Times.Once());
        }

        [Test]
        public void LinkTo_Calls_Underlying_Target_Block_Method()
        {
            _block.LinkTo(_fakeTarget.Object, new DataflowLinkOptions());

            _fakeBlock.Verify(b => b.LinkTo(It.IsAny<ITargetBlock<int>>(), It.IsAny<DataflowLinkOptions>()), Times.Once());
        }

        [Test]
        public void ConsumeMessage_Calls_Underlying_Target_Block_Method()
        {
            bool consumed;
            _block.ConsumeMessage(new DataflowMessageHeader(1), _fakeTarget.Object, out consumed);

            _fakeBlock.Verify(b => b.ConsumeMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>(), out consumed), Times.Once());
        }

        [Test]
        public void ReserveMessage_Calls_Underlying_Target_Block_Method()
        {
            _block.ReserveMessage(new DataflowMessageHeader(1), _fakeBlock.Object);

            _fakeBlock.Verify(b => b.ReserveMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>()), Times.Once());
        }

        [Test]
        public void ReleaseReservation_Calls_Underlying_Target_Block_Method()
        {
            _block.ReleaseReservation(new DataflowMessageHeader(1), _fakeBlock.Object);

            _fakeBlock.Verify(b => b.ReleaseReservation(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>()), Times.Once());
        }

        [Test]
        public void OfferMessage_Calls_Underlying_Source_Block_Method()
        {
            const int message = -1;
            _block.OfferMessage(new DataflowMessageHeader(1), message, _fakeBlock.Object, true);

            _fakeBlock.Verify(b => b.OfferMessage(It.IsAny<DataflowMessageHeader>(), message, It.IsAny<ISourceBlock<int>>(), true), Times.Once());
        }

        private static Mock<IPropagatorBlock<int, int>> MakeFakePropogatorBlock()
        {
            var disposable = new Mock<IDisposable>().Object;
            var fakeBlock = new Mock<IPropagatorBlock<int, int>>(MockBehavior.Strict);
            fakeBlock.Setup(b => b.Completion).Returns(new Task(() => { }));
            fakeBlock.Setup(b => b.Complete());
            fakeBlock.Setup(b => b.Fault(It.IsAny<Exception>()));
            fakeBlock.Setup(b => b.LinkTo(It.IsAny<ITargetBlock<int>>(), It.IsAny<DataflowLinkOptions>())).Returns(disposable);
            bool consumed;
            fakeBlock.Setup(b => b.ConsumeMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>(), out consumed)).Returns(It.IsAny<int>());
            fakeBlock.Setup(b => b.ReserveMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>())).Returns(It.IsAny<bool>());
            fakeBlock.Setup(b => b.ReleaseReservation(It.IsAny<DataflowMessageHeader>(), It.IsAny<ITargetBlock<int>>()));
            fakeBlock.Setup(b => b.OfferMessage(It.IsAny<DataflowMessageHeader>(), It.IsAny<int>(), It.IsAny<ISourceBlock<int>>(), It.IsAny<bool>())).Returns(It.IsAny<DataflowMessageStatus>());
            return fakeBlock;
        }
    }
}