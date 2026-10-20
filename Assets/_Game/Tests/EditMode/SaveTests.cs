using System;
using System.IO;
using NUnit.Framework;
using LocalGame;

public class SaveTests
{
    [Serializable] public class Data { public int Value; public string Id; }
    string dir,path;
    [SetUp] public void SetUp(){dir=Path.Combine(Path.GetTempPath(),"game-save-test-"+Guid.NewGuid().ToString("N"));path=Path.Combine(dir,"save.json");}
    [TearDown] public void TearDown(){if(Directory.Exists(dir))Directory.Delete(dir,true);}
    static void Validate(Data d){if(d==null||d.Value<0)throw new ArgumentException("Invalid");}
    [Test] public void FreshSaveIsAbsentWithoutError(){var s=new AtomicSave(path);Assert.Null(s.Read<Data>(Validate,out var status));Assert.AreEqual("New local game",status);}
    [Test] public void AtomicRoundTripAndBackupRecovery()
    {
        var s=new AtomicSave(path);s.Write(new Data{Value=17,Id="one"});s.Write(new Data{Value=23,Id="two"});
        Assert.AreEqual(23,s.Read<Data>(Validate,out var status).Value);
        File.WriteAllText(path,"truncated");Assert.AreEqual(17,s.Read<Data>(Validate,out status).Value);Assert.AreEqual("Recovered local backup",status);
        Assert.False(File.Exists(path+".tmp"));
    }
    [Test] public void InvalidDomainStateRejectedDespiteValidChecksum()
    {var s=new AtomicSave(path);s.Write(new Data{Value=-1});Assert.Null(s.Read<Data>(Validate,out var status));Assert.True(status.StartsWith("Save unreadable"));}
    [Test] public void NoSilentResetWhenBothCopiesCorrupt()
    {
        var s=new AtomicSave(path);s.Write(new Data{Value=1});s.Write(new Data{Value=2});File.WriteAllText(path,"bad");File.WriteAllText(path+".bak","bad");
        Assert.Null(s.Read<Data>(Validate,out var status));Assert.True(status.StartsWith("Save unreadable"));Assert.AreEqual("bad",File.ReadAllText(path));
    }
    [Test] public void RecoveryWritePreservesLastKnownGoodBackup()
    {
        var s=new AtomicSave(path);s.Write(new Data{Value=17});s.Write(new Data{Value=23});File.WriteAllText(path,"bad");
        Assert.AreEqual(17,s.Read<Data>(Validate,out var status).Value);s.Write(new Data{Value=31});File.WriteAllText(path,"bad again");
        Assert.AreEqual(17,s.Read<Data>(Validate,out status).Value);
    }
    [Test] public void FullGameJsonRoundTripRetainsLedgerAndResolvedBoard()
    {
        var s=new CashflowChains.Domain.Session();s.Move(2,1,1);s.Activate("persisted-result");
        var store=new AtomicSave(path);store.Write(s.Export());var loaded=store.Read<CashflowChains.Domain.SessionState>(v=>CashflowChains.Domain.Session.Restore(v),out var status);
        var copy=CashflowChains.Domain.Session.Restore(loaded);Assert.AreEqual(500,copy.Snapshot.Wallet);Assert.False(copy.Activate("persisted-result"));Assert.AreEqual(s.Preview().Trace(),copy.Preview().Trace());
    }
}
