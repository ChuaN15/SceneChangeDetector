using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SceneChangeDetector.Controllers
{
    public class HomeController : Controller
    {
        SceneDetectorEntities ent = new SceneDetectorEntities();
        string data = "";

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Search(string search)
        {
            List<VIDEO> videolist = ent.VIDEOs.Where(x => x.Name.ToString().Contains(search)).ToList();

            return View(videolist);
        }

        [HttpGet]
        public ActionResult Index(string message)
        {
            List<VIDEO> videolist = ent.VIDEOs.OrderByDescending(x => x.ViewCount).ToList();

            if(message != null)
            {
                ViewBag.Message = message;
            }

            return View(videolist);
        }

        [HttpPost]
        public ActionResult UploadVideo(HttpPostedFileBase fileupload)
        {
            if (fileupload != null)
            {
                string fileName = Path.GetFileName(fileupload.FileName);
                int fileSize = fileupload.ContentLength;
                int Size = fileSize / 1000;
                fileupload.SaveAs(Server.MapPath("~/Controllers/VideoFileUpload/" + fileName));

                VIDEO video = new VIDEO();
                video.Name = fileName;
                video.FileSize = fileSize;
                video.FilePath = "~/Controllers/VideoFileUpload/" + fileName;
                video.UploadDateTime = DateTime.Now;
                ent.VIDEOs.Add(video);
                ent.SaveChanges();

            }
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult RegisterVideo(string message)
        {
            if (message != null)
            {
                ViewBag.Message = message;
            }

            return View();
        }

        public ActionResult EditVideo(int id)
        {
            Session["VideoId2"] = id;
            var whichvideo = ent.VIDEOs.FirstOrDefault(x => x.ID == id);

            return View(whichvideo);
        }

        public ActionResult DeleteVideo(int id)
        {
            Session["VideoId2"] = id;
            var whichvideo = ent.VIDEOs.FirstOrDefault(x => x.ID == id);
            ent.VIDEOs.Remove(whichvideo);
            ent.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditVideo(VIDEO video2, HttpPostedFileBase UserPhoto, HttpPostedFileBase fileupload)
        {

            var whichVideo = ent.VIDEOs.FirstOrDefault(x => x.ID == video2.ID);

            whichVideo.Name = video2.Name;
            ent.SaveChanges();

            whichVideo.UploadDateTime = DateTime.Now;
            ent.SaveChanges();

            if (fileupload != null)
            {
                string fileName = Path.GetFileName(fileupload.FileName);
                int fileSize = fileupload.ContentLength;
                int Size = fileSize / 1000;
                fileupload.SaveAs(Server.MapPath("~/Controllers/VideoFileUpload/" + fileName));

                whichVideo.FileSize = fileSize;
                whichVideo.FilePath = "~/Controllers/VideoFileUpload/" + fileName;



                if (UserPhoto != null)
                {
                    MemoryStream target = new MemoryStream();
                    UserPhoto.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();
                    whichVideo.CoverImage = data;
                }

                ent.SaveChanges();

            }
            return RedirectToAction("Index");
        }

        public ActionResult ViewVideo(int id)
        {
            Session["VideoId"] = id;
            var whichvideo = ent.VIDEOs.FirstOrDefault(x => x.ID == id);
            whichvideo.ViewCount = whichvideo.ViewCount + 1;
            whichvideo.FilePath = @"~/Controllers/VideoFileUpload/" + whichvideo.ID.ToString() + ".mp4";
            ent.SaveChanges();

            var allvideo = ent.VIDEOs.ToList();

            List<string> dataList = new List<string>();

            DirectoryInfo di = new DirectoryInfo(@"C:\Users\chuan\source\repos\SceneChangeDetector\SceneChangeDetector\Controllers\VideoFileUpload\" + whichvideo.ID);
            FileInfo[] files = di.GetFiles("*.mp4");

            foreach (var item in files)
            {
                dataList.Add(item.Name);
            }

            return View(dataList);
        }

        public FileResult downloadfile(int id)
        {
            var whichvideo = ent.VIDEOs.FirstOrDefault(x => x.ID == id);
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"C:\Users\chuan\source\repos\SceneChangeDetector\SceneChangeDetector\Controllers\VideoFileUpload\" + whichvideo.ID.ToString() + @"\" + whichvideo.ID.ToString() + "-Scenes.csv");
            string fileName = whichvideo.Name + "-Scenes.csv";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterVideo(VIDEO video2, HttpPostedFileBase UserPhoto, HttpPostedFileBase fileupload)
        {

            if (fileupload != null && string.IsNullOrEmpty(video2.Name) && string.IsNullOrEmpty(video2.Email))
            {
                VIDEO video = new VIDEO();
                video.Name = video2.Name;
                video.Email = video2.Email;

                string fileName = Path.GetFileName(fileupload.FileName);
                int fileSize = fileupload.ContentLength;
                int Size = fileSize / 1000;

                video.FileSize = fileSize;
                video.FilePath = "~/Controllers/VideoFileUpload/" + video.ID.ToString() + ".mp4";
                video.UploadDateTime = DateTime.Now;
                video.ViewCount = 0;
                ent.VIDEOs.Add(video);

                if (UserPhoto != null)
                {
                    MemoryStream target = new MemoryStream();
                    UserPhoto.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();
                    video.CoverImage = data;
                }


                ent.SaveChanges();

                string msg = "Your video are in processing. You will receive an email once the process is done.";
                Response.Write("<script>alert('" + msg + "')</script>");



                fileupload.SaveAs(Server.MapPath("~/Controllers/VideoFileUpload/" + video.ID.ToString() + ".mp4"));

                string subdir = @"C:\Users\chuan\source\repos\SceneChangeDetector\SceneChangeDetector\Controllers\VideoFileUpload\" + video.ID.ToString();
                Directory.CreateDirectory(subdir);

                var filepath = subdir + "/" + video.ID.ToString() + "-Scenes.csv";
                using (StreamWriter writer = new StreamWriter(new FileStream(filepath,
                FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine("");
                }


                string path = @"C:\Program Files\Python36\Scripts\output_dir\" + video.ID.ToString() + ".txt";
                if (!System.IO.File.Exists(path))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = System.IO.File.CreateText(path))
                    {
                        sw.WriteLine("@echo off");
                        sw.WriteLine(@"cd C:\Program Files\Python36\Scripts");
                        sw.WriteLine(@"scenedetect -i C:\Users\chuan\source\repos\SceneChangeDetector\SceneChangeDetector\Controllers\VideoFileUpload\" + video.ID.ToString() + @".mp4 -o C:\Users\chuan\source\repos\SceneChangeDetector\SceneChangeDetector\Controllers\VideoFileUpload\" + video.ID.ToString() + " detect-content -t 27 list-scenes split-video");
                        sw.WriteLine(@"SET GmailAccount =% ~1");
                        sw.WriteLine(@"SET GmailPassword =% ~2");
                        sw.WriteLine(@"SET TargetAccount =% ~3");
                        sw.WriteLine(@"SET PowerShellDir = C:\Windows\System32\WindowsPowerShell\v1.0");
                        sw.WriteLine(@"CD /D "" % PowerShellDir % """);
                        sw.WriteLine(@"Powershell -ExecutionPolicy Bypass -Command "" & 'C:\ChuaN\Degree\Degree Sem4\Multimedia Database\Stage2 & 3\SendEmail.ps1' 'chuanyou1997@gmail.com' 'ccy04050' " + video.Email  + @" 'Dear user, Your video has been processed into seperated scenes. Click on the link to access it. Link:" + " http://7aee1956700c.ngrok.io/Home/ViewVideo/" + video.ID.ToString() + "'");
                        sw.WriteLine(@"pause");
                    }
                }

                // Open the file to read from.
                using (StreamReader sr = System.IO.File.OpenText(path))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(s);
                    }
                }

                System.IO.File.Move(path, @"C:\Program Files\Python36\Scripts\output_dir\" + video.ID.ToString() + ".bat");

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = @"C:\Program Files\Python36\Scripts\output_dir\" + video.ID.ToString() + ".bat";
                psi.Verb = "runas";
                Process.Start(psi);

                return RedirectToAction("Index", "Home", new { message = "Your video is under processing, an email will be send to you once the process is done." });

            }
            else
            {
                return RedirectToAction("RegisterVideo", "Home", new { message = "Please fill in all of the fields." });

            }
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public FileContentResult GetUserImage(int id)
        {
            if (id.ToString() != null)
            {
                int userId = id;

                var userImage = ent.VIDEOs.Where(x => x.ID == userId).FirstOrDefault();

                if (userImage.CoverImage == null)
                {
                    string fileName = HttpContext.Server.MapPath(@"~/Images/noImg.png");


                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);


                    var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                    ffMpeg.GetVideoThumbnail(userImage.FilePath, fs, 1);


                    return File(ReadToEnd(fs), "image/png");
                }

                return new FileContentResult(userImage.CoverImage, "image/jpeg");
            }
            else
            {


                string fileName = HttpContext.Server.MapPath(@"~/Images/noImg.png");

                byte[] imageData = null;
                FileInfo fileInfo = new FileInfo(fileName);
                long imageFileLength = fileInfo.Length;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                imageData = br.ReadBytes((int)imageFileLength);
                return File(imageData, "image/png");

            }
        }

    }
}