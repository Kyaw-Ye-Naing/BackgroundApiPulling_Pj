using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Background_Task_Api_Pulling.Models;
using RestSharp;
using Newtonsoft.Json;

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
            // RecurringJob.AddOrUpdate(() => GetGoals(), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => GetData(), Cron.Minutely);
            // RecurringJob.AddOrUpdate(() => CalculateHomeHandicap(), Cron.Minutely);
            // RecurringJob.AddOrUpdate(() => CalculateAwayHandicap(), Cron.Minutely);
            // RecurringJob.AddOrUpdate(() => CalculateZeroHandicap(), Cron.Minutely);


            return View();
        }

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

        //Calculation odds to take the nearest value
        public int Round(int odds)
        {
            int result = 0;
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
            string result = "";
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
            string result = "";
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
                    var golArr = data.results[0].ss.Split("-");
                    goalResult.RapidEventId = c.ToString();
                    goalResult.HomeResult = Convert.ToInt32(golArr[0]);
                    goalResult.AwayResult = Convert.ToInt32(golArr[1]);
                    db.TblGoalResult.Add(goalResult);
                    db.SaveChanges();
                }
                else
                {
                    //Record Existing
                }
            }
        }

        //Fetch data from API(background process)
        public void GetData()
        {
            //Calling data from RapidApI 
            var client = new RestClient("https://bet365-sports-odds.p.rapidapi.com/v1/bet365/upcoming?page=1&LNG_ID=1&sport_id=1");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "bet365-sports-odds.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
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
                var pageString = String.Concat("https://bet365-sports-odds.p.rapidapi.com/v1/bet365/upcoming?page=", page, "&LNG_ID=1&sport_id=1");
                var client1 = new RestClient(pageString);
                var request1 = new RestRequest(Method.GET);
                request1.AddHeader("x-rapidapi-host", "bet365-sports-odds.p.rapidapi.com");
                request1.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
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

                    }
                    else
                    {
                        //Fetch  Odds data from RapidApI 
                        eventId = data1.results[ii].id;
                        var resultString = String.Concat("https://bet365-sports-odds.p.rapidapi.com/v3/bet365/prematch?FI=", eventId);
                        var client2 = new RestClient(resultString);
                        var request2 = new RestRequest(Method.GET);
                        request2.AddHeader("x-rapidapi-host", "bet365-sports-odds.p.rapidapi.com");
                        request2.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                        IRestResponse response2 = client2.Execute(request2);
                        Odds data2 = JsonConvert.DeserializeObject<Odds>(response2.Content);
                        Handicap data5 = JsonConvert.DeserializeObject<Handicap>(response2.Content);

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
                        decimal decHd = Decimal.Parse(data2.results[0].FI);
                        decimal dec_home_odds = Decimal.Parse(data2.results[0].asian_lines.sp.asian_handicap.odds[0].odds);
                        decimal dec_away_odds = Decimal.Parse(data2.results[0].asian_lines.sp.asian_handicap.odds[1].odds);
                        decimal dec_overOdds = Decimal.Parse(data2.results[0].asian_lines.sp.goal_line.odds[0].odds);
                        decimal dec_underOdds = Decimal.Parse(data2.results[0].asian_lines.sp.goal_line.odds[1].odds);
                        var value = data2.results[0].asian_lines.sp.asian_handicap.odds[0].name;
                        var value_h = data2.results[0].asian_lines.sp.asian_handicap.odds[1].name;
                        var goalsHandicap = data2.results[0].asian_lines.sp.goal_line.odds[0].name;
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
                       
                        if (goalsHandicap.Length > 4) {
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
                                handicap.AwayHandicap = cutHandicap[1];
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
                                hd.AwayHandicap = cutHandicap[1];
                                hd.EventDatetime = dtDateTime;
                                db.TblHandicap.Add(hd);
                                db.SaveChanges();
                            }

                        }

                    }
                    Console.WriteLine("Completed Data" + ii + "Result");

                }
                Console.WriteLine("Completed Page" + page + "Result");
            }

        }

        public IActionResult Get()
        {

            decimal dec_overOdds = Decimal.Parse("1.85");
            decimal dec_underOdds = Decimal.Parse("2.00");
            var goalsHandicap ="null";
            //Check class name of api whether it is name or handicap

            if (goalsHandicap.Length > 4)
            {
                var newGoalsHandicap = goalsHandicap.Split(",");
                goalsHandicap = newGoalsHandicap[1];
            }
            var dd = goalsHandicap;

            return View();
        }

        public void UpdatePre()
        {
            //var preValue = db.TblPreUpcomingEvent.ToList().Any(a => a.RapidEventId == decPre);
            //if (preValue == true)
            //{
            //    //Update if data exist 
            //   var id = db.TblPreUpcomingEvent.Where(a => a.RapidEventId == decPre).FirstOrDefault().PreUpcommingEventId;
            //    var preUpcoming = db.TblPreUpcomingEvent.FirstOrDefault(s => s.PreUpcommingEventId.Equals(id));
            //    preUpcoming.RapidEventId = decPre;
            //    preUpcoming.LeagueId = pre_lgId;
            //    preUpcoming.HomeTeamId = pre_home;
            //    preUpcoming.AwayTeamId = pre_away;
            //    preUpcoming.EventDate = DateTime.Parse(shortdate);
            //    preUpcoming.EventTime = DateTime.Parse(shorttime);
            //    db.SaveChanges();
            //}
        }


    }
}
