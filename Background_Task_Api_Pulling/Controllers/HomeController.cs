using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Background_Task_Api_Pulling.Models;
using RestSharp;
using Newtonsoft.Json;
using Hangfire;

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
            //RecurringJob.AddOrUpdate(() => GetGoals(), Cron.Hourly);
           // RecurringJob.AddOrUpdate(() => GetData(),Cron.Minutely);
          //  RecurringJob.AddOrUpdate(() => CalculateHomeHandicap(), Cron.Hourly);
          //  RecurringJob.AddOrUpdate(() => CalculateAwayHandicap(), Cron.Hourly);
          //  RecurringJob.AddOrUpdate(() => CalculateZeroHandicap(), Cron.Hourly);
            return View();
        }

        //----------------------------------------Main Background Methods------------------------ 

        // Calculation Myanmar Handicap from home handicap
        public IActionResult CalculateHomeHandicap()
        {
            string bodyResult = "";
            string goalResult = "";
            var person = (from p in db.TblPreUpcomingEvent
                          join e in db.TblHandicap
                        on p.RapidEventId equals e.RapidEventId
                          where e.HomeHandicap.Contains("-")
                          select new
                          {
                              ID = p.RapidEventId,
                              homeHandicap = e.HomeHandicap,
                              awayHandicap = e.AwayHandicap,
                              homeOdds = e.HomeOdd,
                              awayOdds = e.AwayOdd,
                              homeTeam = p.HomeTeamId,
                              awayTeam = p.AwayTeamId,
                              league = p.LeagueId,
                              goalHandicap = e.GoalHandicap,
                              goalOverOdds = e.OverOdd,
                              goalUnderOdds = e.UnderOdd
                          }).ToList();
            foreach (var s in person)
            {
                TblMyanHandicapResult myanHandicapResult = new TblMyanHandicapResult();
                
                //Calculate body handicap
                var oddsResult = (decimal)(s.homeOdds - s.awayOdds);
                var intResult = (int)(oddsResult * 100);
                var unit = Round(intResult);
                decimal hanDecimal = Decimal.Parse(s.homeHandicap);
                string hanString = Math.Abs(hanDecimal).ToString();
                var goalUnit = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().GoalUnit;
                var unitAmount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().UnitAmount;
                var unitAmountInt = Convert.ToInt32(unitAmount);
                int unitAmountResult = unitAmountInt + unit;
                var positiveNumber = Math.Abs(unitAmountResult);

                if (positiveNumber < 100)
                {
                    if (goalUnit == 0)
                    {
                        bodyResult = Draw(unitAmountResult);

                    }
                    else
                    {
                        bodyResult = Body(unitAmountResult, (int)goalUnit);
                    }
                }
                else if (positiveNumber == 100)
                {
                    if (goalUnit == 0)
                    {
                        bodyResult = "=" + "-";
                    }
                    else
                    {
                        bodyResult = goalUnit.ToString() + "-";
                    }

                }
                else
                {
                    int newGoalUnit = (int)goalUnit + 1;
                    int newUnitAmountResult = 200 - positiveNumber;
                    bodyResult = Body(newUnitAmountResult, newGoalUnit);
                }

                //Calculate goal handicap
                var goalsOdds = (decimal)(s.goalOverOdds - s.goalUnderOdds);
                var goalsOddsInt = (int)(goalsOdds * 100);
                var goalsUnit = Round(goalsOddsInt);
                decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                string goalsString = Math.Abs(goalsDecimal).ToString();
                var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                var amountInt = Convert.ToInt32(amount);
                int amountResult = amountInt + goalsUnit;
                var positiveAmount = Math.Abs(amountResult);

                if (positiveAmount < 100)
                {
                    goalResult = Body(amountResult, (int)goals);
                }
                else if (positiveAmount == 100)
                {
                    goalResult = goalUnit.ToString() + "-";

                }
                else
                {
                    int newGoals = (int)goalUnit + 1;
                    int newAmountResult = 200 - positiveAmount;
                    goalResult = Body(newAmountResult, newGoals);
                }

                //Save into database
                var value = db.TblMyanHandicapResult.ToList().Any(a => Convert.ToInt32(a.RapidEventId) == Convert.ToInt32(s.ID));
                if (value == false)
                {
                    //Save if data not exists
                    myanHandicapResult.Body = bodyResult;
                    myanHandicapResult.Goal = goalResult;
                    myanHandicapResult.OverTeamId = s.homeTeam;
                    myanHandicapResult.UnderTeamId = s.awayTeam;
                    myanHandicapResult.HomeTeamId = s.homeTeam;
                    myanHandicapResult.AwayTeamId = s.awayTeam;
                    myanHandicapResult.LeagueId = s.league;
                    myanHandicapResult.RapidEventId = s.ID;
                    db.TblMyanHandicapResult.Add(myanHandicapResult);
                    db.SaveChanges();
                }
                else
                {
                    //Update if data exists
                    var idResult = db.TblMyanHandicapResult.First(a => a.RapidEventId.Equals(s.ID));
                    idResult.Body = bodyResult;
                    idResult.Goal = goalResult;
                    idResult.OverTeamId = s.homeTeam;
                    idResult.UnderTeamId = s.awayTeam;
                    idResult.HomeTeamId = s.homeTeam;
                    idResult.AwayTeamId = s.awayTeam;
                    idResult.LeagueId = s.league;
                    idResult.RapidEventId = s.ID;
                    db.SaveChanges();
                }

                //Home/CalculateHomeHandicap
            }
            return View();
        }

        // Calculation Myanmar Handicap from away handicap
        public IActionResult CalculateAwayHandicap()
        {
            string bodyResult = "";
            string goalResult = "";
            var person = (from p in db.TblPreUpcomingEvent
                          join e in db.TblHandicap
                        on p.RapidEventId equals e.RapidEventId
                          where e.AwayHandicap.Contains("-")
                          select new
                          {
                              ID = p.RapidEventId,
                              homeHandicap = e.HomeHandicap,
                              awayHandicap = e.AwayHandicap,
                              homeOdds = e.HomeOdd,
                              awayOdds = e.AwayOdd,
                              homeTeam = p.HomeTeamId,
                              awayTeam = p.AwayTeamId,
                              league = p.LeagueId,
                              goalHandicap = e.GoalHandicap,
                              goalOverOdds = e.OverOdd,
                              goalUnderOdds = e.UnderOdd
                          }).ToList();
            foreach (var s in person)
            {
                TblMyanHandicapResult myanHandicapResult = new TblMyanHandicapResult();

                //Calculate body handicap
                var oddsResult = (decimal)(s.awayOdds - s.homeOdds);
                var intResult = (int)(oddsResult * 100);
                var unit = Round(intResult);
                decimal hanDecimal = Decimal.Parse(s.awayHandicap);
                string hanString = Math.Abs(hanDecimal).ToString();
                var goalUnit = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().GoalUnit;
                var unitAmount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().UnitAmount;
                var unitAmountInt = Convert.ToInt32(unitAmount);
                int unitAmountResult = unitAmountInt + unit;
                var positiveNumber = Math.Abs(unitAmountResult);

                if (positiveNumber < 100)
                {
                    if (goalUnit == 0)
                    {
                        bodyResult = Draw(unitAmountResult);

                    }
                    else
                    {
                        bodyResult = Body(unitAmountResult, (int)goalUnit);
                    }
                }
                else if (positiveNumber == 100)
                {
                    if (goalUnit == 0)
                    {
                        bodyResult = "=" + "-";
                    }
                    else
                    {
                        bodyResult = goalUnit.ToString() + "-";
                    }

                }
                else
                {
                    int newGoalUnit = (int)goalUnit + 1;
                    int newUnitAmountResult = 200 - positiveNumber;
                    bodyResult = Body(newUnitAmountResult, newGoalUnit);
                }

                //Calculate goal handicap
                var goalsOdds = (decimal)(s.goalOverOdds - s.goalUnderOdds);
                var goalsOddsInt = (int)(goalsOdds * 100);
                var goalsUnit = Round(goalsOddsInt);
                decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                string goalsString = Math.Abs(goalsDecimal).ToString();
                var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                var amountInt = Convert.ToInt32(amount);
                int amountResult = amountInt + goalsUnit;
                var positiveAmount = Math.Abs(amountResult);

                if (positiveAmount < 100)
                {
                    goalResult = Body(amountResult, (int)goals);
                }
                else if (positiveAmount == 100)
                {
                    goalResult = goalUnit.ToString() + "-";

                }
                else
                {
                    int newGoals = (int)goalUnit + 1;
                    int newAmountResult = 200 - positiveAmount;
                    goalResult = Body(newAmountResult, newGoals);
                }

                //Save into database
                var value = db.TblMyanHandicapResult.ToList().Any(a => Convert.ToInt32(a.RapidEventId) == Convert.ToInt32(s.ID));
                if (value == false)
                {
                    //Save if data not exists
                    myanHandicapResult.Body = bodyResult;
                    myanHandicapResult.Goal = goalResult;
                    myanHandicapResult.OverTeamId = s.awayTeam;
                    myanHandicapResult.UnderTeamId = s.homeTeam;
                    myanHandicapResult.HomeTeamId = s.homeTeam;
                    myanHandicapResult.AwayTeamId = s.awayTeam;
                    myanHandicapResult.LeagueId = s.league;
                    myanHandicapResult.RapidEventId = s.ID;
                    db.TblMyanHandicapResult.Add(myanHandicapResult);
                    db.SaveChanges();
                }
                else
                {
                    //Update if data exists
                    var idResult = db.TblMyanHandicapResult.First(a => a.RapidEventId.Equals(s.ID));
                    idResult.Body = bodyResult;
                    idResult.Goal = goalResult;
                    idResult.OverTeamId = s.awayTeam;
                    idResult.UnderTeamId = s.homeTeam;
                    idResult.HomeTeamId = s.homeTeam;
                    idResult.AwayTeamId = s.awayTeam;
                    idResult.LeagueId = s.league;
                    idResult.RapidEventId = s.ID;
                    db.SaveChanges();
                }

                //Home/CalculateAwayHandicap
            }
            return View();
        }

        // Calculation Myanmar Handicap from zero handicap
        public IActionResult CalculateZeroHandicap()
        {
            string bodyResult = "";
            string goalResult = "";
            var person = (from p in db.TblPreUpcomingEvent
                          join e in db.TblHandicap
                        on p.RapidEventId equals e.RapidEventId
                          where e.HomeHandicap.Contains("0.0") && e.AwayHandicap.Contains("0.0")
                          select new
                          {
                              ID = p.RapidEventId,
                              homeHandicap = e.HomeHandicap,
                              awayHandicap = e.AwayHandicap,
                              homeOdds = e.HomeOdd,
                              awayOdds = e.AwayOdd,
                              homeTeam = p.HomeTeamId,
                              awayTeam = p.AwayTeamId,
                              league = p.LeagueId,
                              goalHandicap = e.GoalHandicap,
                              goalOverOdds = e.OverOdd,
                              goalUnderOdds = e.UnderOdd
                          }).ToList();
            foreach (var s in person)
            {
                TblMyanHandicapResult myanHandicapResult = new TblMyanHandicapResult();

                //Calculate body handicap
                var oddsResult = (decimal)(s.homeOdds - s.awayOdds);
                var intResult = (int)(oddsResult * 100);
                var unit = Round(intResult);
                decimal hanDecimal = Decimal.Parse(s.homeHandicap);
                string hanString = Math.Abs(hanDecimal).ToString();
                var goalUnit = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().GoalUnit;
                var unitAmount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().UnitAmount;
                var unitAmountInt = Convert.ToInt32(unitAmount);
                int unitAmountResult = unitAmountInt + unit;
                var positiveNumber = Math.Abs(unitAmountResult);

                if (positiveNumber < 100)
                {
                        bodyResult = Draw(unitAmountResult);
                }

                //Calculate goal handicap
                var goalsOdds = (decimal)(s.goalOverOdds - s.goalUnderOdds);
                var goalsOddsInt = (int)(goalsOdds * 100);
                var goalsUnit = Round(goalsOddsInt);
                decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                string goalsString = Math.Abs(goalsDecimal).ToString();
                var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                var amountInt = Convert.ToInt32(amount);
                int amountResult = amountInt + goalsUnit;
                var positiveAmount = Math.Abs(amountResult);

                if (positiveAmount < 100)
                {
                    goalResult = Body(amountResult, (int)goals);
                }
                else if (positiveAmount == 100)
                {
                    goalResult = goalUnit.ToString() + "-";

                }
                else
                {
                    int newGoals = (int)goalUnit + 1;
                    int newAmountResult = 200 - positiveAmount;
                    goalResult = Body(newAmountResult, newGoals);
                }

                //Save into database
                var value = db.TblMyanHandicapResult.ToList().Any(a => Convert.ToInt32(a.RapidEventId) == Convert.ToInt32(s.ID));
                if (value == false)
                {
                    //Save if data not exists
                    myanHandicapResult.Body = bodyResult;
                    myanHandicapResult.Goal = goalResult;
                    myanHandicapResult.OverTeamId = s.homeTeam;
                    myanHandicapResult.UnderTeamId = s.awayTeam;
                    myanHandicapResult.HomeTeamId = s.homeTeam;
                    myanHandicapResult.AwayTeamId = s.awayTeam;
                    myanHandicapResult.LeagueId = s.league;
                    myanHandicapResult.RapidEventId = s.ID;
                    db.TblMyanHandicapResult.Add(myanHandicapResult);
                    db.SaveChanges();
                }
                else
                {
                    //Update if data exists
                    var idResult = db.TblMyanHandicapResult.First(a => a.RapidEventId.Equals(s.ID));
                    idResult.Body = bodyResult;
                    idResult.Goal = goalResult;
                    idResult.OverTeamId = s.homeTeam;
                    idResult.UnderTeamId = s.awayTeam;
                    idResult.HomeTeamId = s.homeTeam;
                    idResult.AwayTeamId = s.awayTeam;
                    idResult.LeagueId = s.league;
                    idResult.RapidEventId = s.ID;
                    db.SaveChanges();
                }

                //Home/CalculateZeroHandicap
            }
            return View();
        }

        //Fetch data from goal API(background process)
        public void GetGoals()
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
                    request.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                    IRestResponse response = client.Execute(request);
                    GoalResult data = JsonConvert.DeserializeObject<GoalResult>(response.Content);

                    //Add data into goal result table
                    TblGoalResult goalResult = new TblGoalResult();
                    DateTime dt = DateTime.Now;
                    var golArr = data.results[0].ss.Split("-");
                    goalResult.RapidEventId = c.ToString();
                    goalResult.HomeResult = Convert.ToInt32(golArr[0]);
                    goalResult.AwayResult = Convert.ToInt32(golArr[1]);
                    goalResult.EventDate = dt.Date;
                    goalResult.EventDatetime = dt;
                    db.TblGoalResult.Add(goalResult);
                    db.SaveChanges();


                }
                else
                {
                    //Record Existing
                }
            }//end of foreach loop
        }

        //Fetch data from API(background process)
        public void GetData()
        {
            //Calling data from RapidApI 
            var client = new RestClient("https://betsapi2.p.rapidapi.com/v1/bet365/upcoming?sport_id=1");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
            request.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
            IRestResponse response = client.Execute(request);
            Football data = JsonConvert.DeserializeObject<Football>(response.Content);

            //Get eventId of first array result
            var eventId = data.results[0].id;

            //Get total result from api data and calculate page
            var total = data.pager.total;
            int data_page = total / 50;
            if (total % 50 != 0)
            {
                data_page = data_page + 1;
            }

            for (var page = 1; page <= data_page; page++)
            {
                //Calling event data from RapidApI 
                var pageString = String.Concat("https://betsapi2.p.rapidapi.com/v1/bet365/upcoming?sport_id=1&page=", page);
                var client1 = new RestClient(pageString);
                var request1 = new RestRequest(Method.GET);
                request1.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                request1.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
                IRestResponse response1 = client1.Execute(request1);
                Football data1 = JsonConvert.DeserializeObject<Football>(response1.Content);



                for (var ii = 0; ii <= data.results.Length; ii++)
                {
                    var lastName = "";
                    var dd = data1.results[ii].league.name;
                    if (dd != null && dd != "")
                    {
                        var tmpArr = dd.Split(" ");
                        lastName = tmpArr[tmpArr.Count() - 1];
                    }
                    if (lastName.Equals("play"))
                    {
                        Console.WriteLine("This is Esports data");
                    }
                    else
                    {
                        //Fetch  Odds data from RapidApI 
                        eventId = data1.results[ii].id;
                        var resultString = String.Concat("https://betsapi2.p.rapidapi.com/v3/bet365/prematch?FI=", eventId);
                        var client2 = new RestClient(resultString);
                        var request2 = new RestRequest(Method.GET);
                        request2.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                        request2.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
                        IRestResponse response2 = client2.Execute(request2);
                        Odds data2 = JsonConvert.DeserializeObject<Odds>(response2.Content);
                        Handicap data5 = JsonConvert.DeserializeObject<Handicap>(response2.Content);
                        //FROdds data8 = JsonConvert.DeserializeObject<FROdds>(response2.Content);
                        //FRHandicap data9 = JsonConvert.DeserializeObject<FRHandicap>(response2.Content);

                        //------------------------------------------------------------Adding into leageue table--------------------------------------------
                        TblLeague lg = new TblLeague();
                        var league_name = data1.results[ii].league.name;
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

                        //----------------------------------------------------Adding away team into football team table-----------------------------------
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

                        //------------------------------------------------------Adding home team into football team table----------------------------------
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

                        //----------------------------------------------------------Adding preUpcoming table---------------------------------------------
                        TblPreUpcomingEvent pre = new TblPreUpcomingEvent();
                        decimal decPre = Decimal.Parse(data2.results[0].FI);
                        var pre_lgId = db.TblLeague.Where(a => a.LeagueName == league_name).FirstOrDefault().LeagueId;
                        var pre_home = db.TblFootballTeam.Where(a => a.FootballTeam == homeTeam).FirstOrDefault().FootballTeamId;
                        var pre_away = db.TblFootballTeam.Where(a => a.FootballTeam == awayTeam).FirstOrDefault().FootballTeamId;

                        //Change timestamp to local time
                        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                        var time_stamp = Convert.ToDouble(data1.results[ii].time);
                        dtDateTime = dtDateTime.AddSeconds(time_stamp).ToLocalTime();
                        var shortdate = dtDateTime.ToShortDateString();
                        var shorttime = dtDateTime.ToShortTimeString();

                        //Filter eventId
                        var preValue = db.TblPreUpcomingEvent.ToList().Any(a => a.RapidEventId == decPre);
                        if (preValue == true)
                        {
                            //Update if data exist 
                            var id = db.TblPreUpcomingEvent.Where(a => a.RapidEventId == decPre).FirstOrDefault().PreUpcommingEventId;
                            var preUpcoming = db.TblPreUpcomingEvent.FirstOrDefault(s => s.PreUpcommingEventId.Equals(id));
                            preUpcoming.RapidEventId = decPre;
                            preUpcoming.LeagueId = pre_lgId;
                            preUpcoming.HomeTeamId = pre_home;
                            preUpcoming.AwayTeamId = pre_away;
                            preUpcoming.EventDate = DateTime.Parse(shortdate);
                            preUpcoming.EventTime = DateTime.Parse(shorttime);
                            db.SaveChanges();
                        }
                        //------------------------------------------------------Adding upcomingevent table-----------------------------------------------
                        TblUpcomingEvent up = new TblUpcomingEvent();
                        decimal decUp = Decimal.Parse(data1.results[ii].id);
                        var up_lgId = db.TblLeague.Where(a => a.LeagueName == league_name).FirstOrDefault().LeagueId;
                        var up_home = db.TblFootballTeam.Where(a => a.FootballTeam == homeTeam).FirstOrDefault().FootballTeamId;
                        var up_away = db.TblFootballTeam.Where(a => a.FootballTeam == awayTeam).FirstOrDefault().FootballTeamId;

                        //Filter eventId
                        var upValue = db.TblUpcomingEvent.ToList().Any(a => a.RapidEventId == decUp);
                        if (upValue == true)
                        {
                            //Update if data exist 
                            var id = db.TblUpcomingEvent.Where(a => a.RapidEventId == decUp).FirstOrDefault().UpcomingEventId;
                            var upcoming = db.TblUpcomingEvent.FirstOrDefault(s => s.UpcomingEventId.Equals(id));
                            upcoming.RapidEventId = decUp;
                            upcoming.LeagueId = up_lgId;
                            upcoming.HomeTeamId = up_home;
                            upcoming.AwayTeamId = up_away;
                            upcoming.EventDate = DateTime.Parse(shortdate);
                            upcoming.EventTime = DateTime.Parse(shorttime);
                            db.SaveChanges();
                        }
                        else
                        {
                            //Insert if data not exist
                            up.RapidEventId = decUp;
                            up.LeagueId = up_lgId;
                            up.HomeTeamId = up_home;
                            up.AwayTeamId = up_away;
                            up.EventDate = DateTime.Parse(shortdate);
                            up.EventTime = DateTime.Parse(shorttime);
                            db.TblUpcomingEvent.Add(up);
                            db.SaveChanges();
                        }


                        //--------------------------------------------------------Adding handicap table--------------------------------------------------
                        TblHandicap hd = new TblHandicap();
                        decimal dec_overOdds = 0;
                        decimal dec_underOdds = 0;
                        string goalsHandicap = "0";
                        if (data2.results[0].asian_lines != null)
                        {
                            decimal decHd = Decimal.Parse(data2.results[0].FI);
                            decimal dec_home_odds = Decimal.Parse(data2.results[0].asian_lines.sp.asian_handicap.odds[0].odds);
                            decimal dec_away_odds = Decimal.Parse(data2.results[0].asian_lines.sp.asian_handicap.odds[1].odds);
                            if (data2.results[0].asian_lines.sp.goal_line != null)
                            {
                                dec_overOdds = Decimal.Parse(data2.results[0].asian_lines.sp.goal_line.odds[0].odds);
                                dec_underOdds = Decimal.Parse(data2.results[0].asian_lines.sp.goal_line.odds[1].odds);
                                goalsHandicap = data2.results[0].asian_lines.sp.goal_line.odds[0].name;
                            }
                            var value_h = data2.results[0].asian_lines.sp.asian_handicap.odds[0].name;
                            var value = data2.results[0].asian_lines.sp.asian_handicap.odds[1].name;
                            //Check class name of api whether it is name or handicap
                            if (value == null && value_h == null)
                            {
                                value_h = data5.results[0].asian_lines.sp.asian_handicap.odds[0].handicap;
                                value = data5.results[0].asian_lines.sp.asian_handicap.odds[1].handicap;
                            }
                            if (goalsHandicap == null)
                            {
                                goalsHandicap = data2.results[0].asian_lines.sp.goal_line.odds[0].handicap;
                            }

                            if (goalsHandicap.Length > 4)
                            {
                                var newGoalsHandicap = goalsHandicap.Split(",");
                                goalsHandicap = newGoalsHandicap[1];
                            }
                            //Filter eventId
                            var hanValue = db.TblHandicap.ToList().Any(a => a.RapidEventId == decHd);
                            if (hanValue == true)
                            {
                                //Update if data exist 
                                var id = db.TblHandicap.Where(a => a.RapidEventId == decHd).FirstOrDefault().HandicapId;
                                var handicap = db.TblHandicap.FirstOrDefault(s => s.HandicapId.Equals(id));
                                if (value.Length <= 5 && value_h.Length <= 5)
                                {

                                    handicap.RapidEventId = decHd;
                                    handicap.HomeOdd = dec_home_odds;
                                    handicap.HomeHandicap = value_h;
                                    handicap.AwayOdd = dec_away_odds;
                                    handicap.AwayHandicap = value;
                                    handicap.OverOdd = dec_overOdds;
                                    handicap.UnderOdd = dec_underOdds;
                                    handicap.GoalHandicap = goalsHandicap;
                                    handicap.EventDatetime = dtDateTime;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var cutHandicap = value.Split(",");
                                    var cutHandicap_h = value_h.Split(",");
                                    handicap.RapidEventId = decHd;
                                    handicap.HomeOdd = dec_home_odds;
                                    handicap.HomeHandicap = cutHandicap_h[0];
                                    handicap.AwayOdd = dec_away_odds;
                                    handicap.AwayHandicap = cutHandicap[0];
                                    handicap.OverOdd = dec_overOdds;
                                    handicap.UnderOdd = dec_underOdds;
                                    handicap.GoalHandicap = goalsHandicap;
                                    handicap.EventDatetime = dtDateTime;
                                    db.SaveChanges();
                                }

                            }
                            else
                            {
                                //Insert if data not exist
                                if (value.Length <= 5 && value_h.Length <= 5)
                                {
                                    hd.RapidEventId = decHd;
                                    hd.HomeOdd = dec_home_odds;
                                    hd.HomeHandicap = value_h;
                                    hd.AwayOdd = dec_away_odds;
                                    hd.AwayHandicap = value;
                                    hd.EventDatetime = dtDateTime;
                                    hd.OverOdd = dec_overOdds;
                                    hd.UnderOdd = dec_underOdds;
                                    hd.GoalHandicap = goalsHandicap;
                                    db.TblHandicap.Add(hd);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var cutHandicap = value.Split(",");
                                    var cutHandicap_h = value_h.Split(",");
                                    hd.RapidEventId = decHd;
                                    hd.HomeOdd = dec_home_odds;
                                    hd.HomeHandicap = cutHandicap_h[0];
                                    hd.AwayOdd = dec_away_odds;
                                    hd.AwayHandicap = cutHandicap[0];
                                    hd.EventDatetime = dtDateTime;
                                    hd.OverOdd = dec_overOdds;
                                    hd.UnderOdd = dec_underOdds;
                                    hd.GoalHandicap = goalsHandicap;
                                    db.TblHandicap.Add(hd);
                                    db.SaveChanges();
                                }

                            }
                        }


                    }
                    Console.WriteLine("Completed Data" + ii + "Result");

                }
                Console.WriteLine("Completed Page" + page + "Result");
            }

        }

        public IActionResult Test()
        {
            int goalUnitInt = 0;
            decimal unit = 0;
            decimal totalAmount = 0;
            var betting = (from g in db.TblGambling
                           join d in db.TblGamblingDetails
                           on g.GamblingId equals d.GamblingId
                           where g.RapidEventId == 1000
                           select new
                           {
                               betType = g.GamblingTypeId,
                               betTeamCount = g.TeamCount,
                               betTeam = d.FootballTeamId,
                               betOver = d.Overs,
                               betUnder = d.Under,
                               betAmount = g.Amount,
                               betBody = d.BodyOdd,
                               betGoal = d.GoalOdd,
                               betUser=g.UserId
                           }).ToList();
            foreach (var item in betting)
            {
                var diff = 0;
                //Check betting is body or maung handicap
                if (item.betType == 1)
                {
                    var commission = db.TblUserCommission.Where(a => a.UserId == item.betUser && a.UserCommissionTypeId == 1).FirstOrDefault().SubUserCommission;
                    //Check betting is body or goal handicap
                    //----For body betting-----
                    if (item.betTeam != 0)
                    {
                       
                        int[] body = CutBodyHandicap(item.betBody);
                        goalUnitInt = body[0];
                        unit = body[1];
                        var tempOver= db.TblMyanHandicapResult.ToList().Any(a => a.OverTeamId == item.betTeam && a.RapidEventId==1000);
                       // var tempUnder = db.TblMyanHandicapResult.ToList().Any(a => a.UnderTeamId == item.betTeam && a.RapidEventId == 1000);
                        var tempHome = db.TblMyanHandicapResult.ToList().Any(a => a.HomeTeamId == item.betTeam && a.RapidEventId == 1000);
                        //var tempAway = db.TblMyanHandicapResult.ToList().Any(a => a.AwayTeamId == item.betTeam && a.RapidEventId == 1000);
                        //----Check bet team is over----
                        if (tempOver == true)
                        {
                            if (tempHome == true)
                            {
                                //HomeGoal - AwayGoal
                                diff = 0 - 0;
                                totalAmount = WinOrLoseOver(goalUnitInt, diff, unit, item.betAmount, (decimal)commission);
                                Console.WriteLine(totalAmount);
                            }
                            else
                            {
                                //AwayGoal - HomeGoal
                                diff = 0 - 0;
                                totalAmount = WinOrLoseOver(goalUnitInt, diff, unit, item.betAmount, (decimal)commission);
                                Console.WriteLine(totalAmount);
                            }
                        }
                        //----Check bet team is under----
                        else
                        {
                            if (tempHome == true)
                            {
                                //AwayGoal - HomeGoal
                                diff = 4 - 2;
                                unit *= -1;
                                totalAmount = WinOrLoseUnder(goalUnitInt, diff, unit, item.betAmount, (decimal)commission);
                                Console.WriteLine(totalAmount);

                            }
                            else
                            {
                                //HomeGoal - AwayGoal
                                diff = 2 - 0;
                                unit *= -1;
                                totalAmount = WinOrLoseUnder(goalUnitInt, diff, unit, item.betAmount, (decimal)commission);
                                Console.WriteLine(totalAmount);
                            }
                        }//End of Over or Under     
                    }
                    //----For total goal betting-----
                    else
                    {
                        int[] goal = CutBodyHandicap(item.betGoal);
                        goalUnitInt = goal[0];
                        unit = goal[1];
                        diff = 1 + 2;
                        if (item.betOver == true)
                        {
                            totalAmount = WinOrLoseOver(goalUnitInt, diff, unit, item.betAmount, (decimal)commission);
                            Console.WriteLine(totalAmount);
                        }
                        if(item.betUnder == true)
                        {
                            unit *= -1;
                            totalAmount = WinOrLoseUnder(goalUnitInt, diff, unit, item.betAmount, (decimal)commission);
                            Console.WriteLine(totalAmount);
                        }
                    }//End of body or goal handicap
                }//End of body or maung
                // /Home/Test
           }//End of one event result
            return View();
        }

        //---------------------------------------Calculation Methods----------------------------

        //Calculation odds to take the nearest value
        public int Round(int odds)
        {
            int result;
            int remainder = odds % 10;
            if (remainder < 5)
            {
                result = odds - remainder;
            }
            else
            {
                int tempValue = 10 - remainder;
                result = odds + tempValue;
            }
            return result;
        }

        //Concatanation goal and unit as a string body
        public string Body(int unit, int goal)
        {
            string result;
            if (unit.ToString().Length == 3)
            {
                result = goal.ToString() + unit.ToString();
            }
            else
            {
                result = goal.ToString() + "+" + unit.ToString();
            }
            return result;
        }

        //Concatanation unit as a string body
        public string Draw(int unit)
        {
            string result;
            if (unit.ToString().Length == 3)
            {
                result = unit.ToString();
            }
            else
            {
                result = "+" + unit.ToString();
            }
            return result;
        }

        //Method of Spliting String Handicap to Int Array
        private int[] CutBodyHandicap(string betBody)
        {
            int[] result=new int[2];
            string goalUnit;
            if (betBody.Length >= 4)
            {
                goalUnit = betBody.Remove(1, 3);
                string unit = betBody.Remove(0, 1);
                if (goalUnit.Equals("="))
                {
                    result[0] = 0;
                    result[1] = Convert.ToInt32(unit);
                }
                else
                {
                    result[0] = Convert.ToInt32(goalUnit);
                    result[1] = Convert.ToInt32(unit);
                }

            }
            else
            {
                goalUnit = betBody.Remove(1, 1);
                if (goalUnit.Equals("="))
                {
                    result[0] = 0;
                    result[1] = -100;
                }
                else
                {
                    result[0] = Convert.ToInt32(goalUnit);
                    result[1] = -100;
                }
            }
            return result;
        }

        //Method of Calculation Win or Lose for Over Betting
        private int WinOrLoseOver(int goalUnit, int diff, decimal unit, int? betAmount,decimal com)
        {
            int result;
            int tempValue;
            int tempAmount;
            if (diff == goalUnit)
            {
                tempAmount = (int)(betAmount * Math.Round(unit / 100, 2));
                tempValue = (int)(betAmount + tempAmount);
                if (tempValue > betAmount)
                {
                    var commision= (int)(tempValue * Math.Round(com / 100, 2));
                    result = tempValue - commision;
                }
                else
                {
                    result = tempValue;
                }
            }
            else if (diff > goalUnit)
            {
                tempAmount = (int)(betAmount * 2);
                tempValue = (int)(tempAmount * Math.Round(com / 100, 2));
                result = (int)(tempAmount - tempValue);
            }
            else
            {
                result = 0;
            }

            return result;
        }

        //Method of Calculation Win or Lose for Under Betting
        private int WinOrLoseUnder(int goalUnit, int diff, decimal unit, int? betAmount, decimal com)
        {
            int result;
            int tempValue;
            int tempAmount;
            if (diff == goalUnit)
            {
                tempAmount = (int)(betAmount * Math.Round(unit / 100, 2));
                tempValue = (int)(betAmount + tempAmount);
                if (tempValue > betAmount)
                {
                    var commision = (int)(tempValue * Math.Round(com / 100, 2));
                    result = tempValue - commision;
                }
                else
                {
                    result = tempValue;
                }
            }
            else if (diff < goalUnit)
            {
                tempAmount = (int)(betAmount * 2);
                tempValue = (int)(tempAmount * Math.Round(com / 100, 2));
                result = (int)(tempAmount - tempValue);
            }
            else
            {
                result = 0;
            }

            return result;
        }

        public IActionResult Hola()
        {
            int goalUnitInt = 0;
            decimal unit = 0;
            decimal totalAmount = 0;
            var diff = 0;
            //Check betting is body or maung handicap
            var betTeam = 1;
            var betAmount = 10000;
            var betOver = false;
            var betUnder = false;
            var h = 4;
            var a = 2;
            var commission = db.TblUserCommission.Where(a => a.UserId == 1 && a.UserCommissionTypeId == 1).FirstOrDefault().SubUserCommission;
            //Check betting is body or goal handicap
            //----For body betting-----
            if (betTeam != 0)
            {

                int[] body = CutBodyHandicap("2+50");
                goalUnitInt = body[0];
                unit = body[1];
                var tempOver = false;
                var tempUnder = true;
                var tempHome = false;
                var tempAway = true;

                //----Check bet team is over----
                if (tempOver == true)
                {
                    if (tempHome == true)
                    {
                        //HomeGoal - AwayGoal
                        diff = h - a;
                        totalAmount = WinOrLoseOver(goalUnitInt, diff, unit, betAmount, (decimal)commission);
                        Console.WriteLine(totalAmount);
                    }
                    else
                    {
                        //AwayGoal - HomeGoal
                        diff = a - h;
                        totalAmount = WinOrLoseOver(goalUnitInt, diff, unit, betAmount, (decimal)commission);
                        Console.WriteLine(totalAmount);
                    }
                }
                //----Check bet team is under----
                else
                {
                    if (tempHome == true)
                    {
                        //AwayGoal - HomeGoal
                        diff = a - h;
                        unit *= -1;
                        totalAmount = WinOrLoseUnder(goalUnitInt, diff, unit, betAmount, (decimal)commission);
                        Console.WriteLine(totalAmount);

                    }
                    else
                    {
                        //HomeGoal - AwayGoal
                        diff = h - a;
                        unit *= -1;
                        totalAmount = WinOrLoseUnder(goalUnitInt, diff, unit, betAmount, (decimal)commission);
                        Console.WriteLine(totalAmount);
                    }
                }//End of Over or Under     
            }
            //----For total goal betting-----
            else
            {
                int[] goal = CutBodyHandicap("1+50");
                goalUnitInt = goal[0];
                unit = goal[1];
                diff = a + h;
                if (betOver == true)
                {
                    totalAmount = WinOrLoseOver(goalUnitInt, diff, unit, betAmount, (decimal)commission);
                    Console.WriteLine(totalAmount);
                }
                if (betUnder == true)
                {
                    unit *= -1;
                    totalAmount = WinOrLoseUnder(goalUnitInt, diff, unit, betAmount, (decimal)commission);
                    Console.WriteLine(totalAmount);
                }
            }//End of body or goal handicap
             //End of body or maung
             // /Home/Test
             //End of one event result
            return View();
        }
    }
}
