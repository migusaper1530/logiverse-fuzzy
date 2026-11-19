using System;
using System.IO;
public static class FakeMatlab
{
    public static void Main(string[] args)
    {
        File.WriteAllLines(Environment.GetEnvironmentVariable("LOGIVERSE_TEST_ARGUMENTS"), args);
    }
}
