using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TgApi.Test;

[TestClass]
public class UtilsTest
{

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        if (Directory.Exists(Constants.OutputDir))
        {
            foreach (var file in new DirectoryInfo(Constants.OutputDir).EnumerateFiles())
            {
                file.Delete();
            }
        }
        else
        {
            Directory.CreateDirectory(Constants.OutputDir);
        }
    }
    
	[TestMethod]
	public void EnsureFile_TestExistingFile()
    {
        var path = $"{Constants.OutputDir}ensurefile_testexistingfile";
        File.Create(path);
        var rval = Utils.EnsureFile(path);
        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(path, rval);
    }

    [TestMethod]
    public void EnsureFile_TestNonexistentFile()
    {
        var path = $"{Constants.OutputDir}ensurefile_testnonexistentpath";
        var rval = Utils.EnsureFile(path);
        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(path, rval);
    }

    [TestMethod]
    public void EnsurePath_TestExistingPath()
    {
        var path = $"{Constants.OutputDir}ensurefile_testexistingpath";
        Directory.CreateDirectory(path);
        var rval = Utils.EnsurePath(path);
        Assert.IsTrue(Directory.Exists(path));
        Assert.AreEqual(path, rval);
    }
    
    [TestMethod]
    public void EnsurePath_TestNonexistentPath()
    {
        var path = $"{Constants.OutputDir}ensurefile_testnonexistentfile";
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
        var path = $"{Constants.OutputDir}serializedeserialize_testlist.json";
        Utils.Serialize(testList, path);
        Assert.IsTrue(File.Exists(path));
        var returnValue = Utils.Deserialize<string[]>(path);
        for (int i = 0; i < testList.Length; i++)
        {
            Assert.AreEqual(testList[i], returnValue[i]);
        }
    }


    
}
