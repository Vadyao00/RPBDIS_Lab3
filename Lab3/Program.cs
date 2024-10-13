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

            //app.UseStaticFiles();

            app.UseSession();

            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    string strResponse = "<HTML><HEAD><TITLE>����������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>����������:</H1>";
                    strResponse += "<BR> ������: " + context.Request.Host;
                    strResponse += "<BR> ����: " + context.Request.PathBase;
                    strResponse += "<BR> ��������: " + context.Request.Protocol;
                    strResponse += "<BR><A href='/'>�������</A></BODY></HTML>";

                    await context.Response.WriteAsync(strResponse);
                });
            });

            app.Map("/searchform1", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    Movie movie = context.Session.Get<Movie>("movie") ?? new Movie();

                    ICachedService<Actor> cachedActorsService = context.RequestServices.GetService<ICachedService<Actor>>();
                    List<Actor> actors = cachedActorsService.GetFromCache("Actors20").ToList();

                    ICachedService<Genre> cachedGenresService = context.RequestServices.GetService<ICachedService<Genre>>();
                    List<Genre> genres = cachedGenresService.GetFromCache("Genres20").ToList();

                    string strResponse = "<HTML><HEAD><TITLE>�����</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform1' method='GET'>" +
                    "�������� ������:<BR><INPUT type='text' name='Title' value='" + movie.Title + "'><BR>" +
                    "����:<BR><SELECT name='GenreId'>";
                    foreach (var genre in genres)
                    {
                        bool isSelected = genre.GenreId == movie.GenreId;
                        strResponse += $"<OPTION value='{genre.GenreId}' {(isSelected ? "selected" : "")}>{genre.Name}</OPTION>";
                    }
                    strResponse += "</SELECT><BR>" +
                    "������:<BR><SELECT name='Actors' multiple='multiple'>";
                    foreach (var actor in actors)
                    {
                        bool isSelected = movie.Actors.Any(a => a.ActorId == actor.ActorId);
                        strResponse += $"<OPTION value='{actor.ActorId}' {(isSelected ? "selected" : "")}>{actor.Name}</OPTION>";
                    }
                    strResponse += "</SELECT><BR>" +

                    "<BR><INPUT type='submit' value='��������� � Session'></FORM>";

                    strResponse += "<BR><A href='/'>�������</A></BODY></HTML>";

                    movie.Title = context.Request.Query["Title"];
                    if (Guid.TryParse(context.Request.Query["GenreId"], out Guid genreId))
                    {
                        movie.GenreId = genreId;
                    }

                    var selectedActorIds = context.Request.Query["Actors"].ToArray();
                    movie.Actors = actors.Where(actor => selectedActorIds.Contains(actor.ActorId.ToString())).ToList();

                    context.Session.Set<Movie>("movie", movie);

                    await context.Response.WriteAsync(strResponse);
                });
            });

            app.Map("/searchform2", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    Movie movie = new Movie();
                    if (context.Request.Cookies.ContainsKey("Title"))
                    {
                        movie.Title = context.Request.Cookies["Title"];
                    }
                    if (context.Request.Cookies.ContainsKey("GenreId") && Guid.TryParse(context.Request.Cookies["GenreId"], out Guid genreId))
                    {
                        movie.GenreId = genreId;
                    }
                    if (context.Request.Cookies.ContainsKey("Actors"))
                    {
                        string[] actorIds = context.Request.Cookies["Actors"].Split(',');
                        movie.Actors = actorIds.Select(id => new Actor { ActorId = Guid.Parse(id) }).ToList();
                    }

                    ICachedService<Actor> cachedActorsService = context.RequestServices.GetService<ICachedService<Actor>>();
                    List<Actor> actors = cachedActorsService.GetFromCache("Actors20").ToList();

                    ICachedService<Genre> cachedGenresService = context.RequestServices.GetService<ICachedService<Genre>>();
                    List<Genre> genres = cachedGenresService.GetFromCache("Genres20").ToList();

                    string strResponse = "<HTML><HEAD><TITLE>�����</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform2' method='GET'>" +
                    "�������� ������:<BR><INPUT type='text' name='Title' value='" + movie.Title + "'><BR>" +
                    "����:<BR><SELECT name='GenreId'>";
                    foreach (var genre in genres)
                    {
                        bool isSelected = genre.GenreId == movie.GenreId;
                        strResponse += $"<OPTION value='{genre.GenreId}' {(isSelected ? "selected" : "")}>{genre.Name}</OPTION>";
                    }
                    strResponse += "</SELECT><BR>" +
                    "������:<BR><SELECT name='Actors' multiple='multiple'>";
                    foreach (var actor in actors)
                    {
                        bool isSelected = movie.Actors.Any(a => a.ActorId == actor.ActorId);
                        strResponse += $"<OPTION value='{actor.ActorId}' {(isSelected ? "selected" : "")}>{actor.Name}</OPTION>";
                    }
                    strResponse += "</SELECT><BR>" +
                    "<BR><INPUT type='submit' value='��������� � Cookie'></FORM>";

                    strResponse += "<BR><A href='/'>�������</A></BODY></HTML>";

                    movie.Title = context.Request.Query["Title"];
                    if (Guid.TryParse(context.Request.Query["GenreId"], out genreId))
                    {
                        movie.GenreId = genreId;
                    }

                    var selectedActorIds = context.Request.Query["Actors"].ToArray();
                    movie.Actors = actors.Where(actor => selectedActorIds.Contains(actor.ActorId.ToString())).ToList();

                    context.Response.Cookies.Append("Title", string.IsNullOrEmpty(movie.Title)?"":movie.Title, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) });
                    context.Response.Cookies.Append("GenreId", string.IsNullOrEmpty(movie.GenreId.ToString())?"":movie.GenreId.ToString(), new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(1) });
                    context.Response.Cookies.Append("Actors", string.IsNullOrEmpty(string.Join(",", movie.Actors.Select(a => a.ActorId.ToString())))?"": string.Join(",", movie.Actors.Select(a => a.ActorId.ToString())), new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(1) });

                    await context.Response.WriteAsync(strResponse);
                });
            });

            app.Map("/genres", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    ICachedService<Genre> cachedGenresService = context.RequestServices.GetService<ICachedService<Genre>>();
                    IEnumerable<Genre> genres = cachedGenresService.GetFromCache("Genres20");

                    string HtmlString = "<HTML><HEAD><TITLE>�����</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ ������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "</TR>";
                    foreach (var genre in genres)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + genre.Name + "</TD>";
                        HtmlString += "<TD>" + genre.Description + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                    string HtmlString = "<HTML><HEAD><TITLE>������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ �������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "</TR>";
                    foreach (var actor in actors)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + actor.Name + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                    string HtmlString = "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ ����������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>���������</TH>";
                    HtmlString += "</TR>";
                    foreach (var employee in employees)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + employee.Name + "</TD>";
                        HtmlString += "<TD>" + employee.Role + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                    string HtmlString = "<HTML><HEAD><TITLE>�����������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ �����������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>����</TH>";
                    HtmlString += "<TH>����� ������</TH>";
                    HtmlString += "<TH>����� ���������</TH>";
                    HtmlString += "<TH>���� ������</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                    string HtmlString = "<HTML><HEAD><TITLE>������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ �������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>������������</TH>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>������</TH>";
                    HtmlString += "<TH>���������� �����������</TH>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>����</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                    string HtmlString = "<HTML><HEAD><TITLE>�����</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ ����</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>�����</TH>";
                    HtmlString += "<TH>�����������</TH>";
                    HtmlString += "<TH>����� �����</TH>";
                    HtmlString += "<TH>������</TH>";
                    HtmlString += "</TR>";
                    foreach (var seat in seats)
                    {
                        string isOc = seat.IsOccupied == true ? "������" : "��������";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                    string HtmlString = "<HTML><HEAD><TITLE>������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ �������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>����</TH>";
                    HtmlString += "<TH>����� ������</TH>";
                    HtmlString += "<TH>����� ���������</TH>";
                    HtmlString += "<TH>���� ������</TH>";
                    HtmlString += "<TH>�����</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                    string HtmlString = "<HTML><HEAD><TITLE>������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ �������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>����� �����</TH>";
                    HtmlString += "<TH>����</TH>";
                    HtmlString += "</TR>";
                    foreach (var ticket in tickets)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + ticket.Seat.SeatNumber + "</TD>";
                        HtmlString += "<TD>" + ticket.PurchaseDate + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                    string HtmlString = "<HTML><HEAD><TITLE>������� ����������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ ��������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>���� ������</TH>";
                    HtmlString += "<TH>�����</TH>";
                    HtmlString += "<TH>���������� �����</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
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

                string HtmlString = "<HTML><HEAD><TITLE>�������</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>�������</H1>";
                HtmlString += "<H2>������ �������� � ��� �������</H2>";
                HtmlString += "<BR><A href='/'>�������</A></BR>";
                HtmlString += "<BR><A href='/info'>����������</A></BR>";
                HtmlString += "<BR><BR><A href='/genres'>�����</A></BR>";
                HtmlString += "<BR><A href='/actors'>������</A></BR>";
                HtmlString += "<BR><A href='/employees'>���������</A></BR>";
                HtmlString += "<BR><A href='/events'>�����������</A></BR>";
                HtmlString += "<BR><A href='/movies'>������</A></BR>";
                HtmlString += "<BR><A href='/seats'>�����</A></BR>";
                HtmlString += "<BR><A href='/showtimes'>������</A></BR>";
                HtmlString += "<BR><A href='/tickets'>������</A></BR>";
                HtmlString += "<BR><A href='/workLogs'>������� ����������</A></BR>";
                HtmlString += "<BR><BR><A href='/searchform1'>����� 1</A></BR>";
                HtmlString += "<BR><A href='/searchform2'>����� 2</A></BR>";
                HtmlString += "</BODY></HTML>";

                await context.Response.WriteAsync(HtmlString);
            });

            app.Run();
        }
    }
}
