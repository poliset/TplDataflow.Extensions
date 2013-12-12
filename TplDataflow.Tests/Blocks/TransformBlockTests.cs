using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace TplDataflow.Tests.Blocks
{
    [TestFixture]
    public class TransformBlockTests
    {
        [Test]
        public void Preserves_Ordering_When_There_Are_More_Items_Than_Max_Degree_Of_Parallelism()
        {
            const int max = 100;

            var output = new List<int>();
            var inputBlock = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 1 });
            var transformBlock = new TransformBlock<int, int>(i =>
            {
                var wait = max - i;
                Task.Delay(wait).Wait();
                return i;
            }, new ExecutionDataflowBlockOptions { BoundedCapacity = 10, MaxDegreeOfParallelism = 10, MaxMessagesPerTask = 1 });
            var outputBlock = new ActionBlock<int>(i =>
            {
                Console.WriteLine(i);
                output.Add(i);
            }, new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

            inputBlock.LinkTo(transformBlock, new DataflowLinkOptions { PropagateCompletion = true });
            transformBlock.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

            for (var i = 0; i < max; i++)
            {
                inputBlock.SendAsync(i).Wait();
            }

            inputBlock.Complete();
            outputBlock.Completion.Wait();

            CollectionAssert.IsOrdered(output);
        }
        
    }
}