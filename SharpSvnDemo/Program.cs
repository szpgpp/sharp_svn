using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpSvn;
namespace SharpSvnDemo
{
    class Program
    {
        static SvnClient client = new SvnClient();
        static void Main(string[] args)
        {
            client.Authentication.Clear();
            client.Authentication.UserNamePasswordHandlers += (sender, e) =>
            {
                e.UserName = "szp";
                e.Password = "85071163";
            };
            client.Conflict += (sender, e) =>
            {
                Console.WriteLine(" {0}","there is conflict in your local, please dealing with it first!");
            };
            while (true)
            {
                Console.Write(">");
                var cmd_str = Console.ReadLine();
                if (String.IsNullOrEmpty(cmd_str)) continue;
                var arr_cmd = cmd_str.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var cmd = arr_cmd[0];
                var cmd_param = String.Empty;
                if (arr_cmd.Length >= 2) cmd_param = arr_cmd[1];
                if (cmd == "getinfo")
                {
                    if (cmd_param == String.Empty || cmd_param == "remote")
                    {
                        string repoUrl = "svn://118.25.197.165/test_szp";
                        Console.WriteLine(" to getinfo {0} ...", repoUrl);
                        SvnInfoEventArgs info;
                        client.GetInfo(new Uri(repoUrl), out info);
                        Console.WriteLine(" Reversion:{0} lastChangedReversion:{1} lastChangedTime:{2} islocked:{3}"
                            , info.Revision
                            , info.LastChangeRevision
                            , info.LastChangeTime
                            , info.Lock != null
                            );
                    }
                    else
                    {
                        var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                        Console.WriteLine(" to getinfo {0} ...", local_path);
                        try
                        {
                            SvnInfoEventArgs info;
                            client.GetInfo(new SvnPathTarget(local_path), out info);
                            Console.WriteLine(" Reversion:{0} lastChangedReversion:{1} lastChangedTime:{2} islocked:{3}"
                                , info.Revision
                                , info.LastChangeRevision
                                , info.LastChangeTime
                                , info.Lock != null
                            );
                        }
                        catch (SvnException svnEx)
                        {
                            if (svnEx.SvnErrorCode == SvnErrorCode.SVN_ERR_WC_PATH_NOT_FOUND)
                            {
                                Console.WriteLine(" {0}", "it is not a svn path! your should add it into svn.");
                            }
                            else if (svnEx.SvnErrorCode == SvnErrorCode.SVN_ERR_WC_NOT_WORKING_COPY)
                            {
                                Console.WriteLine(" {0}", "it is not a svn dir! your should checkout this dir.");
                            }
                            else
                            {
                                Console.WriteLine(" {0}({1})", svnEx.Message, svnEx.SvnErrorCode);
                            }
                        }
                    }
                }
                else if (cmd == "checkout")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}\", cmd_param);
                    Console.WriteLine(" to checkout {0} ...", local_path);
                    var repo_url = "svn://118.25.197.165/test_szp";

                    var r = client.CheckOut(new SvnUriTarget(repo_url), local_path);
                    if (r)
                    {
                        Console.WriteLine(" {0}", "check out successfully!");
                    }
                    else
                    {
                        Console.WriteLine(" {0}", "check out faild!");
                    }
                }
                else if (cmd == "getstate")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to getstate {0} ...", local_path);
                    var r = SvnTools.IsManagedPath(local_path);
                    if (r)
                    {
                        Console.WriteLine(" {0} is svn local path", local_path);
                    }
                    else
                    {
                        Console.WriteLine(" {0} is not svn local path", local_path);
                    }
                }
                else if (cmd == "geturi")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to geturi {0} ...", local_path);
                    var uri = client.GetUriFromWorkingCopy(local_path);
                    Console.WriteLine(" {0} ====svn==== {1}", local_path, uri);
                }
                else if (cmd == "add")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to add {0} ...", local_path);
                    var r = client.Add(local_path);
                    if (r)
                        Console.WriteLine(" add {0} successfully!", local_path);
                    else
                        Console.WriteLine(" faild to add {0} !", local_path);
                }
                else if (cmd == "commit")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to commit {0} ...", local_path);
                    var arg = new SvnCommitArgs();
                    arg.LogMessage = String.Format("test {0:MM:dd HH:mm}", DateTime.Now);
                    SvnCommitResult result;
                    try
                    {
                        var r = client.Commit(local_path, arg, out result);
                        if (r)
                        {
                            Console.WriteLine(" {0}", "commit successfully!");
                        }
                        else
                        {
                            Console.WriteLine(" {0}", "faild to commit!");
                        }
                    }
                    catch (SvnException svnEx)
                    {
                        if (svnEx.SvnErrorCode == SvnErrorCode.SVN_ERR_FS_BAD_LOCK_TOKEN)
                            Console.WriteLine(" {0} ({1})", "faild to commit!", "another locked this node!");
                        else if (svnEx.SvnErrorCode == SvnErrorCode.SVN_ERR_FS_TXN_OUT_OF_DATE)
                            Console.WriteLine(" {0} ({1})", "faild to commit!", "please first update!");
                        else if (svnEx.SvnErrorCode == SvnErrorCode.SVN_ERR_WC_FOUND_CONFLICT)
                            Console.WriteLine(" {0} ({1})", "faild to commit!", "please dealing with conflict!");
                        else
                            Console.WriteLine(" {0} ({1})", svnEx.Message, svnEx.SvnErrorCode);
                    }
                }
                else if (cmd == "update")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to update {0} ...", local_path);
                    var r = client.Update(local_path);
                    if (r)
                    {
                        Console.WriteLine(" {0}", "Update successfully!");
                    }
                    else
                    {
                        Console.WriteLine(" {0}", "Update faild!");
                    }
                }
                else if (cmd == "remotelock")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to lock {0} ...", local_path);
                    var uri = client.GetUriFromWorkingCopy(local_path);
                    var r = client.RemoteLock(uri, String.Format("test lock {0:MM-dd HH:mm}", DateTime.Now));
                    if (r)
                    {
                        Console.WriteLine(" remote lock {0} successfully!", local_path);
                    }
                    else
                        Console.WriteLine(" faild to remote lock {0}!", local_path);
                }
                else if (cmd == "remoteunlock")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to unlock {0} ...", local_path);
                    var uri = client.GetUriFromWorkingCopy(local_path);
                    var r = client.RemoteUnlock(uri);
                    if (r)
                    {
                        Console.WriteLine(" remote unlock {0} successfully!", local_path);
                    }
                    else
                        Console.WriteLine(" faild to remote lock {0}!", local_path);
                }
                else if (cmd == "lock")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to lock {0} ...", local_path);
                    //var uri = client.GetUriFromWorkingCopy(local_path);
                    var r = client.Lock(local_path, String.Format("test lock {0:MM-dd HH:mm}", DateTime.Now));
                    if (r)
                    {
                        Console.WriteLine(" lock {0} successfully!", local_path);
                    }
                    else
                        Console.WriteLine(" faild to lock {0}!", local_path);
                }
                else if (cmd == "unlock")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    Console.WriteLine(" to unlock {0} ...", local_path);
                    //var uri = client.GetUriFromWorkingCopy(local_path);
                    var r = client.Unlock(local_path);
                    if (r)
                    {
                        Console.WriteLine(" unlock {0} successfully!", local_path);
                    }
                    else
                        Console.WriteLine(" faild to lock {0}!", local_path);
                }
                else if (cmd == "export")
                {
                    var local_path = String.Format(@"E:\SVN_BASE\{0}", cmd_param);
                    var dest_path = arr_cmd[2];
                    Console.WriteLine(" to unlock {0} ...", local_path);
                    var uri = client.GetUriFromWorkingCopy(local_path);
                    try
                    {
                        var r = client.Export(new SvnUriTarget(uri), dest_path);
                        if (r)
                        {
                            Console.WriteLine(" export {0} to {1} successfully!", uri, dest_path);
                        }
                        else
                            Console.WriteLine(" faild to export {0} to {1}!", uri, dest_path);
                    }
                    catch(SvnException svnEx)
                    {
                        Console.WriteLine(" {0}(1)", svnEx.Message, svnEx.SvnErrorCode);
                    }
                }
                else if (cmd == "help" || cmd == "?")
                {
                    Console.WriteLine(" 1.getinfo [localpath] or [null] or [remotepath]");
                    Console.WriteLine(" 2.getstate [localpath]");
                    Console.WriteLine(" 2.geturi [localpath]");
                    Console.WriteLine(" 3.checkout [localpath]");
                    Console.WriteLine(" 4.add [localpath]");
                    Console.WriteLine(" 5.commit [localpath]");
                    Console.WriteLine(" 6.update [localpath]");
                    Console.WriteLine(" 7.lock [localpath]");
                    Console.WriteLine(" 8.unlock [localpath]");
                    Console.WriteLine(" 9.remotelock [localpath]");
                    Console.WriteLine(" 10.remoteunlock [localpath]");
                    Console.WriteLine(" 10.export [localpath] [localpath]");
                }
            }
        }
    }
}
