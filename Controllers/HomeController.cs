using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using kaban.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using Microsoft.AspNetCore.StaticFiles;

namespace kaban.Controllers;

public class HomeController(Context context) : Controller
{
    private readonly Context _context = context;

    public IActionResult Index()
    {
        ViewBag.AddInfo = TempData["AddInfo"];
        TempData["AddInfo"] = null;

        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult ImageFile(string imageId)
    {
        var file = $"{Directory.GetCurrentDirectory()}/uploads/{imageId}";

        if (!Path.Exists(file))
            return NotFound();

        FileStream fileStream = new(file, FileMode.Open);

        var cd = new ContentDisposition
        {
            FileName = file,
            Inline = true,
        };

        Response.Headers.Append("Content-Disposition", cd.ToString());
        return File(fileStream, new FileExtensionContentTypeProvider().TryGetContentType(file, out string? contentPath) ? contentPath : "image/png");
    }

    [Authorize, HttpPost]
    public IActionResult AddPlace()
    {
        var name = Request.Form["name"];
        IFormFile? image = null;

        if (string.IsNullOrWhiteSpace(name[0]))
        {
            TempData["AddError"] = "Имя не указано.";

            return Redirect("Admin?entity=Places");
        }

        if (_context.Places.Any(x => x.Name == name[0]))
        {
            TempData["AddError"] = "Такое мероприятие уже существует.";

            return Redirect("Admin?entity=Places");
        }

        foreach (var file in Request.Form.Files)
        {
            image = file;
            break;
        }

        if (image is null && string.IsNullOrEmpty(Request.Form["url-image"][0]))
        {
            TempData["AddError"] = "Изображение не указано.";

            return Redirect("Admin?entity=Places");
        }

        string imageUrl;
        if (image is not null)
        {
            string imageName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(image.FileName);

            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/uploads/"))
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/uploads");

            using FileStream stream = new($"{Directory.GetCurrentDirectory()}/uploads/{imageName}", FileMode.Create);
            image.CopyTo(stream);

            imageUrl = $"/imagefile?imageId={imageName}";
        }
        else imageUrl = Request.Form["url-image"][0]!.Trim();

        _context.Places.Add(new PlaceEntity() { Name = name[0]!, ImageId = imageUrl });
        _context.SaveChanges();

        return Redirect("Admin?entity=Places");
    }

    [HttpPost]
    public IActionResult RegisterQuestion()
    {
        var name = Request.Form["name"].FirstOrDefault();
        var phone = Request.Form["email"].FirstOrDefault();
        var questionInput = Request.Form["question"].FirstOrDefault();

        if (name is not null && phone is not null && questionInput is not null)
        {
            _context.Questions.Add(new QuestionEntity() { Name = name, Phone = phone, Date = DateTime.UtcNow, Question = questionInput });
            _context.SaveChanges();
        }

        TempData["AddInfo"] = "Заявка успешно отправлена.";

        return Redirect(Request.Form["ReturnUrl"].FirstOrDefault() ?? Request.Headers.Referer.FirstOrDefault() ?? "/Contacts");
    }

    [Authorize, HttpPost]
    public IActionResult AnswerToQuestion()
    {
        var id = Request.Form["id"];
        var answer = Request.Form["answer"];

        if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(answer) && int.TryParse(id, out int questionId))
        {
            var question = _context.Questions.Where(x => x.Id == questionId && x.Answered == false).FirstOrDefault();

            if (question is not null)
            {
                question.Answered = true;
                _context.SaveChanges();

                TempData["AnswerIsSuccess"] = true;
                TempData["AnswerMessage"] = $"Ответ успешно отправлен на почту: '{question.Phone}'";
            }
            else
            {
                TempData["AnswerIsSuccess"] = false;
                TempData["AnswerMessage"] = "Вопрос не найден в базе данных.";
            }
        }
        else
        {
            TempData["AnswerIsSuccess"] = false;
            TempData["AnswerMessage"] = "Данные не корректны.";
        }

        return Redirect("/Admin?Entity=Questions");
    }

    [HttpPost]
    public IActionResult RegisterCallback()
    {
        var name = Request.Form["name"].FirstOrDefault();
        var phone = Request.Form["email"].FirstOrDefault();
        var placeInput = Request.Form["place"].FirstOrDefault();

        if (name is not null && phone is not null && placeInput is not null)
        {
            var place = _context.Places.Where(x => x.Id.ToString() == placeInput || x.Name == placeInput).FirstOrDefault();

            if (place is not null)
            {
                _context.Callbacks.Add(new CallbackEntity() { Name = name, Phone = phone, Date = DateTime.UtcNow, PlaceId = place.Id });
                _context.SaveChanges();
            }
        }

        TempData["AddInfo"] = "Запись успешно отправлена.";

        return Redirect(Request.Form["ReturnUrl"].FirstOrDefault() ?? Request.Headers.Referer.FirstOrDefault() ?? "/Zapis");
    }

