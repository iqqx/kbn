using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using kaban.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace kaban.Controllers;

public class HomeController(Context context) : Controller
{
    private readonly Context _context = context;

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Onas()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    [Authorize, HttpPost]
    public IActionResult AddPlace()
    {
        var name = Request.Form["name"];

        if (name[0] is not null)
        {
            _context.Places.Add(new PlaceEntity() { Name = name });
            _context.SaveChanges();
        }

        return Redirect("Admin?entity=Places");
    }

    [HttpPost]
    public IActionResult RegisterQuestion()
    {
        var name = Request.Form["name"];
        var phone = Request.Form["phone"];
        var questionInput = Request.Form["question"];

        if (name[0] is not null && phone[0] is not null && questionInput[0] is not null)
        {
            var phoned = phone[0].Replace(" ", "");

            if (phoned.Length >= 10 && phoned.Length <= 12)
            {
                _context.Questions.Add(new QuestionEntity() { Name = name[0], Phone = phone[0], Date = DateTime.UtcNow, Question = questionInput[0] });
                _context.SaveChanges();
            }
        }

        return Redirect("/Contacts");
    }

    [HttpPost]
    public IActionResult RegisterCallback()
    {
        var name = Request.Form["name"];
        var phone = Request.Form["phone"];
        var placeInput = Request.Form["place"];

        if (name[0] is not null && phone[0] is not null && placeInput[0] is not null)
        {
            var phoned = phone[0].Replace(" ", "");

            if (phoned.Length >= 10 && phoned.Length <= 12)
            {
                var place = _context.Places.Where(x => x.Id.ToString() == placeInput[0] || x.Name == placeInput[0]).FirstOrDefault();

                if (place is not null)
                {
                    _context.Callbacks.Add(new CallbackEntity() { Name = name[0], Phone = phone[0], Date = DateTime.UtcNow, PlaceId = place.Id });
                    _context.SaveChanges();
                }
            }
        }

        return Redirect("/Zapis");
    }

    [Authorize, HttpPost]
    public IActionResult AddCallback()
    {
        var name = Request.Form["name"];
        var phone = Request.Form["phone"];
        var placeInput = Request.Form["place"];

        if (name[0] is not null && phone[0] is not null && placeInput[0] is not null)
        {
            if (phone[0].Length < 10 || phone[0].Length > 12)
                TempData["AddError"] = "Телефон не верный.";
            else
            {
                var place = _context.Places.Where(x => x.Id.ToString() == placeInput[0] || x.Name == placeInput[0]).FirstOrDefault();

                if (place is null)
                    TempData["AddError"] = "Место не найдено.";
                else
                {
                    _context.Callbacks.Add(new CallbackEntity() { Name = name[0], Phone = phone[0], Date = DateTime.UtcNow, PlaceId = place.Id });
                    _context.SaveChanges();
                }
            }
        }
        else TempData["AddError"] = "Данные не верны.";

        return Redirect("/Admin?entity=Callbacks");
    }

    [Authorize, HttpPost]
    public IActionResult OnDelete()
    {
        switch (Request.Headers.Referer[0][Request.Headers.Referer[0].LastIndexOf('=')..])
        {
            case "=Callbacks":
                {
                    var val = _context.Callbacks.Where(x => x.Id.ToString() == Request.Form["Id"][0]).FirstOrDefault();
                    if (val is not null)
                    {
                        _context.Callbacks.Remove(val);
                        _context.SaveChanges();
                    }

                    break;
                }
            case "=Places":
                {
                    var val = _context.Places.Where(x => x.Id.ToString() == Request.Form["Id"][0]).FirstOrDefault();
                    if (val is not null)
                    {
                        _context.Places.Remove(val);
                        _context.SaveChanges();
                    }

                    break;
                }
            case "=Questions":
                {
                    var val = _context.Questions.Where(x => x.Id.ToString() == Request.Form["Id"][0]).FirstOrDefault();
                    if (val is not null)
                    {
                        _context.Questions.Remove(val);
                        _context.SaveChanges();
                    }

                    break;
                }
        }

        return Redirect(Request.Headers.Referer[0] ?? "/Admin");
    }

    [Authorize]
    public IActionResult Admin(string? entity)
    {
        switch (entity)
        {
            case "Places":
                {
                    ViewBag.Name = "Места";
                    ViewBag.AddUrl = "AddPlace";
                    ViewBag.Headers = (dynamic[])[new { Name = "Номер", Input = (string?)null }, new { Name = "Название", Input = "name" }];
                    ViewBag.Values = _context.Places.Select(x => new string[] { x.Id.ToString(), x.Name }).ToArray();
                    break;
                }
            case "Callbacks":
                {
                    ViewBag.Name = "Заявки";
                    ViewBag.AddUrl = "AddCallback";
                    ViewBag.Headers = (dynamic[])[new { Name = "Номер" , Input= (string?)null},
                        new { Name = "Имя",  Input = "name" },
                        new { Name = "Телефон", Input = "phone" },
                        new { Name = "Место", Input = "place" },
                        new { Name = "Дата" , Input= (string?)null}
                        ];
                    ViewBag.Values = _context.Callbacks.Include(x => x.Place).Select(x => new string[] { x.Id.ToString(), x.Name, x.Phone, x.Place.Name, x.Date.ToString("HH:mm M MMM yyyy") }).ToArray();
                    break;
                }
            case "Questions":
                {
                    ViewBag.Name = "Вопросы";
                    ViewBag.Headers = (dynamic[])[new { Name = "Номер" , Input= (string?)null},
                        new { Name = "Имя",  Input = "name" },
                        new { Name = "Телефон", Input = "phone" },
                        new { Name = "Вопрос", Input = "question" },
                        new { Name = "Дата" , Input= (string?)null}
                      ];
                    ViewBag.Values = _context.Questions.Select(x => new string[] { x.Id.ToString(), x.Name, x.Phone, x.Question, x.Date.ToString("HH:mm M MMM yyyy") }).ToArray();
                    break;
                }
        }

        ViewBag.AddError = TempData["AddError"];

        return View();
    }

    public IActionResult Contacts()
    {
        return View();
    }

    public IActionResult Notfound()
    {
        return View();
    }

    public IActionResult D1t()
    {
        return View();
    }

    public IActionResult D2t()
    {
        return View();
    }

    public IActionResult Events()
    {
        return View();
    }

    public IActionResult Zapis()
    {
        ViewBag.Places = _context.Places.ToArray();

        return View();
    }

    public IActionResult Soveti()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return Redirect("/");
    }

    [HttpPost]
    public async Task<IActionResult> PostLogin()
    {
        var name = Request.Form["Login"];
        var pswd = Request.Form["Password"];

        if (pswd != "admin") return Redirect("/login");

        string username;
        string role;

        switch (name)
        {
            case "admin":
                {
                    role = "Administrator";
                    username = "Администратор (крадется)";

                    break;
                }
            case "doctor":
                {
                    role = "Doctor";
                    username = "Доктор (крадется)";

                    break;
                }
            case "patient":
                {
                    role = "Patient";
                    username = "Пациент (крадется)";

                    break;
                }
            default: return Redirect("/login");
        }

        var claimsIdentity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, role)
            ], "sessionAuth");

        HttpContext.Session.SetString("Username", username);
        await HttpContext.SignInAsync("session", new ClaimsPrincipal(claimsIdentity));

        return Redirect(Request.Form["ReturnUrl"]);
    }
}
