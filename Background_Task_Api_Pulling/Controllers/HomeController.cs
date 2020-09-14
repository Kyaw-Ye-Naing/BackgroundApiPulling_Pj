using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Background_Task_Api_Pulling.Models;
using RestSharp;
using Hangfire;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp.Extensions;
using System.Reflection.PortableExecutable;

namespace Background_Task_Api_Pulling.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Gambling_AppContext db;
        public HomeController(ILogger<HomeController> logger, Gambling_AppContext _db)
        {
            _logger = logger;
            db = _db;
        }


        public IActionResult Index()
        {
            //Background Job that calling privacy function Hourly
            RecurringJob.AddOrUpdate(() => Privacy4(), Cron.Minutely);
            // RecurringJob.AddOrUpdate(() => Privacy2(), Cron.Minutely);
            // RecurringJob.AddOrUpdate(() => Privacy(), Cron.Minutely);

            return View();
        }

        public void Privacy4()
        {
            //Filtering with today date
            var eventId = db.TblUpcomingEvent.Where(a => a.EventDate == DateTime.Today).Select(b => b.RapidEventId).ToList();
            foreach (var c in eventId)
            {
                var ftValue = db.TblGoalResult.ToList().Any(a => Convert.ToInt32(a.RapidEventId) == Convert.ToInt32(c));
                if (ftValue == false)
                {
                    //Record not exist
                    //Data fetch from API
                    var eventString = String.Concat("https://bet365-sports-odds.p.rapidapi.com/v1/bet365/result?event_id=", c.ToString());
                    var client = new RestClient(eventString);
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("x-rapidapi-host", "bet365-sports-odds.p.rapidapi.com");
                    request.AddHeader("x-rapidapi-key", "ffafc278d4msh3c49958fb5ea1b8p175067jsn9dae909c7509");
                    IRestResponse response = client.Execute(request);
                    GoalResult data = JsonConvert.DeserializeObject<GoalResult>(response.Content);

                    //Add data into goal result table
                    TblGoalResult goalResult = new TblGoalResult();
                    goalResult.RapidEventId = c.ToString();
                    goalResult.HomeResult = Convert.ToInt32(data.results[0].ss.Remove(1, 2));
                    goalResult.AwayResult = Convert.ToInt32(data.results[0].ss.Remove(0, 2));
                    db.TblGoalResult.Add(goalResult);
                    db.SaveChanges();
                }
                else
                {
                    //Record Existing
                }


            }


        }


        public void Privacy2()
        {
            Console.WriteLine("This is task 2222222222222");
            Console.WriteLine("This is task 2222222222222");
            Console.WriteLine("This is task 2222222222222");
            Console.WriteLine("This is task 2222222222222");
            Console.WriteLine("This is task 2222222222222");


        }
        public void Privacy()
        {
            Console.WriteLine("This is task 33333333333");
            Console.WriteLine("This is task 33333333333");
            Console.WriteLine("This is task 33333333333");
            Console.WriteLine("This is task 33333333333");
            Console.WriteLine("This is task 33333333333");


        }

        //Background Function
        public void Privacy3()
        {
            //Calling data from RapidApI 
            var client = new RestClient("https://bet365-sports-odds.p.rapidapi.com/v1/bet365/upcoming?page=1&LNG_ID=1&sport_id=1");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "bet365-sports-odds.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "ffafc278d4msh3c49958fb5ea1b8p175067jsn9dae909c7509");
            IRestResponse response = client.Execute(request);
            Football data = JsonConvert.DeserializeObject<Football>(response.Content);

            //Get eventId of first array result
            var eventId = data.results[0].id;

            //Get total result from api data and calculate page
            var total = data.pager.total;
            int data_page = total / 50;
            int con_page = data_page + 1;
            for (var page = 1; page <= con_page; page++)
            {
                //Calling data from RapidApI 
                var yy = String.Concat("https://bet365-sports-odds.p.rapidapi.com/v1/bet365/upcoming?page=", page, "&LNG_ID=1&sport_id=1");
                var client1 = new RestClient(yy);
                var request1 = new RestRequest(Method.GET);
                request1.AddHeader("x-rapidapi-host", "bet365-sports-odds.p.rapidapi.com");
                request1.AddHeader("x-rapidapi-key", "ffafc278d4msh3c49958fb5ea1b8p175067jsn9dae909c7509");
                IRestResponse response1 = client1.Execute(request1);
                Football data1 = JsonConvert.DeserializeObject<Football>(response1.Content);



                for (var ii = 0; ii <= data.results.Length; ii++)
                {
                    eventId = data1.results[ii].id;
                    var yyy = String.Concat("https://bet365-sports-odds.p.rapidapi.com/v3/bet365/prematch?FI=", eventId);
                    var client2 = new RestClient(yyy);
                    var request2 = new RestRequest(Method.GET);
                    request2.AddHeader("x-rapidapi-host", "bet365-sports-odds.p.rapidapi.com");
                    request2.AddHeader("x-rapidapi-key", "ffafc278d4msh3c49958fb5ea1b8p175067jsn9dae909c7509");
                    IRestResponse response2 = client2.Execute(request2);
                    Odds data2 = JsonConvert.DeserializeObject<Odds>(response2.Content);

                    //Adding into leageue table
                    TblLeague lg = new TblLeague();
                    var league_name = data1.results[ii].league.name;
                    //  var lg_cou = db.TblLeague.Where(a => a.LeagueName.Equals(league_name)).FirstOrDefault().LeagueName;
                    //  int lg_count = lg_cou.Count();
                    var lgValue = db.TblLeague.ToList().Any(a => a.LeagueName == league_name);
                    if (lgValue == false)
                    {
                        //Record not present
                        lg.LeagueName = league_name;
                        decimal decRapid = Decimal.Parse(data1.results[ii].league.id);
                        lg.RapidLeagueId = decRapid;
                        db.TblLeague.Add(lg);
                        db.SaveChanges();
                    }
                    else
                    {
                        //Record is present
                    }

                    //Adding away team into football team table
                    TblFootballTeam ft = new TblFootballTeam();
                    var awayTeam = data1.results[ii].away.name;
                    var lgId = data1.results[ii].league.id;
                    var ft_lgId = db.TblLeague.Where(a => a.RapidLeagueId.ToString().Equals(lgId)).FirstOrDefault().LeagueId;
                    var ftValue = db.TblFootballTeam.ToList().Any(a => a.FootballTeam == awayTeam);
                    if (ftValue == false)
                    {
                        //no record
                        ft.FootballTeam = awayTeam;
                        decimal decAway = Decimal.Parse(data1.results[ii].away.id);
                        ft.RapidTeamId = decAway;
                        ft.LeagueId = ft_lgId;
                        ft.CreatedDate = DateTime.Now;
                        db.TblFootballTeam.Add(ft);
                        db.SaveChanges();

                    }
                    else
                    {
                        //record existing

                    }

                    //Adding home team into football team table
                    TblFootballTeam home_ft = new TblFootballTeam();
                    var homeTeam = data1.results[ii].home.name;
                    var ftValue1 = db.TblFootballTeam.ToList().Any(a => a.FootballTeam == homeTeam);
                    if (ftValue1 == false)
                    {
                        //no record
                        home_ft.FootballTeam = homeTeam;
                        decimal decHome = Decimal.Parse(data1.results[ii].home.id);
                        home_ft.RapidTeamId = decHome;
                        home_ft.LeagueId = ft_lgId;
                        home_ft.CreatedDate = DateTime.Now;
                        db.TblFootballTeam.Add(home_ft);
                        db.SaveChanges();

                    }
                    else
                    {
                        //record existing

                    }

                    //Adding preUpcoming table 
                    TblPreUpcomingEvent pre = new TblPreUpcomingEvent();
                    decimal decPre = Decimal.Parse(data2.results[0].event_id);
                    pre.RapidEventId = decPre;
                    var pre_lgId = db.TblLeague.Where(a => a.LeagueName == league_name).FirstOrDefault().LeagueId;
                    var pre_home = db.TblFootballTeam.Where(a => a.FootballTeam == homeTeam).FirstOrDefault().FootballTeamId;
                    var pre_away = db.TblFootballTeam.Where(a => a.FootballTeam == awayTeam).FirstOrDefault().FootballTeamId;
                    //Change timestamp to local time
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    var time_stamp = Convert.ToDouble(data1.results[ii].time);
                    dtDateTime = dtDateTime.AddSeconds(time_stamp).ToLocalTime();
                    var shortdate = dtDateTime.ToShortDateString();
                    var shorttime = dtDateTime.ToShortTimeString();
                    pre.LeagueId = pre_lgId;
                    pre.HomeTeamId = pre_home;
                    pre.AwayTeamId = pre_away;
                    pre.EventDate = DateTime.Parse(shortdate);
                    pre.EventTime = DateTime.Parse(shorttime);
                    db.TblPreUpcomingEvent.Add(pre);
                    db.SaveChanges();

                    //Adding upcomingevent table
                    TblUpcomingEvent up = new TblUpcomingEvent();
                    decimal decUp = Decimal.Parse(data1.results[ii].id);
                    var up_lgId = db.TblLeague.Where(a => a.LeagueName == league_name).FirstOrDefault().LeagueId;
                    var up_home = db.TblFootballTeam.Where(a => a.FootballTeam == homeTeam).FirstOrDefault().FootballTeamId;
                    var up_away = db.TblFootballTeam.Where(a => a.FootballTeam == awayTeam).FirstOrDefault().FootballTeamId;
                    up.RapidEventId = decUp;
                    up.LeagueId = up_lgId;
                    up.HomeTeamId = up_home;
                    up.AwayTeamId = up_away;
                    up.EventDate = DateTime.Parse(shortdate);
                    up.EventTime = DateTime.Parse(shorttime);
                    db.TblUpcomingEvent.Add(up);
                    db.SaveChanges();

                    //Adding handicap table
                    TblHandicap hd = new TblHandicap();
                    decimal decHd = Decimal.Parse(data2.results[0].event_id);
                    decimal dec_home_odds = Decimal.Parse(data2.results[0].asian_lines.sp.asian_handicap.odds[0].odds);
                    decimal dec_away_odds = Decimal.Parse(data2.results[0].asian_lines.sp.asian_handicap.odds[1].odds);
                    String value = data2.results[0].asian_lines.sp.asian_handicap.odds[0].name;
                    String value_h = data2.results[0].asian_lines.sp.asian_handicap.odds[1].name;
                    if (value.Length <= 4 && value_h.Length <= 4)
                    {
                        hd.RapidEventId = decHd;
                        hd.HomeOdd = dec_home_odds;
                        hd.HomeHandicap = Decimal.Parse(value_h);
                        hd.AwayOdd = dec_away_odds;
                        hd.AwayHandicap = Decimal.Parse(value);
                        hd.EventDatetime = dtDateTime;
                        db.TblHandicap.Add(hd);
                        db.SaveChanges();
                    }
                    else
                    {
                        var str = value.Remove(0, 4);
                        var str_h = value_h.Remove(0, 4);
                        hd.RapidEventId = decHd;
                        hd.HomeOdd = dec_home_odds;
                        hd.HomeHandicap = Decimal.Parse(str_h);
                        hd.AwayOdd = dec_away_odds;
                        hd.AwayHandicap = Decimal.Parse(str);
                        hd.EventDatetime = dtDateTime;
                        db.TblHandicap.Add(hd);
                        db.SaveChanges();
                    }

                }

            }
            //return View();
        }

        public void Privacy11()
        {

            var client = new RestClient("https://bet365-sports-odds.p.rapidapi.com/v1/bet365/upcoming?page=1&LNG_ID=1&sport_id=1");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "bet365-sports-odds.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "ffafc278d4msh3c49958fb5ea1b8p175067jsn9dae909c7509");
            IRestResponse response = client.Execute(request);
            Football data = JsonConvert.DeserializeObject<Football>(response.Content);
            var time_stamp = Convert.ToDouble(data.results[0].time);

            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(time_stamp).ToLocalTime();
            var shortdate = dtDateTime.ToShortDateString();
            var shorttime = dtDateTime.ToShortTimeString();
            //var printDate = dtDateTime.Date;
            // DateTime dddd = dtDateTime.ToLocalTime();
            // var ererer = Convert.DateTime(dddd);
            // DateTime dddd=dtDateTime
            // DateTime  dt = Convert.ToDateTime(time.ToString());
            //var time1 = Convert.ToByte(time);

            TblUpcomingEvent tt = new TblUpcomingEvent();
            tt.RapidEventId = 342322;
            tt.LeagueId = 4;
            tt.HomeTeamId = 3;
            tt.AwayTeamId = 2;

            tt.EventDate = DateTime.Parse(shortdate);
            tt.EventTime = DateTime.Parse(shorttime);
            db.TblUpcomingEvent.Add(tt);
            db.SaveChanges();



        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
