using LogTest;
using System.IO;
using Xunit;

namespace LilTests;

[TestClass]
public class UnitTests
{
    private readonly string basePath = @"./LogTest";

    [TestMethod]
    public void WriteLog_LogsCanBeWritten()
    {
        //Arrange
        ClearTestLogs();
        var logger = new AsyncLogInterface();
        string testMessage = "Write to log from test";
        string fullPath = Path.GetFullPath(basePath);

        //Act
        logger.WriteLog(testMessage);

        string[] testLogs = Directory.GetFiles(fullPath + "/", "*.txt");
        bool filesCreated = Directory.EnumerateFileSystemEntries(fullPath).Any();

        //Assert
        Xunit.Assert.True(filesCreated);
    }

    [TestMethod]
    public void WriteLog_MidnightCrossed()
    {
        //Arrange
        //Act
        //Assert
    }

    [TestMethod]
    public void StopBehavior_StopWithFlush()
    {
        //Arrange
        ClearTestLogs();
        var logger = new AsyncLogInterface();
        string testMessage = "Write to log from test";
        logger.WriteLog(testMessage);

        //Act
        logger.StopWithFlush();
        Thread.Sleep(1000);

        //Assert
        var logFiles = Directory.GetFiles(Path.GetFullPath(basePath));
        Xunit.Assert.Single(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Xunit.Assert.DoesNotContain(testMessage, logContent);
    }

    [TestMethod]
    public void StopBehavior_StopWithoutFlush()
    {
        //Arrange
        ClearTestLogs();
        var logger = new AsyncLogInterface();
        string testMessage = "Write to log from test";
        logger.WriteLog(testMessage);

        //Act
        logger.StopWithoutFlush();
        Thread.Sleep(1000);

        //Assert
        var logFiles = Directory.GetFiles(Path.GetFullPath(basePath));
        Xunit.Assert.Single(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Xunit.Assert.Contains(testMessage, logContent);
    }

    private void ClearTestLogs()
    {
        System.IO.DirectoryInfo di = new DirectoryInfo(Path.GetFullPath(basePath));
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true);
        }
    }
}
