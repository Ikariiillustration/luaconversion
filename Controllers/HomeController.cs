using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using project2.Data;
using project2.Models;
using System.Text.RegularExpressions;
using System.Drawing;
using Microsoft.Ajax.Utilities;
using System.Web.Configuration;

namespace project2.Controllers
{
    public class HomeController : Controller
    {
        private project2Context db = new project2Context();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            Boolean isCompressedLua = false;
            string name = "";
            string channel = "";
            string finaldir = "";
            List<String> dir = new List<String>();
            Regex rx;

            string path = Server.MapPath("~/App_Data/File");
            string fileName = Path.GetFileName(file.FileName);
            string fullPath = Path.Combine(path, fileName);
            file.SaveAs(fullPath);

            //Check to decompile Lua

            if (file.FileName.Contains(".lua"))
            {
                isCompressedLua = true;
            }
            
            if (isCompressedLua == true)
            {
                var compile = true;
                compile= file.Equals(compile);
                Console.Write(compile);//decompile
            }

            //oh boy, here we go

            using (StreamReader reader = new StreamReader(fullPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.Contains("name =")) //retrieve name
                    {
                        var collection = Regex.Matches(line, "\\\"(.*?)\\\"");
                        foreach (var item in collection)
                        {
                            name = item.ToString().Trim('"');
                        }
                    }

                    if(line.Contains("channel =")) //retrieve channel
                    {
                        var collection = Regex.Matches(line, "=(.*?),");
                        foreach (var item in collection)
                        {
                            channel = item.ToString().Trim('=', ',');
                        }
                    }

                    if(line.Contains(".ogg") || line.Contains(".wav") || line.Contains(".mp3")) //source engine only supports these formats
                    {
                        rx = new Regex("\"([^\"]*)\"");
                        MatchCollection matches = rx.Matches(line);
                        foreach (Match match in matches)
                        {
                            dir.Add(match.Value);
                        }
                    }

                    if(line.Contains("} )")) //end of entry
                    {
                        string[] tempArray = dir.ToArray();
                        if (dir.Count == 1)
                        {
                            db.Entries.Add(
                                new Entry
                                {
                                    Name = name,
                                    Channel = channel,
                                    Directory = tempArray[0],
                                }
                                );
                            db.SaveChanges();
                        }
                        if (dir.Count > 1)
                        {
                            for(int i=0;i<tempArray.Length;i++)
                            {
                                finaldir = finaldir + dir[i] + " \r\n";
                            }
                            db.Entries.Add(
                                new Entry
                                {
                                    Name = name,
                                    Channel = channel,
                                    Directory = finaldir,
                                }
                                );

                            db.SaveChanges();
                        }
                        name = "";
                        channel = "";
                        finaldir = "";
                        dir = new List<String>();
                    }
                }
            }
            db.SaveChanges();
            var entries = from m in db.Entries
                          select m;

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            //delete file from server, what if they want 2 files that have the same name?

            return View(entries.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Welcome to the first Lua to Manifest conversion tool";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
