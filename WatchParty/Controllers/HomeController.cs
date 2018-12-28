using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using WatchParty.Database;
using WatchParty.Models;

namespace WatchParty.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public HomeController(DatabaseContext context)
        {
            _databaseContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string url, string name, bool join)
        {
            if (string.IsNullOrEmpty(name))
            {
                TempData["Errors"] = "Enter a valid room name";
                return View();
            }

            name = name.Trim();

            try
            {
                if (join)
                {
                    var room = _databaseContext.Rooms.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
                    if (room == null)
                    {
                        TempData["Errors"] = "Room with the specified name not found";
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("RoomByName",new {name});
                    }
                }

                if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url))
                {
                    var hasRoom = _databaseContext.Rooms.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
                    if (hasRoom != null)
                    {
                        TempData["Errors"] = "A room with such a name has already been created";
                        return View();
                    }

                    var newRoom = new Room {Name = name};
                    return View(newRoom);
                }

                var request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        var responseString = reader.ReadToEnd();

                        var rx = new Regex(@"(?<=<iframe[^>]*?)(?:\s*width=[""'](?<width>[^""']+)[""']|\s*height=[""'](?<height>[^'""]+)[""']|\s*src=[""'](?<src>[^'""]+[""']))+[^>]*?>");
                        var input = responseString;
                        var matches = rx.Matches(input).Cast<Match>().ToList();
                        var math = matches.FirstOrDefault();
                        if (math != null)
                        {
                            var iFrame = $"<iframe {math.Value}</iframe>";
                            ViewBag.Url = url;

                            var newRoom = new Room
                            {
                                Url = url,
                                Iframe = iFrame,
                                Name = name
                            };
                            _databaseContext.Rooms.Add(newRoom);
                            _databaseContext.SaveChanges();
                            return RedirectToAction("RoomByName", new { name = newRoom.Name });
                        }
                        else
                        {
                            TempData["Errors"] = "On the page will not find iframe with the broadcast";
                            return View();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TempData["Errors"] = "On the page will not find iframe with the broadcast";
                return View();
            }
        }

        public IActionResult Room(Guid id)
        {
            var room = _databaseContext.Rooms.FirstOrDefault(x => x.Id == id);
            if (room == null)
            {
                TempData["Errors"] = "The specified room does not exist. Go to another or enter the address of your broadcast.";
                return RedirectToAction("Index");
            }

            return View(room);
        }

        public IActionResult RoomByName(string name)
        {
            var room = _databaseContext.Rooms.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (room == null)
            {
                TempData["Errors"] = "The specified room does not exist. Go to another or enter the address of your broadcast.";
                return RedirectToAction("Index");
            }

            return View("Room", room);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
