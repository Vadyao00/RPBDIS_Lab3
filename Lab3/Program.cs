using Data.Lab3;
using Lab3.Infrastructure;
using Lab3.Models;
using Lab3.Services;

namespace Lab3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;

            services.AddDbContext<CinemaContext>();

            services.AddMemoryCache();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddScoped<ICachedService<Actor>, CachedActorsService>();
            services.AddScoped<ICachedService<Employee>, CachedEmployeesService>();
            services.AddScoped<ICachedService<Event>, CachedEventsService>();
            services.AddScoped<ICachedService<Genre>, CachedGenresService>();
            services.AddScoped<ICachedService<Movie>, CachedMoviesService>();
            services.AddScoped<ICachedService<Seat>, CachedSeatsService>();
            services.AddScoped<ICachedService<Showtime>, CachedShowtimesService>();
            services.AddScoped<ICachedService<Ticket>, CachedTicketsService>();
            services.AddScoped<ICachedService<WorkLog>, CachedWorkLogsService>();

            var app = builder.Build();

            app.UseSession();

            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    string strResponse = "<HTML><HEAD><TITLE>Информация</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Информация:</H1>";
                    strResponse += "<BR> Сервер: " + context.Request.Host;
                    strResponse += "<BR> Путь: " + context.Request.PathBase;
                    strResponse += "<BR> Протокол: " + context.Request.Protocol;
                    strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";

                    await context.Response.WriteAsync(strResponse);
                });
            });

            app.Map("/searchform1", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    Movie movie = context.Session.Get<Movie>("movie") ?? new Movie();

                    ICachedService<Actor> cachedActorsService = context.RequestServices.GetService<ICachedService<Actor>>();
                    List<Actor> actors20 = cachedActorsService.GetFromCache("Actors20").ToList();
                    List<Actor> actors = cachedActorsService.GetAll().ToList();
                    
                    ICachedService<Genre> cachedGenresService = context.RequestServices.GetService<ICachedService<Genre>>();
                    List<Genre> genres = cachedGenresService.GetFromCache("Genres20").ToList();

                    string strResponse = "<HTML><HEAD><TITLE>Фильм</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform1' method='GET'>" +
                    "Возрастное ограничение:<BR><INPUT type='text' name='AgeRestriction' value='" + movie.AgeRestriction + "'><BR>" +
                    //"Жанр:<BR><SELECT name='GenreId'>";
                    //foreach (var genre in genres)
                    //{
                    //    bool isSelected = genre.GenreId == movie.GenreId;
                    //    strResponse += $"<OPTION value='{genre.GenreId}' {(isSelected ? "selected" : "")}>{genre.Name}</OPTION>";
                    //}
                    //strResponse += "</SELECT><BR>" +
                    "Актеры:<BR><SELECT name='Actors' multiple='multiple'>";
                    foreach (var actor in actors20)
                    {
                        bool isSelected = movie.Actors.Any(a => a.ActorId == actor.ActorId);
                        strResponse += $"<OPTION value='{actor.ActorId}' {(isSelected ? "selected" : "")}>{actor.Name}</OPTION>";
                    }
                    strResponse += "</SELECT><BR>" +

                    "<BR><INPUT type='submit' value='Сохранить в Session и найти в базе данных'></FORM>";

                    strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";

             

                    if (!string.IsNullOrEmpty(context.Request.Query["AgeRestriction"]))
                        movie.AgeRestriction = int.Parse(context.Request.Query["AgeRestriction"]);
                    else 
                        movie.AgeRestriction = 0;
                    if (Guid.TryParse(context.Request.Query["GenreId"], out Guid genreId))
                    {
                        movie.GenreId = genreId;
                    }

                    var selectedActorIds = context.Request.Query["Actors"].ToArray();
                    movie.Actors = actors20.Where(actor => selectedActorIds.Contains(actor.ActorId.ToString())).ToList();

                    List<Movie> results = new List<Movie>();
                    if (movie != default(Movie))
                    {
                        foreach (var p in movie.Actors)
                        {
                            results.AddRange(p.Movies);
                        }
                        results.Distinct();
                        results = results.Where(m => m.AgeRestriction == movie.AgeRestriction).ToList();
                        //results = results.Where(m => m.GenreId == movie.GenreId).ToList();
                    }

                    var html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Поиск из бд</title></head><body>";
                    html += "<h1>Найденные результаты</h1>";

                    if (results.Count > 0)
                    {
                        html += "<table border='1' style='border-collapse:collapse'>";
                        html += "<tr><th>Название</th><th>Возрастное ограничение</th><th>Описание</th><th>Актеры</th></tr>";
                        foreach (var mov in results)
                        {
                            html += "<tr>";
                            html += $"<td>{mov.Title}</td>";
                            html += $"<td>{mov.AgeRestriction}</td>";
                            html += $"<td>{mov.Description}</td>";
                            html += $"<td>{string.Join(", ", mov.Actors.Select(a => a.Name))}</td>";
                            html += "</tr>";
                        }
                        html += "</table>";
                    }
                    else
                    {
                        html += "<p>Ничего не найдено.</p>";
                    }
                    html += "</body></html>";

                    context.Session.Set<Movie>("movie", movie);

                    await context.Response.WriteAsync(strResponse + html);
                });
            });

            app.Map("/searchform2", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    Movie movie = new Movie();

                    if (context.Request.Cookies.ContainsKey("AgeRestriction"))
                    {
                        movie.AgeRestriction = int.Parse(context.Request.Cookies["AgeRestriction"]);
                    }

                    if (context.Request.Cookies.ContainsKey("Actors"))
                    {
                        string[] actorIds = context.Request.Cookies["Actors"].Split(',');
                        movie.Actors = actorIds.Select(id => new Actor { ActorId = Guid.Parse(id) }).ToList();
                    }

                    ICachedService<Actor> cachedActorsService = context.RequestServices.GetService<ICachedService<Actor>>();
                    List<Actor> actors20 = cachedActorsService.GetFromCache("Actors20").ToList();
                    List<Actor> actors = cachedActorsService.GetAll().ToList();

                    ICachedService<Genre> cachedGenresService = context.RequestServices.GetService<ICachedService<Genre>>();
                    List<Genre> genres = cachedGenresService.GetFromCache("Genres20").ToList();

                    string strResponse = "<HTML><HEAD><TITLE>Фильм</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform2' method='GET'>" +
                    "Возрастное ограничение:<BR><INPUT type='text' name='AgeRestriction' value='" + movie.AgeRestriction + "'><BR>" +
                    //"Жанр:<BR><SELECT name='GenreId'>";
                    //foreach (var genre in genres)
                    //{
                    //    bool isSelected = genre.GenreId == movie.GenreId;
                    //    strResponse += $"<OPTION value='{genre.GenreId}' {(isSelected ? "selected" : "")}>{genre.Name}</OPTION>";
                    //}
                    //strResponse += "</SELECT><BR>" +
                    "Актеры:<BR><SELECT name='Actors' multiple='multiple'>";
                    foreach (var actor in actors20)
                    {
                        bool isSelected = movie.Actors.Any(a => a.ActorId == actor.ActorId);
                        strResponse += $"<OPTION value='{actor.ActorId}' {(isSelected ? "selected" : "")}>{actor.Name}</OPTION>";
                    }
                    strResponse += "</SELECT><BR>" +

                    "<BR><INPUT type='submit' value='Сохранить в куки и найти в базе данных'></FORM>";

                    strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";

                    // Обработка данных из формы
                    if (!string.IsNullOrEmpty(context.Request.Query["AgeRestriction"]))
                    {
                        movie.AgeRestriction = int.Parse(context.Request.Query["AgeRestriction"]);
                    }

                    if (Guid.TryParse(context.Request.Query["GenreId"], out Guid selectedGenreId))
                    {
                        movie.GenreId = selectedGenreId;
                    }

                    var selectedActorIds = context.Request.Query["Actors"].ToArray();
                    movie.Actors = actors20.Where(actor => selectedActorIds.Contains(actor.ActorId.ToString())).ToList();

                    List<Movie> results = new List<Movie>();
                    if (movie != default(Movie))
                    {
                        foreach (var p in movie.Actors)
                        {
                            results.AddRange(p.Movies);
                        }
                        results = results.Distinct().Where(m => m.AgeRestriction == movie.AgeRestriction).ToList();
                        //results = results.Where(m => m.GenreId == movie.GenreId).ToList();
                    }

                    var html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Поиск из бд</title></head><body>";
                    html += "<h1>Найденные результаты</h1>";

                    if (results.Count > 0)
                    {
                        html += "<table border='1' style='border-collapse:collapse'>";
                        html += "<tr><th>Название</th><th>Возрастное ограничение</th><th>Описание</th><th>Актеры</th></tr>";
                        foreach (var mov in results)
                        {
                            html += "<tr>";
                            html += $"<td>{mov.Title}</td>";
                            html += $"<td>{mov.AgeRestriction}</td>";
                            html += $"<td>{mov.Description}</td>";
                            html += $"<td>{string.Join(", ", mov.Actors.Select(a => a.Name))}</td>";
                            html += "</tr>";
                        }
                        html += "</table>";
                    }
                    else
                    {
                        html += "<p>Ничего не найдено.</p>";
                    }
                    html += "</body></html>";

                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddSeconds(266) 
                    };

                    context.Response.Cookies.Append("AgeRestriction", movie.AgeRestriction.ToString(), cookieOptions);
                    context.Response.Cookies.Append("Actors", string.Join(",", movie.Actors.Select(a => a.ActorId.ToString())), cookieOptions);

                    await context.Response.WriteAsync(strResponse + html);
                });
            });

            app.Map("/genres", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Genre> cachedGenresService = context.RequestServices.GetService<ICachedService<Genre>>();
                    IEnumerable<Genre> genres = cachedGenresService.GetFromCache("Genres20");

                    string HtmlString = "<HTML><HEAD><TITLE>Жанры</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список жанров</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Название</TH>";
                    HtmlString += "<TH>Описание</TH>";
                    HtmlString += "</TR>";
                    foreach (var genre in genres)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + genre.Name + "</TD>";
                        HtmlString += "<TD>" + genre.Description + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/actors", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Actor> cachedActorsService = context.RequestServices.GetService<ICachedService<Actor>>();
                    IEnumerable<Actor> actors = cachedActorsService.GetFromCache("Actors20");

                    string HtmlString = "<HTML><HEAD><TITLE>Актеры</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список актеров</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Имя</TH>";
                    HtmlString += "</TR>";
                    foreach (var actor in actors)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + actor.Name + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/employees", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Employee> cachedEmployeesService = context.RequestServices.GetService<ICachedService<Employee>>();
                    IEnumerable<Employee> employees = cachedEmployeesService.GetFromCache("Employees20");

                    string HtmlString = "<HTML><HEAD><TITLE>Работники</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список работников</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Имя</TH>";
                    HtmlString += "<TH>Должность</TH>";
                    HtmlString += "</TR>";
                    foreach (var employee in employees)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + employee.Name + "</TD>";
                        HtmlString += "<TD>" + employee.Role + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/events", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Event> cachedEventsService = context.RequestServices.GetService<ICachedService<Event>>();
                    IEnumerable<Event> events = cachedEventsService.GetFromCache("Events20");

                    string HtmlString = "<HTML><HEAD><TITLE>Мероприятия</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список мероприятий</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Название</TH>";
                    HtmlString += "<TH>Дата</TH>";
                    HtmlString += "<TH>Время начала</TH>";
                    HtmlString += "<TH>Время окончания</TH>";
                    HtmlString += "<TH>Цена билета</TH>";
                    HtmlString += "</TR>";
                    foreach (var ev in events)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + ev.Name + "</TD>";
                        HtmlString += "<TD>" + ev.Date + "</TD>";
                        HtmlString += "<TD>" + ev.StartTime + "</TD>";
                        HtmlString += "<TD>" + ev.EndTime + "</TD>";
                        HtmlString += "<TD>" + ev.TicketPrice + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/movies", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Movie> cachedMoviesService = context.RequestServices.GetService<ICachedService<Movie>>();
                    IEnumerable<Movie> movies = cachedMoviesService.GetFromCache("Movies20");

                    string HtmlString = "<HTML><HEAD><TITLE>Фильмы</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список фильмов</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Название</TH>";
                    HtmlString += "<TH>Длительность</TH>";
                    HtmlString += "<TH>Компания</TH>";
                    HtmlString += "<TH>Страна</TH>";
                    HtmlString += "<TH>Возрастное ограничение</TH>";
                    HtmlString += "<TH>Описание</TH>";
                    HtmlString += "<TH>Жанр</TH>";
                    HtmlString += "</TR>";
                    foreach (var movie in movies)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + movie.Title + "</TD>";
                        HtmlString += "<TD>" + movie.Duration + "</TD>";
                        HtmlString += "<TD>" + movie.ProductionCompany + "</TD>";
                        HtmlString += "<TD>" + movie.Country + "</TD>";
                        HtmlString += "<TD>" + movie.AgeRestriction + "</TD>";
                        HtmlString += "<TD>" + movie.Description + "</TD>";
                        HtmlString += "<TD>" + movie.Genre.Name + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/seats", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Seat> cachedSeatsService = context.RequestServices.GetService<ICachedService<Seat>>();
                    IEnumerable<Seat> seats = cachedSeatsService.GetFromCache("Seats20");

                    ICachedService<Movie> cachedMoviesService = context.RequestServices.GetService<ICachedService<Movie>>();
                    IEnumerable<Movie> movies = cachedMoviesService.GetFromCache("Movies20");

                    string HtmlString = "<HTML><HEAD><TITLE>Места</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список мест</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Сеанс</TH>";
                    HtmlString += "<TH>Мероприятие</TH>";
                    HtmlString += "<TH>Номер места</TH>";
                    HtmlString += "<TH>Занято</TH>";
                    HtmlString += "</TR>";
                    foreach (var seat in seats)
                    {
                        string isOc = seat.IsOccupied == true ? "Занято" : "Свободно";
                        string ev = "-";
                        string shw = "-";
                        if (seat.Event != null)
                            ev = seat.Event.Name;
                        else
                            shw = seat.Showtime.Movie.Title;
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + shw + "</TD>";
                        HtmlString += "<TD>" + ev + "</TD>";
                        HtmlString += "<TD>" + seat.SeatNumber + "</TD>";
                        HtmlString += "<TD>" + isOc + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/showtimes", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Showtime> cachedShowtimesService = context.RequestServices.GetService<ICachedService<Showtime>>();
                    IEnumerable<Showtime> showtimes = cachedShowtimesService.GetFromCache("Showtimes20");

                    string HtmlString = "<HTML><HEAD><TITLE>Сеансы</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список сеансов</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Дата</TH>";
                    HtmlString += "<TH>Время начала</TH>";
                    HtmlString += "<TH>Время окончания</TH>";
                    HtmlString += "<TH>Цена билета</TH>";
                    HtmlString += "<TH>Фильм</TH>";
                    HtmlString += "</TR>";
                    foreach (var showtime in showtimes)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + showtime.Date + "</TD>";
                        HtmlString += "<TD>" + showtime.StartTime + "</TD>";
                        HtmlString += "<TD>" + showtime.EndTime + "</TD>";
                        HtmlString += "<TD>" + showtime.TicketPrice + "</TD>";
                        HtmlString += "<TD>" + showtime.Movie.Title + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/tickets", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Ticket> cachedTicketsService = context.RequestServices.GetService<ICachedService<Ticket>>();
                    IEnumerable<Ticket> tickets = cachedTicketsService.GetFromCache("Tickets20");

                    string HtmlString = "<HTML><HEAD><TITLE>Билеты</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список билетов</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Номер места</TH>";
                    HtmlString += "<TH>Дата</TH>";
                    HtmlString += "</TR>";
                    foreach (var ticket in tickets)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + ticket.Seat.SeatNumber + "</TD>";
                        HtmlString += "<TD>" + ticket.PurchaseDate + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/workLogs", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<WorkLog> cachedWorkLogsService = context.RequestServices.GetService<ICachedService<WorkLog>>();
                    IEnumerable<WorkLog> workLogs = cachedWorkLogsService.GetFromCache("Tickets20");

                    string HtmlString = "<HTML><HEAD><TITLE>Журналы работников</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список журналов</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>Работник</TH>";
                    HtmlString += "<TH>Стаж работы</TH>";
                    HtmlString += "<TH>Месяц</TH>";
                    HtmlString += "<TH>Количество часов</TH>";
                    HtmlString += "</TR>";
                    foreach (var workLog in workLogs)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + workLog.Employee.Name + "</TD>";
                        HtmlString += "<TD>" + workLog.WorkExperience + "</TD>";
                        HtmlString += "<TD>" + workLog.StartDate + "</TD>";
                        HtmlString += "<TD>" + workLog.WorkHours + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Run(async (context) =>
            {
                ICachedService<Genre> cachedGenresService = context.RequestServices.GetService<ICachedService<Genre>>();
                ICachedService<Actor> cachedActorsService = context.RequestServices.GetService<ICachedService<Actor>>();
                ICachedService<Employee> cachedEmployeesService = context.RequestServices.GetService<ICachedService<Employee>>();
                ICachedService<Event> cachedEventsService = context.RequestServices.GetService<ICachedService<Event>>();
                ICachedService<Movie> cachedMoviesService = context.RequestServices.GetService<ICachedService<Movie>>();
                ICachedService<Seat> cachedSeatsService = context.RequestServices.GetService<ICachedService<Seat>>();
                ICachedService<Showtime> cachedShowtimesService = context.RequestServices.GetService<ICachedService<Showtime>>();
                ICachedService<Ticket> cachedTicketsService = context.RequestServices.GetService<ICachedService<Ticket>>();
                ICachedService<WorkLog> cachedWorkLogsService = context.RequestServices.GetService<ICachedService<WorkLog>>();

                cachedGenresService.AddIntoCache("Genres20");
                cachedActorsService.AddIntoCache("Actors20");
                cachedEmployeesService.AddIntoCache("Employees20");
                cachedEventsService.AddIntoCache("Events20");
                cachedMoviesService.AddIntoCache("Movies20");
                cachedSeatsService.AddIntoCache("Seats20");
                cachedShowtimesService.AddIntoCache("Showtimes20");
                cachedTicketsService.AddIntoCache("Tickets20");
                cachedWorkLogsService.AddIntoCache("WorkLogs20");

                string HtmlString = "<HTML><HEAD><TITLE>Главная</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>Главная</H1>";
                HtmlString += "<H2>Данные записаны в кэш сервера</H2>";
                HtmlString += "<BR><A href='/'>Главная</A></BR>";
                HtmlString += "<BR><A href='/info'>Информация</A></BR>";
                HtmlString += "<BR><BR><A href='/genres'>Жанры</A></BR>";
                HtmlString += "<BR><A href='/actors'>Актеры</A></BR>";
                HtmlString += "<BR><A href='/employees'>Работники</A></BR>";
                HtmlString += "<BR><A href='/events'>Мероприятия</A></BR>";
                HtmlString += "<BR><A href='/movies'>Фильмы</A></BR>";
                HtmlString += "<BR><A href='/seats'>Места</A></BR>";
                HtmlString += "<BR><A href='/showtimes'>Сеансы</A></BR>";
                HtmlString += "<BR><A href='/tickets'>Билеты</A></BR>";
                HtmlString += "<BR><A href='/workLogs'>Журналы работников</A></BR>";
                HtmlString += "<BR><BR><A href='/searchform1'>Форма 1</A></BR>";
                HtmlString += "<BR><A href='/searchform2'>Форма 2</A></BR>";
                HtmlString += "</BODY></HTML>";

                await context.Response.WriteAsync(HtmlString);
            });

            app.Run();
        }
    }
}
