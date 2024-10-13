using Data.Lab3;
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

            app.UseStaticFiles();

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

            //app.Map("/form", (appBuilder) =>
            //{
            //    appBuilder.Run(async (context) =>
            //    {
            //        Genre genre = context.Session.Get<Genre>("genre") ?? new Genre();

            //        string strResponse = "<HTML><HEAD><TITLE>Жанр</TITLE></HEAD>" +
            //        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
            //        "<BODY><FORM action ='/form' / >" +
            //        "Название жанра:<BR><INPUT type = 'text' name = 'Name' value = " + genre.Name + ">" +
            //        "<BR>Описание:<BR><INPUT type = 'text' name = 'Description' value = " + genre.Description + " >" +
            //        "<BR><BR><INPUT type ='submit' value='Сохранить в Session'><INPUT type ='submit' value='Показать'></FORM>";
            //        strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";

            //        genre.Name = context.Request.Query["Name"];
            //        genre.Description = context.Request.Query["Description"];
            //        context.Session.Set<Genre>("genre", genre);

            //        await context.Response.WriteAsync(strResponse);
            //    });
            //});

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
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
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
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
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
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
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
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
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
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
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
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
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
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
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
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
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
                HtmlString += "<BR><A href='/genres'>Жанры</A></BR>";
                HtmlString += "<BR><A href='/actors'>Актеры</A></BR>";
                HtmlString += "<BR><A href='/employees'>Работники</A></BR>";
                HtmlString += "<BR><A href='/events'>Мероприятия</A></BR>";
                HtmlString += "<BR><A href='/movies'>Фильмы</A></BR>";
                HtmlString += "<BR><A href='/seats'>Места</A></BR>";
                HtmlString += "<BR><A href='/showtimes'>Сеансы</A></BR>";
                HtmlString += "<BR><A href='/tickets'>Билеты</A></BR>";
                HtmlString += "<BR><A href='/workLogs'>Журналы работников</A></BR>";
                HtmlString += "<BR><A href='/searchform1'>Форма 1</A></BR>";
                HtmlString += "<BR><A href='/searchform2'>Форма 2</A></BR>";
                HtmlString += "</BODY></HTML>";

                await context.Response.WriteAsync(HtmlString);
            });

            app.Run();
        }
    }
}
