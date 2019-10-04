using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Scripting.Batch;
using Playnite.Common;
using System.IO;

namespace Playnite.Tests.Scripting.Batch
{
    [TestFixture]
    public class BatchTests
    {
        [Test]
        public void ExecuteWorkDirTest()
        {
            using (var tempDir = TempDirectory.Create())
            using (var runtime = new BatchRuntime())
            {
                var outPath = "workDirTest.txt";
                FileSystem.DeleteFile(outPath);
                FileAssert.DoesNotExist(outPath);
                runtime.Execute(@"echo test> workDirTest.txt");
                FileAssert.Exists(outPath);

                outPath = Path.Combine(tempDir.TempPath, outPath);
                FileSystem.DeleteFile(outPath);
                FileAssert.DoesNotExist(outPath);
                runtime.Execute(@"echo test> workDirTest.txt", tempDir.TempPath);
                FileAssert.Exists(outPath);
            }
        }

        [Test]
        public void ErrorHandlingTest()
        {
            using (var runtime = new BatchRuntime())
            {
                Assert.Throws<BatchRuntimeException>(() => runtime.Execute("ech test"));
            }
        }
    }
}
