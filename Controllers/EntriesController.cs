using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using project2.Data;
using project2.Models;

namespace project2.Controllers
{
    public class EntriesController : Controller
    {
        private project2Context db = new project2Context();

        // GET: Entries
        public ActionResult Index(string searchString, string channelString, string directoryString)
        {
            if (db.Entries.ToList() == null)
            {
                return View();
            }

            var entries = from m in db.Entries
                          select m;


            if (!String.IsNullOrEmpty(searchString) && String.IsNullOrEmpty(channelString) && String.IsNullOrEmpty(directoryString))
            {
                entries = entries.Where(s => s.Name.Contains(searchString)); //Name
            }

            if (!String.IsNullOrEmpty(channelString) && String.IsNullOrEmpty(searchString) && String.IsNullOrEmpty(directoryString))
            {
                entries = entries.Where(s => s.Channel.Contains(channelString)); //Channel
            }

            if (!String.IsNullOrEmpty(channelString) && !String.IsNullOrEmpty(searchString) && String.IsNullOrEmpty(directoryString))
            {
                entries = entries.Where(s => s.Channel.Contains(channelString) && s.Name.Contains(searchString)); //Name and Channel
            }

            if (String.IsNullOrEmpty(channelString) && String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(directoryString))
            {
                entries = entries.Where(s => s.Directory.Contains(directoryString)); //Directory
            }

            if (!String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(directoryString) && String.IsNullOrEmpty(channelString))
            {
                entries = entries.Where(s => s.Name.Contains(searchString) && s.Directory.Contains(directoryString)); //Directory and Name
            }

            if (!String.IsNullOrEmpty(channelString) && !String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(directoryString))
            {
                entries = entries.Where(s => s.Directory.Contains(directoryString) && s.Channel.Contains(channelString) && s.Name.Contains(searchString)); // Directory Channel and Name
            }

            if (!String.IsNullOrEmpty(channelString) && String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(directoryString))
            {
                entries = entries.Where(s => s.Directory.Contains(directoryString) && s.Channel.Contains(channelString)); //Directory and Channel
            }

            return View(entries.ToList());
        }

        // GET: Entries/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Entry entry = db.Entries.Find(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            return View(entry);
        }

        // GET: Entries/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Entries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Channel,Directory")] Entry entry)
        {
            if (ModelState.IsValid)
            {
                db.Entries.Add(entry);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(entry);
        }

        // GET: Entries/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Entry entry = db.Entries.Find(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            return View(entry);
        }

        // POST: Entries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Channel,Directory")] Entry entry)
        {
            if (ModelState.IsValid)
            {
                db.Entry(entry).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(entry);
        }

        // GET: Entries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Entry entry = db.Entries.Find(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            return View(entry);
        }

        public ActionResult Clear()
        {
            foreach(Entry entry in db.Entries)
            {
                db.Entries.Remove(entry);
            }

            db.SaveChanges();

            return View("~/Views/Home/Index.cshtml");

        }

        //ok here we go, this is gonna take awhile
        public ActionResult Download()
        {
            string name = "";
            string channel = "";
            string written = "";
            string finaldir = "";
            List<String> dir = new List<string>();
            string path = Server.MapPath("~/App_Data/File/source_sound_manifest.txt");
            Regex rx;
            var stream = new StreamWriter(path, append: true);
            stream.Write("//Converted");
            foreach (Entry entry in db.Entries)
            {
                name = entry.Name;
                channel = entry.Channel;
                rx = new Regex("\"([^\"]*)\"");
                MatchCollection matches = rx.Matches(entry.Directory);
                foreach (Match match in matches)
                {
                    dir.Add(match.Value);
                }
                string[] tempArray = dir.ToArray();
                if (tempArray.Length == 1)
                {
                    written = "\n\n" + "\"" + name + "\"\n{\n    \"channel\"  \"" + channel + "\"\n    \"volume\"   \"0.7\"\n    \"soundlevel\"  \"SNDLVLNORM\"\n\n    \"wave\"    " + "\"" + tempArray[0].ToString().Trim('"') + "\"\n}";
                    stream.Write(written);
                }
                if (tempArray.Length > 1)
                {
                    for (int i = 0; i < tempArray.Length; i++)
                    {
                        finaldir = finaldir + "\n" + "          \"wave\" \"" + tempArray[i].ToString().Trim('"') + "\"";
                    }
                    written = "\n\n" + "\"" + name + "\"\n{\n    \"channel\"  \"" + channel + "\"\n    \"volume\"   \"0.7\"\n    \"soundlevel\"  \"SNDLVLNORM\"\n\n    \"rndwave\"\n         " + "{" + finaldir + "\n        }\n} \n \n";
                    stream.Write(written);
                }

                name = "";
                channel = "";
                written = "";
                finaldir = "";
                dir = new List<string>();
            }

            stream.Close();

            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.AddHeader("Content-Disposition", "attachment; filename=source_sound_manifest.manifest");
            Response.Flush();
            Response.TransmitFile(path);
            Response.End();

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            return View("~/Views/Home/Index.cshtml");

        }

        // POST: Entries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Entry entry = db.Entries.Find(id);
            db.Entries.Remove(entry);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