    [Authorize, HttpPost]
    public IActionResult AddCallback()
    {
        var name = Request.Form["name"].FirstOrDefault();
        var phone = Request.Form["phone"].FirstOrDefault();
        var placeInput = Request.Form["place"].FirstOrDefault();

        if (name is not null && phone is not null && placeInput is not null)
        {
            if (phone.Length < 10 || phone.Length > 12)
                TempData["AddError"] = "Телефон не верный.";
            else
            {
                var place = _context.Places.Where(x => x.Id.ToString() == placeInput || x.Name == placeInput).FirstOrDefault();

                if (place is null)
                    TempData["AddError"] = "Место не найдено.";
                else
                {
                    _context.Callbacks.Add(new CallbackEntity() { Name = name, Phone = phone, Date = DateTime.UtcNow, PlaceId = place.Id });
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
        var referer = Request.Headers.Referer.FirstOrDefault();

        if (referer is null)
            return Redirect("/Admin");

        switch (referer[referer.LastIndexOf('=')..])
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

        return Redirect(referer ?? "/Admin");
    }

    [Authorize]
    public IActionResult Admin(string? entity)
    {
        switch (entity)
        {
            case "Places":
                {
                    ViewBag.Name = "Мероприятия";
                    ViewBag.AddUrl = "AddPlace";
                    ViewBag.Headers = (dynamic[])[new { Name = "Номер", Input = (string?)null }, new { Name = "Название", Input = "name", Type = (string?)null }, new { Name = "Изображение", Input = "image", Type = "img" }];
                    ViewBag.Values = _context.Places.Select(x => new string[] { x.Id.ToString(), x.Name, $"{x.ImageId}|{(x.ImageId.StartsWith('/') ? x.ImageId.Substring(x.ImageId.LastIndexOf('=') + 1) : x.ImageId.Substring(x.ImageId.LastIndexOf('/') + 1))}" }).ToArray();
                    break;
                }
            case "Callbacks":
                {
                    ViewBag.Name = "Заявки";
                    ViewBag.AddUrl = "AddCallback";
                    ViewBag.Headers = (dynamic[])[new { Name = "Номер" , Input= (string?)null, Type=(string?)null},
                        new { Name = "Имя",  Input = "name", Type=(string?)null },
                        new { Name = "Почта", Input = "phone", Type=(string?)null },
                        new { Name = "Место", Input = "place", Type=(string?)null },
                        new { Name = "Дата" , Input= (string?)null, Type=(string?)null}
                        ];
                    ViewBag.Values = _context.Callbacks.Include(x => x.Place).Select(x => new string[] { x.Id.ToString(), x.Name, x.Phone, x.Place.Name, x.Date.ToLocalTime().ToString("HH:mm dd MMM yyyy") }).ToArray();
                    break;
                }
            case "Questions":
                {
                    ViewBag.Name = "Вопросы";
                    ViewBag.AnswerUrl = "AnswerToQuestion";
                    ViewBag.Headers = (dynamic[])[new { Name = "Номер" , Input= (string?)null, Type=(string?)null},
                        new { Name = "Имя",  Input = "name", Type=(string?)null },
                        new { Name = "Почта", Input = "phone", Type=(string?)null },
                        new { Name = "Вопрос", Input = "question" , Type=(string?)null},
                        new { Name = "Дата" , Input= (string?)null, Type=(string?)null}
                      ];
                    ViewBag.Values = _context.Questions.Where(x => !x.Answered).Select(x => new string[] { x.Id.ToString(), x.Name, x.Phone, x.Question, x.Date.ToLocalTime().ToString("HH:mm dd MMM yyyy") }).ToArray();

                    ViewBag.AnswerIsSuccess = TempData["AnswerIsSuccess"] ?? true;
                    ViewBag.AnswerMessage = TempData["AnswerMessage"] ?? "";

                    TempData["AnswerIsSuccess"] = null;
                    TempData["AnswerMessage"] = null;

                    break;
                }
        }

        ViewBag.AddError = TempData["AddError"];

        return View();
    }

    public IActionResult Contacts()
    {
        ViewBag.AddInfo = TempData["AddInfo"];
        TempData["AddInfo"] = null;

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
        ViewBag.Events = _context.Places.Select(x => new { x.Name, Image = x.ImageId }).ToArray();

        return View();
    }

    public IActionResult Zapis()
    {
        ViewBag.AddInfo = TempData["AddInfo"];
        ViewBag.Places = _context.Places.ToArray();

        TempData["AddInfo"] = null;

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

        if (pswd != "admin1") return Redirect("/login");

        string username;
        string role;

        switch (name)
        {
            case "admin":
                {
                    role = "Administrator";
                    username = "Администратор";

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

        return Redirect(Request.Form["ReturnUrl"][0] ?? "/");
    }
}
