using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Adform.TplDataflow.Extensions.Blocks;
using NUnit.Framework;

namespace TplDataflow.Tests.Blocks
{
    [TestFixture]
    public class SimpleTransformManyBlockTests : TransformBlockBaseTests
    {
        public override IPropagatorBlock<int, int> CreateBlock(IPropagatorBlock<int, int> internalBlock)
        {
            return new SimpleTransformManyBlockImpl(internalBlock);
        }

        private class SimpleTransformManyBlockImpl : SimpleTransformManyBlock<int, int>
        {
            public SimpleTransformManyBlockImpl(IPropagatorBlock<int, int> block)
                : base(block)
            {
            }

            public override IEnumerable<int> Handle(int input)
            {
                return new[] { input };
            }
        }
    }
}