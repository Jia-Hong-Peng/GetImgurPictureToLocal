using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetPictureLocal
{
    class Program
    {
        private static string FIND_START = @"![](http://i.imgur.com/";
        private static string FIND_END = @"g)";
        private static string strWebProxy = "twty3tmg01.delta.corp:8080";


        static void Main(string[] args)
        {
            string InitialDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) ;
            string[] dirs = Directory.GetDirectories(InitialDirectory);
            Console.WriteLine("Get Directories count: " + dirs.Length.ToString());
            if (dirs.Length > 0)
            {                
                foreach (var dir in dirs)
                {
                    if (dir.IndexOf(".github.io") >= 0)
                    {
                        Console.WriteLine("Found Directories: " + dir);
                        string[] files = Directory.GetFiles(dir + @"\_posts", "*.md");
                        Console.WriteLine("Get Files count: " + files.Length.ToString());
                        if (files.Length > 0)
                        {                            
                            foreach (var file in files)
                            {
                                Console.WriteLine("Found File: " + file);
                                string[] lines = System.IO.File.ReadAllLines(file);
                                Console.WriteLine("Get Lines count: " + lines.Length.ToString());
                                if (lines.Length > 0)
                                {
                                    string[] strNewFile = new string[lines.Length];
                                    int i = 0;
                                    bool bIsChange = false;
                                    foreach (var line in lines)
                                    {
                                        int iStart = line.IndexOf(FIND_START, StringComparison.OrdinalIgnoreCase);
                                        int iEnd = line.IndexOf(FIND_END, StringComparison.OrdinalIgnoreCase);
                                        if (iStart >= 0 && iEnd >= 0)
                                        {
                                            string strPicUrl = line.Substring(iStart, iEnd + (FIND_END.Length - 1));
                                            strPicUrl = strPicUrl.Replace("![](", "");
                                            string strFileName = strPicUrl.Replace(@"http://i.imgur.com/", "");
                                            Console.WriteLine("Get PicUrl: " + strPicUrl);

                                            string strTargetPath = dir + @"\images\" + Path.GetFileName(file).Replace(".md", "");
                                            string strTargetFile = dir + @"\images\" + Path.GetFileName(file).Replace(".md", "") + @"\" + strFileName;
                                            Console.WriteLine("Target Path: " + strTargetPath);
                                            if (!Directory.Exists(strTargetPath)) Directory.CreateDirectory(strTargetPath);

                                            GetImageFromUrl(strPicUrl, strTargetFile);

                                            strNewFile[i] = line.Replace(@"![](" + strPicUrl + ")", @"![](" + @"..\images\" + Path.GetFileName(file).Replace(".md", "") + @"\" + strFileName + ")");
                                            bIsChange = true;
                                        }
                                        else
                                        {
                                            strNewFile[i] = line;
                                        }

                                        i++;
                                    }

                                    if (bIsChange)
                                    {
                                        System.IO.File.WriteAllLines(file, strNewFile);
                                        strNewFile = null;
                                        bIsChange = false;
                                    }
                                     
                                }                               
                            }
                        }
                    }
                }            
            }
            
        }


        public static void GetImageFromUrl(string url, string path)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.ServicePoint.Expect100Continue = false;
            request.Method = "GET";
            request.KeepAlive = true;

            request.ContentType = "image/png";

            WebProxy proxyObj = new WebProxy(strWebProxy);
            proxyObj.Credentials = CredentialCache.DefaultCredentials;
            request.Proxy = proxyObj;


            // Send the request to the Internet resource and wait for the response.    
            using (HttpWebResponse rsp = (HttpWebResponse)request.GetResponse())
            {
                System.IO.Stream stream = null;
                try
                {
                    using (stream = rsp.GetResponseStream())
                    {
                        Image.FromStream(stream).Save(path);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception: {0}", e);
                }
                finally
                {
                    if (stream != null) stream.Close();
                    if (rsp != null) rsp.Close();
                }
            }
        }
    }
}
