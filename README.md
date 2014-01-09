TplDataflow.Extensions [![Build status](https://ci.appveyor.com/api/projects/status?id=g2883afb9a0kbqsa)](https://ci.appveyor.com/project/adform-tpldataflow-extensions)
======================

Set of extensions for [Microsoft TPL Dataflow](http://msdn.microsoft.com/en-us/library/hh228603(v=vs.110).aspx) library

Available blocks:
*  [SourceBlock](https://github.com/adform/TplDataflow.Extensions/blob/master/TplDataflow.Extensions/Blocks/SourceBlock.cs) - absract block that provides basic implementation of [ISourceBlock](http://msdn.microsoft.com/en-us/library/hh160369(v=vs.110\).aspx)
*  [SimpleTransformBlock](https://github.com/adform/TplDataflow.Extensions/blob/master/TplDataflow.Extensions/Blocks/SimpleTransformBlock.cs) - abstract block similar to [TransformBlock](http://msdn.microsoft.com/en-us/library/hh194782(v=vs.110\).aspx), but can swallow exceptions
*  [SimpleTransformManyBlock](https://github.com/adform/TplDataflow.Extensions/blob/master/TplDataflow.Extensions/Blocks/SimpleTransformManyBlock.cs) - abstract block similar to [TransformManyBlock](http://msdn.microsoft.com/en-us/library/hh194784(v=vs.110\).aspx), but can swallow exceptions
*  [RetriableTransformBlock](https://github.com/adform/TplDataflow.Extensions/blob/master/TplDataflow.Extensions/Blocks/RetriableTransformBlock.cs) - abstract block similar to [TransformBlock](http://msdn.microsoft.com/en-us/library/hh194782(v=vs.110\).aspx), but can retry on exception by using specified [RetryPolicy](http://msdn.microsoft.com/en-us/library/hh680901(v=pandp.50\).aspx)
*  [RetriableTransformManyBlock](https://github.com/adform/TplDataflow.Extensions/blob/master/TplDataflow.Extensions/Blocks/RetriableTransformManyBlock.cs) - abstract block similar to [TransformManyBlock](http://msdn.microsoft.com/en-us/library/hh194784(v=vs.110\).aspx), but can retry on exception by using specified [RetryPolicy](http://msdn.microsoft.com/en-us/library/hh680901(v=pandp.50\).aspx)
*  [OptionalTransformBlock](https://github.com/adform/TplDataflow.Extensions/blob/master/TplDataflow.Extensions/Blocks/OptionalTransformBlock.cs) - abstract block similar to [TransformManyBlock](http://msdn.microsoft.com/en-us/library/hh194782(v=vs.110\).aspx), but returns single or no result
