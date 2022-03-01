using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TgApi.Test;

[TestClass]
public class UtilsTest
{

    public static string UtilsTestDir = $"{Constants.OutputDir}UtilsTestDirectory{Path.DirectorySeparatorChar}";
    
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        var path = UtilsTestDir;
        if (Directory.Exists(path)) foreach (var file in new DirectoryInfo(path).EnumerateFiles()) file.Delete();
        else Directory.CreateDirectory(path);
    }
    
	[TestMethod]
	public void EnsureFile_TestExistingFile()
    {
        var path = $"{UtilsTestDir}ensurefile_testexistingfile";
        File.Create(path);
        var rval = Utils.EnsureFile(path);
        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(path, rval);
    }

    [TestMethod]
    public void EnsureFile_TestNonexistentFile()
    {
        var path = $"{UtilsTestDir}ensurefile_testnonexistentpath";
        var rval = Utils.EnsureFile(path);
        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(path, rval);
    }

    [TestMethod]
    public void EnsurePath_TestExistingPath()
    {
        var path = $"{UtilsTestDir}ensurefile_testexistingpath";
        Directory.CreateDirectory(path);
        var rval = Utils.EnsurePath(path);
        Assert.IsTrue(Directory.Exists(path));
        Assert.AreEqual(path, rval);
    }
    
    [TestMethod]
    public void EnsurePath_TestNonexistentPath()
    {
        var path = $"{UtilsTestDir}ensurefile_testnonexistentfile";
        var rval = Utils.EnsurePath(path);
        Assert.IsTrue(Directory.Exists(path));
        Assert.AreEqual(path, rval);
    }

    [TestMethod]
    public void SerializeDeserialize_TestList()
    {
        string[] testList =
        {
            "hi", "hello", "this", "is", "a", "test", "!!!!", "richard"
        };
        var path = $"{UtilsTestDir}serializedeserialize_testlist.json";
        Utils.Serialize(testList, path);
        Assert.IsTrue(File.Exists(path));
        var returnValue = Utils.Deserialize<string[]>(path);
        for (int i = 0; i < testList.Length; i++)
        {
            Assert.AreEqual(testList[i], returnValue[i]);
        }
    }


    
}
