using System.Threading.Tasks.Dataflow;
using Adform.TplDataflow.Extensions.Blocks;
using NUnit.Framework;

namespace TplDataflow.Tests.Blocks
{
    [TestFixture]
    public class SimpleTransformBlockTests : TransformBlockBaseTests
    {
        public override IPropagatorBlock<int, int> CreateBlock(IPropagatorBlock<int, int> internalBlock)
        {
            return new SimpleTransformBlockImpl(internalBlock);
        }

        private class SimpleTransformBlockImpl : SimpleTransformBlock<int, int>
        {
            public SimpleTransformBlockImpl(IPropagatorBlock<int, int> block)
                : base(block)
            {
            }

            public override int Handle(int input)
            {
                return input;
            }
        }
    }
}