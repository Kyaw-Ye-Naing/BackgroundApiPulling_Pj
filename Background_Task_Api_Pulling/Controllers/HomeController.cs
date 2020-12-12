using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Background_Task_Api_Pulling.Models;
using RestSharp;
using Background_Task_Api_Pulling.CommonClass;
using Newtonsoft.Json;
using Hangfire;
using System.Collections.Generic;
using Background_Task_Api_Pulling.Models.Requests;

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
            RecurringJob.AddOrUpdate(() => GetData(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => CalculateHomeHandicap(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => CalculateAwayHandicap(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => CalculateZeroHandicap(), Cron.Hourly);
           // RecurringJob.AddOrUpdate(() => MixCalculation(), Cron.Hourly);
            return View();
        }

        //----------------------------------------Main Background Methods------------------------ 

        // Calculation Myanmar Handicap from home handicap
        public void CalculateHomeHandicap()
        {
            try
            {
                Round rd = new Round();
                StringConcat @string = new StringConcat();
                DateTime dt = DateTime.Now.Date;
                List<TblUnitHandicapFix> unitFix = new List<TblUnitHandicapFix>();
                unitFix = db.TblUnitHandicapFix.ToList();
                string bodyResult = "";
                string goalResult = "";
                var person = (from p in db.TblPreUpcomingEvent
                              join e in db.TblHandicap
                              on p.RapidEventId equals e.RapidEventId
                              where e.HomeHandicap.Contains("-") && p.Active == true
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
                                  goalUnderOdds = e.UnderOdd,
                                  PreEventId = p.PreUpcommingEventId
                              }).ToList();
                foreach (var s in person)
                {
                    TblMyanHandicapResult myanHandicapResult = new TblMyanHandicapResult();
                    //Calculate body handicap
                    var intResult = (int)((s.homeOdds - s.awayOdds) * 100);
                    var unit = rd.RoundValue(intResult);
                    decimal hanDecimal = Decimal.Parse(s.homeHandicap);
                    string hanString = Math.Abs(hanDecimal).ToString();
                    var goalUnit = unitFix.Where(a => a.Handicap.Equals(hanString)).First().GoalUnit;
                    var unitAmount = unitFix.Where(a => a.Handicap.Equals(hanString)).First().UnitAmount;
                    var unitAmountInt = Convert.ToInt32(unitAmount);
                    int unitAmountResult = unitAmountInt + unit;
                    var positiveNumber = Math.Abs(unitAmountResult);

                    if (positiveNumber <= 100)
                    {
                        if (goalUnit == 0)
                        {
                            bodyResult = @string.Draw(unitAmountResult);

                        }
                        else
                        {
                            bodyResult = @string.Body(unitAmountResult, (int)goalUnit);
                        }
                    }
                    //else if (positiveNumber == 100)
                    //{
                    //    if (goalUnit == 0)
                    //    {
                    //        bodyResult = "=" + "-";
                    //    }
                    //    else
                    //    {
                    //        bodyResult = goalUnit.ToString() + "-";
                    //    }

                    //}
                    else
                    {
                        int newGoalUnit = (int)goalUnit + 1;
                        int newUnitAmountResult = 200 - positiveNumber;
                        bodyResult = @string.Body(newUnitAmountResult, newGoalUnit);
                    }
                    //end of body calculation

                    //Calculate goal handicap
                    var goalsOdds = (decimal)(s.goalOverOdds - s.goalUnderOdds);
                    var goalsOddsInt = (int)(goalsOdds * 100);
                    var goalsUnit = rd.RoundValue(goalsOddsInt);
                    decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                    string goalsString = Math.Abs(goalsDecimal).ToString();
                    var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                    var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                    var amountInt = Convert.ToInt32(amount);
                    int amountResult = amountInt + goalsUnit;
                    var positiveAmount = Math.Abs(amountResult);

                    if (positiveAmount <= 100)
                    {
                        goalResult = @string.Body(amountResult, (int)goals);
                    }
                    //else if (positiveAmount == 100)
                    //{
                    //    goalResult = goalUnit.ToString() + "-";

                    //}
                    else
                    {
                        int newGoals = (int)goalUnit + 1;
                        int newAmountResult = 200 - positiveAmount;
                        goalResult = @string.Body(newAmountResult, newGoals);
                    }
                    //end of goal calculation

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
                        myanHandicapResult.PreUpcomingEventId = s.PreEventId;
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
                        idResult.PreUpcomingEventId = s.PreEventId;
                        db.SaveChanges();
                    }//end of save data
                    Console.WriteLine("Complete 1 Hadicap result from   HOME\n ");
                }//end of foreach loop
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Calculation Myanmar Handicap from away handicap
        public void CalculateAwayHandicap()
        {
            try
            {
                StringConcat @string = new StringConcat();
                Round rd = new Round();
                string bodyResult = "";
                string goalResult = "";
                var person = (from p in db.TblPreUpcomingEvent
                              join e in db.TblHandicap
                              on p.RapidEventId equals e.RapidEventId
                              where e.AwayHandicap.Contains("-") && p.Active == true
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
                                  goalUnderOdds = e.UnderOdd,
                                  preEvent = p.PreUpcommingEventId
                              }).ToList();
                foreach (var s in person)
                {
                    TblMyanHandicapResult myanHandicapResult = new TblMyanHandicapResult();
                    //Calculate body handicap
                    var oddsResult = (decimal)(s.awayOdds - s.homeOdds);
                    var intResult = (int)(oddsResult * 100);
                    var unit = rd.RoundValue(intResult);
                    decimal hanDecimal = Decimal.Parse(s.awayHandicap);
                    string hanString = Math.Abs(hanDecimal).ToString();
                    var goalUnit = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().GoalUnit;
                    var unitAmount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().UnitAmount;
                    var unitAmountInt = Convert.ToInt32(unitAmount);
                    int unitAmountResult = unitAmountInt + unit;
                    var positiveNumber = Math.Abs(unitAmountResult);

                    if (positiveNumber <= 100)
                    {
                        if (goalUnit == 0)
                        {
                            bodyResult = @string.Draw(unitAmountResult);

                        }
                        else
                        {
                            bodyResult = @string.Body(unitAmountResult, (int)goalUnit);
                        }
                    }
                    //else if (positiveNumber == 100)
                    //{
                    //    if (goalUnit == 0)
                    //    {
                    //        bodyResult = "=" + "-";
                    //    }
                    //    else
                    //    {
                    //        bodyResult = goalUnit.ToString() + "-";
                    //    }

                    //}
                    else
                    {
                        int newGoalUnit = (int)goalUnit + 1;
                        int newUnitAmountResult = 200 - positiveNumber;
                        bodyResult = @string.Body(newUnitAmountResult, newGoalUnit);
                    }
                    //end of body calculation

                    //Calculate goal handicap
                    var goalsOdds = (decimal)(s.goalOverOdds - s.goalUnderOdds);
                    var goalsOddsInt = (int)(goalsOdds * 100);
                    var goalsUnit = rd.RoundValue(goalsOddsInt);
                    decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                    string goalsString = Math.Abs(goalsDecimal).ToString();
                    var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                    var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                    var amountInt = Convert.ToInt32(amount);
                    int amountResult = amountInt + goalsUnit;
                    var positiveAmount = Math.Abs(amountResult);

                    if (positiveAmount <= 100)
                    {
                        goalResult = @string.Body(amountResult, (int)goals);
                    }
                    //else if (positiveAmount == 100)
                    //{
                    //    goalResult = goalUnit.ToString() + "-";

                    //}
                    else
                    {
                        int newGoals = (int)goalUnit + 1;
                        int newAmountResult = 200 - positiveAmount;
                        goalResult = @string.Body(newAmountResult, newGoals);
                    }
                    //end of goal calculation

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
                        myanHandicapResult.PreUpcomingEventId = s.preEvent;
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
                        idResult.PreUpcomingEventId = s.preEvent;
                        db.SaveChanges();
                    }//end of save
                    Console.WriteLine("Complete 1 jandicap result from   AWAY\n  ");
                }
            }
           catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Calculation Myanmar Handicap from zero handicap
        public void CalculateZeroHandicap()
        {
            try
            {
                StringConcat @string = new StringConcat();
                Round rd = new Round();
                string bodyResult = "";
                string goalResult = "";
                int o_team, u_team;
                decimal oddsResult;
                var person = (from p in db.TblPreUpcomingEvent
                              join e in db.TblHandicap
                              on p.RapidEventId equals e.RapidEventId
                              where e.HomeHandicap.Contains("0.0") && e.AwayHandicap.Contains("0.0") && p.Active == true
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
                                  goalUnderOdds = e.UnderOdd,
                                  preEventId = p.PreUpcommingEventId
                              }).ToList();
                foreach (var s in person)
                {

                    TblMyanHandicapResult myanHandicapResult = new TblMyanHandicapResult();
                    //Calculate body handicap
                    if (s.homeOdds > s.awayOdds)
                    {
                        oddsResult = (decimal)(s.awayOdds - s.homeOdds);
                        o_team = (int)s.awayTeam;
                        u_team = (int)s.homeTeam;
                    }
                    else
                    {
                        oddsResult = (decimal)(s.homeOdds - s.awayOdds);
                        o_team = (int)s.homeTeam;
                        u_team = (int)s.awayTeam;
                    }
                    var intResult = (int)(oddsResult * 100);
                    var unit = rd.RoundValue(intResult);
                    decimal hanDecimal = Decimal.Parse(s.homeHandicap);
                    string hanString = Math.Abs(hanDecimal).ToString();
                    var goalUnit = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().GoalUnit;
                    var unitAmount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(hanString)).First().UnitAmount;
                    var unitAmountInt = Convert.ToInt32(unitAmount);
                    int unitAmountResult = unitAmountInt + unit;
                    var positiveNumber = Math.Abs(unitAmountResult);

                    if (positiveNumber < 100)
                    {
                        bodyResult = @string.Draw(unitAmountResult);
                    }
                    //end of body calculation

                    //Calculate goal handicap
                    var goalsOdds = (decimal)(s.goalOverOdds - s.goalUnderOdds);
                    var goalsOddsInt = (int)(goalsOdds * 100);
                    var goalsUnit = rd.RoundValue(goalsOddsInt);
                    decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                    string goalsString = Math.Abs(goalsDecimal).ToString();
                    var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                    var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                    var amountInt = Convert.ToInt32(amount);
                    int amountResult = amountInt + goalsUnit;
                    var positiveAmount = Math.Abs(amountResult);

                    if (positiveAmount <= 100)
                    {
                        goalResult = @string.Body(amountResult, (int)goals);
                    }
                    //else if (positiveAmount == 100)
                    //{
                    //    goalResult = goalUnit.ToString() + "-";

                    //}
                    else
                    {
                        int newGoals = (int)goalUnit + 1;
                        int newAmountResult = 200 - positiveAmount;
                        goalResult = @string.Body(newAmountResult, newGoals);
                    }
                    //end of goal calculation

                    //Save into database
                    var value = db.TblMyanHandicapResult.ToList().Any(a => Convert.ToInt32(a.RapidEventId) == Convert.ToInt32(s.ID));
                    if (value == false)
                    {
                        //Save if data not exists
                        myanHandicapResult.Body = bodyResult;
                        myanHandicapResult.Goal = goalResult;
                        myanHandicapResult.OverTeamId = o_team;
                        myanHandicapResult.UnderTeamId = u_team;
                        myanHandicapResult.HomeTeamId = s.homeTeam;
                        myanHandicapResult.AwayTeamId = s.awayTeam;
                        myanHandicapResult.LeagueId = s.league;
                        myanHandicapResult.RapidEventId = s.ID;
                        myanHandicapResult.PreUpcomingEventId = s.preEventId;
                        db.TblMyanHandicapResult.Add(myanHandicapResult);
                        db.SaveChanges();
                    }
                    else
                    {
                        //Update if data exists
                        var idResult = db.TblMyanHandicapResult.First(a => a.RapidEventId.Equals(s.ID));
                        idResult.Body = bodyResult;
                        idResult.Goal = goalResult;
                        idResult.OverTeamId = o_team;
                        idResult.UnderTeamId = u_team;
                        idResult.HomeTeamId = s.homeTeam;
                        idResult.AwayTeamId = s.awayTeam;
                        idResult.LeagueId = s.league;
                        idResult.RapidEventId = s.ID;
                        idResult.PreUpcomingEventId = s.preEventId;
                        db.SaveChanges();
                    }//end of save
                    Console.WriteLine("Complete 1 result from   ZERO\n  ");
                }//end of foreach loop
            }
           catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Fetch Goals Result Data From RapidAPI
        public IActionResult GetGoals()
        {
            //Filtering with today date
            var eventId = db.TblPreUpcomingEvent.Where(a => a.EventDate == DateTime.Today).ToList();
            foreach (var c in eventId)
            {
                //  var ftValue = db.TblGoalResult.ToList().Any(a => Convert.ToInt32(a.RapidEventId) == Convert.ToInt32(c));
                // if (ftValue == false)
                // {
                //Record not exist
                //Data fetch from API
                var eventString = String.Concat("https://betsapi2.p.rapidapi.com/v1/bet365/result?event_id=", c.RapidEventId.ToString());
                var client = new RestClient(eventString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                request.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
                IRestResponse response = client.Execute(request);
                GoalResult data = JsonConvert.DeserializeObject<GoalResult>(response.Content);

                if (data.results[0].ss != null)
                {
                    //Add data into goal result table
                    TblGoalResult goalResult = new TblGoalResult();
                    DateTime dt = DateTime.Now;
                    var golArr = data.results[0].ss.Split("-");
                    //goalResult.RapidEventId = c.RapidEventId.ToString();
                    //goalResult.UpcomingEventId = c.PreUpcommingEventId;
                    //goalResult.HomeResult = Convert.ToInt32(golArr[0]);
                    //goalResult.AwayResult = Convert.ToInt32(golArr[1]);
                    //goalResult.EventDate = dt.Date;
                    //goalResult.EventDatetime = dt;
                    //db.TblGoalResult.Add(goalResult);
                    //db.SaveChanges();
                    //BodyCalculation(Convert.ToInt32(golArr[0]), Convert.ToInt32(golArr[1]), (decimal)c.RapidEventId);
                    var pp=BodyCalculation(0,1, 2818202);
                }
                else
                {
                    Console.WriteLine("dsfsfffaf");
                }
               
            }//end of foreach loop
            return Ok();
        }

        //Fetch data from API
        public void GetData()
        {
            try
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
                    data_page += 1;
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



                    for (var ii = 0; ii < data1.results.Length; ii++)
                    {
                        var lastName = "";
                        var dd = data1.results[ii].league.name;
                        //if (dd != null && dd != "")
                        //{
                        //    var tmpArr = dd.Split(" ");
                        //    lastName = tmpArr[tmpArr.Count() - 1];
                        //}
                        //if (lastName.Equals("play"))
                        //{
                        //    Console.WriteLine("This is Esports data");
                        //}
                        if (dd.Equals("France Ligue 1") || dd.Equals("England Premier League"))
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

                            //------------------------------------------------------------Adding into leageue table--------------------------------------------
                            TblLeague lg = new TblLeague();
                            var league_name = data1.results[ii].league.name;
                            var lgValue = db.TblLeague.ToList().Any(a => a.LeagueName == league_name);
                            if (!lgValue)
                            {
                                //Record not present
                                lg.LeagueName = league_name;
                                decimal decRapid = Decimal.Parse(data1.results[ii].league.id);
                                lg.RapidLeagueId = decRapid;
                                lg.Active = false;
                                db.TblLeague.Add(lg);
                                db.SaveChanges();
                            }
                            //----------------------------------------------------Adding away team into football team table-----------------------------------
                            TblFootballTeam ft = new TblFootballTeam();
                            var awayTeam = data1.results[ii].away.name;
                            var lgId = data1.results[ii].league.id;
                            var ft_lgId = db.TblLeague.Where(a => a.RapidLeagueId.ToString().Equals(lgId)).FirstOrDefault().LeagueId;
                            var ftValue = db.TblFootballTeam.ToList().Any(a => a.FootballTeam == awayTeam);
                            if (!ftValue)
                            {
                                //no record
                                ft.FootballTeam = awayTeam;
                                ft.FootballTeamMyan = awayTeam;
                                decimal decAway = Decimal.Parse(data1.results[ii].away.id);
                                ft.RapidTeamId = decAway;
                                ft.LeagueId = ft_lgId;
                                ft.CreatedDate = DateTime.Now;
                                db.TblFootballTeam.Add(ft);
                                db.SaveChanges();

                            }
                            //------------------------------------------------------Adding home team into football team table----------------------------------
                            TblFootballTeam home_ft = new TblFootballTeam();
                            var homeTeam = data1.results[ii].home.name;
                            var ftValue1 = db.TblFootballTeam.ToList().Any(a => a.FootballTeam == homeTeam);
                            if (!ftValue1)
                            {
                                //no record
                                home_ft.FootballTeam = homeTeam;
                                home_ft.FootballTeamMyan = homeTeam;
                                decimal decHome = Decimal.Parse(data1.results[ii].home.id);
                                home_ft.RapidTeamId = decHome;
                                home_ft.LeagueId = ft_lgId;
                                home_ft.CreatedDate = DateTime.Now;
                                db.TblFootballTeam.Add(home_ft);
                                db.SaveChanges();

                            }
                            //----------------------------------------------------------Adding Upcoming Event table---------------------------------------------
                            //Change timestamp to local time
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            var time_stamp = Convert.ToDouble(data1.results[ii].time);
                            dtDateTime = dtDateTime.AddSeconds(time_stamp).ToLocalTime();
                            var shortdate = dtDateTime.ToShortDateString();
                            var shorttime = dtDateTime.ToShortTimeString();

                            TblUpcomingEvent up = new TblUpcomingEvent();
                            List<TblUpcomingEvent> events = new List<TblUpcomingEvent>();
                            decimal decUp = Decimal.Parse(data1.results[ii].id);
                            var up_lgId = db.TblLeague.Where(a => a.LeagueName == league_name).FirstOrDefault().LeagueId;
                            var up_home = db.TblFootballTeam.Where(a => a.FootballTeam == homeTeam).FirstOrDefault().FootballTeamId;
                            var up_away = db.TblFootballTeam.Where(a => a.FootballTeam == awayTeam).FirstOrDefault().FootballTeamId;

                            events = db.TblUpcomingEvent.ToList();
                            //Filter eventId
                            var upValue = events.Any(a => a.RapidEventId == decUp);
                            if (upValue == true)
                            {
                                //Update if data exist 
                                var id = events.Where(a => a.RapidEventId == decUp).FirstOrDefault().UpcomingEventId;
                                var upcoming = events.FirstOrDefault(s => s.UpcomingEventId.Equals(id));
                                upcoming.RapidEventId = decUp;
                                upcoming.LeagueId = up_lgId;
                                upcoming.HomeTeamId = up_home;
                                upcoming.AwayTeamId = up_away;
                                upcoming.Active = false;
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
                                up.Active = false;
                                up.EventDate = DateTime.Parse(shortdate);
                                up.EventTime = DateTime.Parse(shorttime);
                                db.TblUpcomingEvent.Add(up);
                                db.SaveChanges();
                            }
                            //--------------------------------------------------------Adding handicap table--------------------------------------------------
                            TblHandicap hd = new TblHandicap();
                            List<TblHandicap> handicaps = new List<TblHandicap>();
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
                                handicaps = db.TblHandicap.ToList();
                                //Filter eventId
                                var hanValue = handicaps.Any(a => a.RapidEventId == decHd);
                                if (hanValue == true)
                                {
                                    //Update if data exist 
                                    var id = handicaps.Where(a => a.RapidEventId == decHd).FirstOrDefault().HandicapId;
                                    var handicap = handicaps.FirstOrDefault(s => s.HandicapId.Equals(id));
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

                                }//end of save database
                            }//end of check asian line data is null
                            Console.WriteLine("Completed Data Result");
                        }//end of filter UCL
                         // Console.WriteLine("Completed Data" + ii + "Result");
                    }//end of fetch one data
                    //Console.WriteLine("Completed Page" + page + "Result");
                }//end of page
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Calculation Body Handicap From Users
        public bool BodyCalculation(int hgoal, int agoal, decimal rapid)
        {
            StringConcat @string = new StringConcat();
            Betting bettingCal = new Betting();
            int goalUnitInt = 0;
            decimal unit = 0;
            decimal totalAmount = 0;
            var betting = (from g in db.TblGambling
                           join d in db.TblGamblingDetails
                           on g.GamblingId equals d.GamblingId
                           where d.RapidEventId == rapid
                           where g.Active == true /*&& g.CreatedDate.Value.Date == DateTime.Now.Date*/ && g.GamblingTypeId == 1
                           select new
                           {
                               betType = g.GamblingTypeId,
                               betTeamCount = g.TeamCount,
                               betTeam = d.FootballTeamId,
                               betOver = d.Overs,
                               betUnder = d.Under,
                               betIsHome = d.IsHome,
                               betAmount = g.Amount,
                               betBody = d.BodyOdd,
                               betGoal = d.GoalOdd,
                               betUser = g.UserId
                           }).ToList();
            if (betting.Count != 0)
            {
                foreach (var item in betting)
                {
                    int diff;
                    bool isdone;
                    TblUserPosting userPosting = new TblUserPosting();
                    var userrole = db.TblUser.Where(a => a.UserId == item.betUser).FirstOrDefault().RoleId;
                    string no = "GW" + item.betUser + userrole + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString()
                            + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                    //Check betting is body or maung handicap
                    if (item.betType == 1)
                    {
                        var commission = db.TblUserCommission.Where(a => a.SubUserId == item.betUser && a.UserCommissionTypeId == 1).FirstOrDefault().SubUserCommission;
                        //Check betting is body or goal handicap
                        //----For body betting-----
                        if (item.betOver == false && item.betUnder == false)
                        {
                            int[] body = @string.CutBodyHandicap(item.betBody);
                            goalUnitInt = body[0];
                            unit = body[1];
                            var tempOver = db.TblMyanHandicapResult.ToList().Any(a => a.OverTeamId == item.betTeam && a.RapidEventId == rapid);
                            //----Check bet team is over----
                            if (tempOver == true)
                            {
                                if (item.betIsHome == true)
                                {
                                    //HomeGoal - AwayGoal
                                    diff = hgoal - agoal;
                                    totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, item.betAmount);
                                    Console.WriteLine(totalAmount);
                                }
                                else
                                {
                                    //AwayGoal - HomeGoal
                                    diff = agoal - hgoal;
                                    totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, item.betAmount);
                                    Console.WriteLine(totalAmount);
                                }
                            }
                            //----Check bet team is under----
                            else
                            {
                                if (item.betIsHome == true)
                                {
                                    //AwayGoal - HomeGoal
                                    diff = agoal - hgoal;
                                    unit *= -1;
                                    totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, item.betAmount);
                                    Console.WriteLine(totalAmount);

                                }
                                else
                                {
                                    //HomeGoal - AwayGoal
                                    diff = hgoal - agoal;
                                    unit *= -1;
                                    totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, item.betAmount);
                                    Console.WriteLine(totalAmount);
                                }
                            }//End of Over or Under 
                        }
                        //----For total goal betting-----
                        else
                        {
                            int[] goal = @string.CutBodyHandicap(item.betGoal);
                            goalUnitInt = goal[0];
                            unit = goal[1];
                            diff = 1 + 2;
                            if (item.betOver == true)
                            {
                                totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, item.betAmount);
                                Console.WriteLine(totalAmount);
                            }
                            if (item.betUnder == true)
                            {
                                unit *= -1;
                                totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, item.betAmount);
                                Console.WriteLine(totalAmount);
                            }
                        }//End of body or goal handicap
                        if (totalAmount >= item.betAmount)
                        {
                            var cashForuser= (int)(totalAmount * Math.Round((decimal)(commission / 100), 2));
                            isdone = GetMasterBalance((decimal)item.betUser, cashForuser,no);
                            totalAmount -= cashForuser;
                        }
                        if (totalAmount == 0)
                        {
                            isdone = GetMasterBalance((decimal)item.betUser, (decimal)item.betAmount, no);
                            //userPosting.Inward = 0;
                            //userPosting.Outward = item.betAmount;
                            //userPosting.TransactionTypeId = 8;
                        }
                        //userPosting.Inward =totalAmount;
                        //userPosting.Outward = 0;
                        //userPosting.TransactionTypeId = 7;
                        //userPosting.UserId = item.betUser;
                        //userPosting.PostingNo = no;
                        //userPosting.CreatedBy = item.betUser;
                        //userPosting.CreatedDate = DateTime.Now;
                        //userPosting.Active = true;
                        //db.TblUserPosting.Add(userPosting);
                        //db.SaveChanges();
                       
                    }
                }//end of foreach loop
            }//end of one event result
            return false;
        }

        // This is Test Method1
        public IActionResult Hola()
        {
            Betting bettingCal = new Betting();
            StringConcat @string = new StringConcat();
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

                int[] body = @string.CutBodyHandicap("2+50");
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
                        totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, betAmount);
                        Console.WriteLine(totalAmount);
                    }
                    else
                    {
                        //AwayGoal - HomeGoal
                        diff = a - h;
                        totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, betAmount);
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
                        totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, betAmount);
                        Console.WriteLine(totalAmount);

                    }
                    else
                    {
                        //HomeGoal - AwayGoal
                        diff = h - a;
                        unit *= -1;
                        totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, betAmount);
                        Console.WriteLine(totalAmount);
                    }
                }//End of Over or Under     
            }
            //----For total goal betting-----
            else
            {
                int[] goal = @string.CutBodyHandicap("1+50");
                goalUnitInt = goal[0];
                unit = goal[1];
                diff = a + h;
                if (betOver == true)
                {
                    totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, betAmount);
                    Console.WriteLine(totalAmount);
                }
                if (betUnder == true)
                {
                    unit *= -1;
                    totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, betAmount);
                    Console.WriteLine(totalAmount);
                }
            }//End of body or goal handicap
             //End of body or maung
             // /Home/Test
             //End of one event result
            return View();
        }

        // This is Test Method2
        public IActionResult Gg()
        {
            StringConcat @string = new StringConcat();
            int[] body = @string.CutBodyHandicap("=D");
            Console.WriteLine(body[0]);
            Console.WriteLine(body[1]);
            return View();
        }

        // Calculation Mix Handicap From Users
        public IActionResult MixCalculation()
        {
            var isTrue = true;
            var isWin = true;
            var result = (from g in db.TblGambling
                          join d in db.TblGamblingDetails
                          on g.GamblingId equals d.GamblingId
                          where g.Active == true && g.GamblingId <= 10003 && g.GamblingTypeId == 2
                          select new BetInfo
                          {
                              BetGambling = g.GamblingId,
                              BetType = g.GamblingTypeId,
                              BetTeamCount = (int)g.TeamCount,
                              BetTeam = (decimal)d.FootballTeamId,
                              BetOver = (bool)d.Overs,
                              BetUnder = (bool)d.Under,
                              BetIsHome = (bool)d.IsHome,
                              BetAmount = g.Amount,
                              BetBody = d.BodyOdd,
                              BetGoal = d.GoalOdd,
                              BetUser = g.UserId,
                              BetRapid = d.RapidEventId
                          }).ToList();

            var resultGroupBy = from r in result.ToList()
                                group r by r.BetGambling;

            List<TblGoalResult> goalResults = new List<TblGoalResult>();
            List<MixHelper> helpers = new List<MixHelper>();
            goalResults = db.TblGoalResult.ToList();
            int balance = 0;decimal _userId = 0;int _count = 0;
            foreach (var info in resultGroupBy)
            {
                var @id = info.Key;
                List<Details> details = new List<Details>();
                foreach (BetInfo r in info)
                {
                    if (isTrue)
                    {
                        balance = (int)r.BetAmount;
                        _userId = (decimal)r.BetUser;
                        var isFinished = goalResults.Any(a => Decimal.Parse(a.RapidEventId) == r.BetRapid);
                        if (isFinished)
                        {
                            details.Add(new Details
                            {
                                GoalOdd = r.BetBody,
                                BodyOdd = r.BetBody,
                                Overs = r.BetOver,
                                Under = r.BetUnder,
                                HomeResult = (int)goalResults.Where(a => Decimal.Parse(a.RapidEventId) == r.BetRapid).FirstOrDefault().HomeResult,
                                AwayResult = (int)goalResults.Where(a => Decimal.Parse(a.RapidEventId) == r.BetRapid).FirstOrDefault().AwayResult,
                                IsHome = r.BetIsHome,
                                RapidEventId = r.BetRapid,
                                GamblingId = r.BetGambling,
                                FootballTeamId = r.BetTeam
                            });
                        }
                        else
                        {
                            isTrue = isFinished;
                        }
                    }
                }
                if (isTrue)
                {
                    helpers.Add(new MixHelper
                    {
                        GamblingId = id,
                        Amount = balance,
                        User= _userId,
                        Count=_count,
                        Details = details
                    });
                }
                else
                {
                    details.Clear();
                    isTrue = true;
                }
            }
            foreach (var data in helpers)
            {
                StringConcat @string = new StringConcat();
                Betting bettingCal = new Betting();
                var userrole = db.TblUser.Where(a => a.UserId == data.User).FirstOrDefault().RoleId;
                string no = "GW" + data.User + userrole + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString()
                           + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() 
                           + DateTime.Now.Second.ToString();
                var commission = db.TblUserCommission.Where(a => a.SubUserId == data.User && a.UserCommissionTypeId == data.Count)
                                                     .FirstOrDefault().SubUserCommission;
                int goalUnitInt = 0;
                decimal unit = 0;
                decimal totalAmount = 0;
                decimal originalAmount = data.Amount;
                int winningAmount =data.Amount;
                foreach (var @item in data.Details)
                {
                    if (isWin == true)
                    {
                        int diff;
                        //Check betting is body or goal handicap
                        //----For body betting-----
                        if (@item.Overs == false && @item.Under == false)
                        {
                            int[] body = @string.CutBodyHandicap(@item.BodyOdd);
                            goalUnitInt = body[0];
                            unit = body[1];
                            var tempOver = db.TblMyanHandicapResult.ToList().Any(a => a.OverTeamId == @item.FootballTeamId && a.RapidEventId == item.RapidEventId);
                            //----Check bet team is over----
                            if (tempOver == true)
                            {
                                if ((bool)@item.IsHome)
                                {
                                    //HomeGoal - AwayGoal
                                    diff = @item.HomeResult - @item.AwayResult;
                                    totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, winningAmount);
                                    Console.WriteLine(totalAmount);
                                }
                                else
                                {
                                    //AwayGoal - HomeGoal
                                    diff = @item.AwayResult - @item.HomeResult;
                                    totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, winningAmount);
                                    Console.WriteLine(totalAmount);
                                }
                            }
                            //----Check bet team is under----
                            else
                            {
                                if ((bool)@item.IsHome)
                                {
                                    //AwayGoal - HomeGoal
                                    diff = @item.AwayResult - @item.HomeResult;
                                    unit *= -1;
                                    totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, winningAmount);
                                    Console.WriteLine(totalAmount);

                                }
                                else
                                {
                                    //HomeGoal - AwayGoal
                                    diff = @item.HomeResult - @item.AwayResult;
                                    unit *= -1;
                                    totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, winningAmount);
                                    Console.WriteLine(totalAmount);
                                }
                            }//End of Over or Under 
                        }
                        //----For total goal betting-----
                        else
                        {
                            int[] goal = @string.CutBodyHandicap(@item.GoalOdd);
                            goalUnitInt = goal[0];
                            unit = goal[1];
                            diff = 1 + 2;
                            if (@item.Overs == true)
                            {
                                totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, winningAmount);
                                Console.WriteLine(totalAmount);
                            }
                            if (@item.Under == true)
                            {
                                unit *= -1;
                                totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, winningAmount);
                                Console.WriteLine(totalAmount);
                            }

                        }//End of body or goal handicap 
                        if (totalAmount == 0) { isTrue = false; }
                        winningAmount = Convert.ToInt32(totalAmount);
                    }     
                }//End of gambling details
                if (winningAmount >= totalAmount)
                {
                    var cashForuser = (int)(winningAmount * Math.Round((decimal)(commission / 100), 2));
                    winningAmount -= cashForuser;
                    var isdone = GetMasterBalance((decimal)data.User, cashForuser, no);
                }
                if (winningAmount == 0)
                {
                    var isdone = GetMasterBalance((decimal)data.User,originalAmount, no);
                    //userPosting.Inward = 0;
                    //userPosting.Outward = item.betAmount;
                    //userPosting.TransactionTypeId = 8;
                }
                TblUserPosting userPosting = new TblUserPosting();
                if (isTrue)
                {
                    userPosting.Inward = winningAmount;
                    userPosting.Outward = 0;
                }
                else
                {
                    userPosting.Inward = 0;
                    userPosting.Outward = originalAmount;
                    isTrue = true;
                }
                userPosting.Active = true;
                userPosting.CreatedBy = 3;
                userPosting.CreatedDate = DateTime.Now;
                userPosting.PostingNo = "fdsfafsfsaf";
                userPosting.TransactionTypeId = 7;
                userPosting.UserId = 3;
                db.TblUserPosting.Add(userPosting);
                db.SaveChanges();
            }
            return View();
        }

        public bool GetMasterBalance(decimal id, decimal amount,string n)
        {
            decimal i = id;
            decimal com = 0;
            Dictionary<decimal, decimal> myDict = new Dictionary<decimal, decimal>();
            while (i != 1)
            {
                List<UserWithCommission> commissions = new List<UserWithCommission>();
                i = (decimal)db.TblUser.Where(a => a.UserId == i).FirstOrDefault().CreatedBy;
                if (i != 1)
                {
                    com = (decimal)db.TblUserCommission.Where(a => a.UserId == i && a.SubUserId == id && a.UserCommissionTypeId == 1)
                    .FirstOrDefault().UserCommission;
                    myDict.Add(i, com);
                }
            }
            //var fff = myDict.Count();
            foreach (KeyValuePair<decimal,decimal> kvp in myDict)
            {
               var win= (int)(amount * Math.Round(kvp.Value / 100, 2));
                amount -= win;
                TblUserPosting posting = new TblUserPosting
                {
                    Active = true,
                    CreatedBy = id,
                    UserId = kvp.Key,
                    CreatedDate = DateTime.Now,
                    Inward = win,
                    Outward = 0,
                    PostingNo = n,
                    TransactionTypeId = 3
                };
            db.TblUserPosting.Add(posting);
            db.SaveChanges();
        }
        TblUserPosting postingadmin = new TblUserPosting
        {
            Active = true,
            CreatedBy = id,
            UserId = 1,
            CreatedDate = DateTime.Now,
            Inward = amount,
            Outward = 0,
            PostingNo = n,
            TransactionTypeId = 3
        };
        db.TblUserPosting.Add(postingadmin);
            db.SaveChanges();
            return true;
        }
    }
}