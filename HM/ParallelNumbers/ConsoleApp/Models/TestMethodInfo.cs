using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class TestMethodInfo
    {
        public string TestType { get; private set; }
        public long LoadTime { get; private set; }
        public int Result { get; private set; }

        public TestMethodInfo(string testType, long loadTime, int result)
        {
            TestType = testType;
            LoadTime = loadTime;
            Result = result;
        }
    }
}