using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Background_Task_Api_Pulling.Models;
using RestSharp;
using Background_Task_Api_Pulling.CommonClass;
using Newtonsoft.Json;
using Hangfire;
using System.Collections.Generic;
using Background_Task_Api_Pulling.Models.Requests;
using Background_Task_Api_Pulling.Models.Data;
using System.Data;
using Background_Task_Api_Pulling.StoredProcedure;
using Microsoft.Extensions.Configuration;

namespace Background_Task_Api_Pulling.Controllers
{
    public class HomeController : Controller
    {
        private readonly Gambling_AppContext db;
        clsDBConnection vd = new clsDBConnection();
        public HomeController(Gambling_AppContext _db, IConfiguration configuration)
        {
            db = _db;
            clsPublicVariable.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            RecurringJob.AddOrUpdate(() => GetGoals(), "0 */1 * * *");
            RecurringJob.AddOrUpdate(() => MixCalculation(), "0 */1 * * *");
            RecurringJob.AddOrUpdate(() => GetData(), "0 */2 * * *");
            RecurringJob.AddOrUpdate(() => CalculateHomeHandicap(), "*/15 * * * *");
            RecurringJob.AddOrUpdate(() => CalculateAwayHandicap(), "*/15 * * * *");
            RecurringJob.AddOrUpdate(() => CalculateZeroHandicap(), "*/15 * * * *");
            RecurringJob.AddOrUpdate(() => UpdateHandicapFromPre(), "*/10 * * * *");

            //BackgroundJob.Schedule(() => PublishMessage(), TimeSpan.FromMilliseconds(2000));
            // RecurringJob.AddOrUpdate(() => Test1(), "*/15 * * * *");
            //RecurringJob.AddOrUpdate(()=>UpdateHandicapFromPre(),Cron.Minutely);
            // RecurringJob.AddOrUpdate(() => CalculateHomeHandicap(), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => CalculateAwayHandicap(), Cron.Hourly);
            //RecurringJob.AddOrUpdate(() => CalculateZeroHandicap(), Cron.Hourly);
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
                string hanString = "";
                string goalsString = "";
                var person = (from p in db.TblPreUpcomingEvent
                              join e in db.TblHandicap
                              on p.RapidEventId equals e.RapidEventId
                              where (e.HomeHandicap.Contains("-")) && p.Active == true
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
                if (person.Count != 0)
                {
                    foreach (var s in person)
                    {
                        TblMyanHandicapResult myanHandicapResult = new TblMyanHandicapResult();
                        //Calculate body handicap
                        var intResult = (int)((s.homeOdds - s.awayOdds) * 100);
                        var unit = rd.RoundValue(intResult);
                        if (s.homeHandicap.Length < 8)
                        {
                            decimal hanDecimal = Decimal.Parse(s.homeHandicap);
                            hanString = Math.Abs(hanDecimal).ToString();
                        }
                        else
                        {
                            var tmpArr = s.homeHandicap.Split(",");
                            var first = Math.Abs(Decimal.Parse(tmpArr[0]));
                            var second = Math.Abs(Decimal.Parse(tmpArr[1]));
                            hanString = first.ToString() + "," + second.ToString();
                        }
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
                        if (s.goalHandicap.Length < 8)
                        {
                            decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                            goalsString = Math.Abs(goalsDecimal).ToString();
                        }
                        else
                        {
                            var g_tmpArr = s.goalHandicap.Split(",");
                            var g_first = Math.Abs(Decimal.Parse(g_tmpArr[0]));
                            var g_second = Math.Abs(Decimal.Parse(g_tmpArr[1]));
                            goalsString = g_first.ToString() + "," + g_second.ToString();
                        }
                        var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                        var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                        var amountInt = Convert.ToInt32(amount);
                        int amountResult = amountInt + goalsUnit;
                        var positiveAmount = Math.Abs(amountResult);

                        if (positiveAmount <= 100)
                        {
                            goalResult = @string.Body(amountResult, (int)goals);
                        }
                        else
                        {
                            int newGoals = (int)goals + 1;
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
                        //Console.WriteLine("Complete 1 Hadicap result from   HOME ");
                    }//end of foreach loop
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           // return View();
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
                string hanString = "";
                string goalsString = "";
                var person = (from p in db.TblPreUpcomingEvent
                              join e in db.TblHandicap
                              on p.RapidEventId equals e.RapidEventId
                              where (e.AwayHandicap.Contains("-")) && p.Active == true
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
                if (person.Count != 0)
                {
                    foreach (var s in person)
                    {
                        TblMyanHandicapResult myanHandicapResult = new TblMyanHandicapResult();
                        //Calculate body handicap
                        var oddsResult = (decimal)(s.awayOdds - s.homeOdds);
                        var intResult = (int)(oddsResult * 100);
                        var unit = rd.RoundValue(intResult);
                        if (s.awayHandicap.Length < 8)
                        {
                            decimal hanDecimal = Decimal.Parse(s.awayHandicap);
                            hanString = Math.Abs(hanDecimal).ToString();
                        }
                        else
                        {
                            var tmpArr = s.awayHandicap.Split(",");
                            var first = Math.Abs(Decimal.Parse(tmpArr[0]));
                            var second = Math.Abs(Decimal.Parse(tmpArr[1]));
                            hanString = first.ToString() + "," + second.ToString();
                        }
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
                        if (s.goalHandicap.Length < 8)
                        {
                            decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                            goalsString = Math.Abs(goalsDecimal).ToString();
                        }
                        else
                        {
                            var g_tmpArr = s.goalHandicap.Split(",");
                            var g_first = Math.Abs(Decimal.Parse(g_tmpArr[0]));
                            var g_second = Math.Abs(Decimal.Parse(g_tmpArr[1]));
                            goalsString = g_first.ToString() + "," + g_second.ToString();
                        }
                        var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                        var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                        var amountInt = Convert.ToInt32(amount);
                        int amountResult = amountInt + goalsUnit;
                        var positiveAmount = Math.Abs(amountResult);

                        if (positiveAmount <= 100)
                        {
                            goalResult = @string.Body(amountResult, (int)goals);
                        }
                        else
                        {
                            int newGoals = (int)goals + 1;
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
                        //Console.WriteLine("Complete 1 jandicap result from   AWAY  ");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //return View();
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
                string goalsString = "";
                int o_team, u_team;
                decimal oddsResult;
                var person = (from p in db.TblPreUpcomingEvent
                              join e in db.TblHandicap
                              on p.RapidEventId equals e.RapidEventId
                              where e.HomeHandicap.Length == 3 && e.AwayHandicap.Length == 3 && p.Active == true
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
                if (person.Count != 0)
                {
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
                        if (s.goalHandicap.Length < 8)
                        {
                            decimal goalsDecimal = Decimal.Parse(s.goalHandicap);
                            goalsString = Math.Abs(goalsDecimal).ToString();
                        }
                        else
                        {
                            var g_tmpArr = s.goalHandicap.Split(",");
                            var g_first = Math.Abs(Decimal.Parse(g_tmpArr[0]));
                            var g_second = Math.Abs(Decimal.Parse(g_tmpArr[1]));
                            goalsString = g_first.ToString() + "," + g_second.ToString();
                        }
                        var goals = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().GoalUnit;
                        var amount = db.TblUnitHandicapFix.Where(a => a.Handicap.Equals(goalsString)).First().UnitAmount;
                        var amountInt = Convert.ToInt32(amount);
                        int amountResult = amountInt + goalsUnit;
                        var positiveAmount = Math.Abs(amountResult);

                        if (positiveAmount <= 100)
                        {
                            goalResult = @string.Body(amountResult, (int)goals);
                        }
                        else
                        {
                            int newGoals = (int)goals + 1;
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
                         // Console.WriteLine("Complete 1 result from   ZERO  ");
                    }//end of foreach loop
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //return View();
        }

        //Fetch Goals Result Data From RapidAPI
        public void GetGoals()
        {
            //Filtering with today date
            var eventId = db.TblPreUpcomingEvent.Where(a => a.Active == true).ToList();
            var goals = db.TblGoalResult.ToList();
            foreach (var c in eventId)
            {
                var ftValue = goals.Any(a => Decimal.Parse(a.RapidEventId) == c.RapidEventId);
                if (ftValue == false)
                {
                    //Record not exist
                    //Data fetch from API
                    var eventString = String.Concat("https://betsapi2.p.rapidapi.com/v1/bet365/result?event_id=", c.RapidEventId.ToString());
                    var client = new RestClient(eventString);
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                    request.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
                    IRestResponse response = client.Execute(request);
                    GoalResult data = JsonConvert.DeserializeObject<GoalResult>(response.Content);
                    int status = Convert.ToInt32(data.results[0].time_status);
                    DateTime dt = DateTime.Now.AddHours(6).AddMinutes(30);

                    //Check event is postponed or cancelled
                    if (status == 3)
                    {
                        TblGoalResult goalResult = new TblGoalResult();

                        var golArr = data.results[0].ss.Split("-");
                        goalResult.RapidEventId = c.RapidEventId.ToString();
                        goalResult.UpcomingEventId = c.PreUpcommingEventId;
                        goalResult.HomeResult = Convert.ToInt32(golArr[0]);
                        goalResult.AwayResult = Convert.ToInt32(golArr[1]);
                        goalResult.EventDate = dt.Date;
                        goalResult.EventDatetime = dt;
                        db.TblGoalResult.Add(goalResult);
                        db.SaveChanges();
                        BodyCalculation(Convert.ToInt32(golArr[0]), Convert.ToInt32(golArr[1]), (decimal)c.RapidEventId);
                        // Console.WriteLine("Complete 1 Body");
                    }
                    else if (status == 4 || status == 5)
                    {
                        TblGoalResult goalResult = new TblGoalResult();
                        goalResult.RapidEventId = c.RapidEventId.ToString();
                        goalResult.UpcomingEventId = 10004;
                        goalResult.HomeResult = -1;
                        goalResult.AwayResult = -1;
                        goalResult.EventDate = dt.Date;
                        goalResult.EventDatetime = dt;
                        db.TblGoalResult.Add(goalResult);
                        db.SaveChanges();
                        BodyCalculation(-1, -1, (decimal)c.RapidEventId);
                        //Console.WriteLine("Complete 1 Body");
                    }
                }
            }//end of foreach loop
            //return View();
        }

        //Fetch data from API
        public void GetData()
        {
            try
            {
                DateTime moment = DateTime.Now.AddHours(6).AddMinutes(30);
                string y = moment.Year.ToString();
                string m = moment.Month.ToString("d2");
                string d = moment.Day.ToString("d2");
                string date = y + m + d;
                //Calling data from RapidApI 
                var client = new RestClient("https://betsapi2.p.rapidapi.com/v1/bet365/upcoming?sport_id=1&day=" + date);
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
                    var client1 = new RestClient("https://betsapi2.p.rapidapi.com/v1/bet365/upcoming?sport_id=1&day=" + date + "&page=" + page);
                    var request1 = new RestRequest(Method.GET);
                    request1.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                    request1.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
                    IRestResponse response1 = client1.Execute(request1);
                    Football data1 = JsonConvert.DeserializeObject<Football>(response1.Content);

                    for (var ii = 0; ii < data1.results.Length; ii++)
                    {
                        var lastName = "";
                        var dd = data1.results[ii].league.name;
                        var status = Convert.ToInt32(data1.results[ii].time_status);

                        if (dd != null && dd != "")
                        {
                            var tmpArr = dd.Split(" ");
                            lastName = tmpArr[tmpArr.Count() - 1];
                        }
                        if (lastName.Equals("play")  )
                        {
                            Console.WriteLine("This is Esports data");
                        }else if (status != 0)
                        {
                            Console.WriteLine("This is Ended");
                        }
                        //if (dd.Equals("England Premier League") || dd.Equals("England Championship") || dd.Equals("Germany Bundesliga I") ||
                        //   dd.Equals("Italy Serie A") || dd.Equals("France Ligue 2"))   dd.Equals("Spain Primera Liga")
                        // if (dd.Equals("Spain Primera Liga") || dd.Equals("England Championship"))
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

                            //------------------------------------------------------------Adding into leageue table--------------------------------------------
                            TblLeague lg = new TblLeague();
                            var league_name = data1.results[ii].league.name;
                            var lgId = Decimal.Parse(data1.results[ii].league.id);
                            var lgValue = db.TblLeague.Any(a => a.RapidLeagueId == lgId);
                            if (!lgValue)
                            {
                                //Record not present
                                lg.LeagueName = league_name;
                                lg.RapidLeagueId = lgId;
                                lg.Active = false;
                                db.TblLeague.Add(lg);
                                db.SaveChanges();
                            }
                            //----------------------------------------------------Adding away team into football team table-----------------------------------
                            TblFootballTeam ft = new TblFootballTeam();
                            var awayTeam = data1.results[ii].away.name;
                            decimal decAway = Decimal.Parse(data1.results[ii].away.id);
                            var ft_lgId = db.TblLeague.Where(a => a.RapidLeagueId == lgId).FirstOrDefault().LeagueId;
                            var ftValue = db.TblFootballTeam.Any(a => a.RapidTeamId == decAway && a.LeagueId == ft_lgId);
                            if (!ftValue)
                            {
                                //no record
                                ft.FootballTeam = awayTeam;
                                ft.FootballTeamMyan = awayTeam;
                                ft.RapidTeamId = decAway;
                                ft.LeagueId = ft_lgId;
                                ft.CreatedDate = DateTime.Now.AddHours(6).AddMinutes(30);
                                db.TblFootballTeam.Add(ft);
                                db.SaveChanges();
                            }
                            //------------------------------------------------------Adding home team into football team table----------------------------------
                            TblFootballTeam home_ft = new TblFootballTeam();
                            var homeTeam = data1.results[ii].home.name;
                            decimal decHome = Decimal.Parse(data1.results[ii].home.id);
                            var ftValue1 = db.TblFootballTeam.Any(a => a.RapidTeamId == decHome && a.LeagueId == ft_lgId);
                            if (!ftValue1)
                            {
                                //no record
                                home_ft.FootballTeam = homeTeam;
                                home_ft.FootballTeamMyan = homeTeam;
                                home_ft.RapidTeamId = decHome;
                                home_ft.LeagueId = ft_lgId;
                                home_ft.CreatedDate = DateTime.Now.AddHours(6).AddMinutes(30);
                                db.TblFootballTeam.Add(home_ft);
                                db.SaveChanges();
                            }
                            //----------------------------------------------------------Adding Upcomming Event table---------------------------------------------
                            //Change timestamp to local time
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            var time_stamp = Convert.ToDouble(data1.results[ii].time);
                            dtDateTime = dtDateTime.AddSeconds(time_stamp).ToLocalTime();
                            DateTime dtDateTimeNew = dtDateTime.AddHours(6).AddMinutes(30);
                            //DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            //var time_stamp = Convert.ToDouble(data1.results[ii].time);
                            //dtDateTime = dtDateTime.AddSeconds(time_stamp).ToLocalTime();
                            //DateTime dtDateTimeNew = dtDateTime;
                            var shortdate = dtDateTimeNew.ToShortDateString();
                            var shorttime = dtDateTimeNew.ToShortTimeString();

                            TblUpcomingEvent up = new TblUpcomingEvent();
                            decimal decUp = Decimal.Parse(data1.results[ii].id);
                            var up_home = db.TblFootballTeam.Where(a => a.RapidTeamId == decHome && a.LeagueId == ft_lgId)
                                                            .FirstOrDefault().FootballTeamId;
                            var up_away = db.TblFootballTeam.Where(a => a.RapidTeamId == decAway && a.LeagueId == ft_lgId)
                                                            .FirstOrDefault().FootballTeamId;
                            //Filter eventId
                            var upValue = db.TblUpcomingEvent.Any(a => a.RapidEventId == decUp);
                            if (upValue == true)
                            {
                                //Update if data exist 
                                var id = db.TblUpcomingEvent.Where(a => a.RapidEventId == decUp).FirstOrDefault().UpcomingEventId;
                                var upcoming = db.TblUpcomingEvent.FirstOrDefault(s => s.UpcomingEventId.Equals(id));
                                upcoming.RapidEventId = decUp;
                                upcoming.LeagueId = ft_lgId;
                                upcoming.HomeTeamId = up_home;
                                upcoming.AwayTeamId = up_away;
                                upcoming.Active = false;
                                upcoming.EventDate = DateTime.Parse(shortdate);
                                upcoming.EventTime = dtDateTimeNew;
                                db.SaveChanges();
                            }
                            else
                            {
                                //Insert if data not exist
                                up.RapidEventId = decUp;
                                up.LeagueId = ft_lgId;
                                up.HomeTeamId = up_home;
                                up.AwayTeamId = up_away;
                                up.Active = false;
                                up.EventDate = DateTime.Parse(shortdate);
                                up.EventTime = dtDateTimeNew;
                                db.TblUpcomingEvent.Add(up);
                                db.SaveChanges();
                            }
                            //--------------------------------------------------------Adding handicap table--------------------------------------------------
                            decimal dec_overOdds = 0;
                            decimal dec_underOdds = 0;
                            string goalsHandicap = "0";
                            decimal dec_home_odds = 0;
                            decimal dec_away_odds = 0;
                            string value_h = "0";
                            string value = "0";
                            if (data2.results[0].asian_lines != null)
                            {
                                if (data2.results[0].asian_lines.sp != null)
                                { //sp
                                    decimal decHd = Decimal.Parse(data2.results[0].FI);
                                    if (data2.results[0].asian_lines.sp.asian_handicap != null && data2.results[0].asian_lines.sp.goal_line != null)
                                    {
                                        dec_home_odds = Decimal.Parse(data2.results[0].asian_lines.sp.asian_handicap.odds[0].odds);
                                        dec_away_odds = Decimal.Parse(data2.results[0].asian_lines.sp.asian_handicap.odds[1].odds);
                                        value_h = data2.results[0].asian_lines.sp.asian_handicap.odds[0].name;
                                        value = data2.results[0].asian_lines.sp.asian_handicap.odds[1].name;
                                        //Check class name of api whether it is name or handicap
                                        if (value == null && value_h == null)
                                        {
                                            value_h = data5.results[0].asian_lines.sp.asian_handicap.odds[0].handicap;
                                            value = data5.results[0].asian_lines.sp.asian_handicap.odds[1].handicap;
                                        }
                                   // }
                                    //if (data2.results[0].asian_lines.sp.goal_line != null)
                                    //
                                        dec_overOdds = Decimal.Parse(data2.results[0].asian_lines.sp.goal_line.odds[0].odds);
                                        dec_underOdds = Decimal.Parse(data2.results[0].asian_lines.sp.goal_line.odds[1].odds);
                                        goalsHandicap = data2.results[0].asian_lines.sp.goal_line.odds[0].name;
                                        if (goalsHandicap == null)
                                        {
                                            goalsHandicap = data2.results[0].asian_lines.sp.goal_line.odds[0].handicap;
                                        }
                                    }
                                    //Filter eventId
                                    var hanValue = db.TblHandicap.ToList().Any(a => a.RapidEventId == decHd);
                                    if (hanValue == true)
                                    {
                                        //Update if data exist 
                                        var id = db.TblHandicap.Where(a => a.RapidEventId == decHd).FirstOrDefault().HandicapId;
                                        var handicap = db.TblHandicap.FirstOrDefault(s => s.HandicapId.Equals(id));
                                        handicap.RapidEventId = decHd;
                                        handicap.HomeOdd = dec_home_odds;
                                        handicap.HomeHandicap = value_h;
                                        handicap.AwayOdd = dec_away_odds;
                                        handicap.AwayHandicap = value;
                                        handicap.OverOdd = dec_overOdds;
                                        handicap.UnderOdd = dec_underOdds;
                                        handicap.GoalHandicap = goalsHandicap;
                                        handicap.EventDatetime = dtDateTimeNew;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        TblHandicap hd = new TblHandicap
                                        {
                                            RapidEventId = decHd,
                                            HomeOdd = dec_home_odds,
                                            HomeHandicap = value_h,
                                            AwayOdd = dec_away_odds,
                                            AwayHandicap = value,
                                            EventDatetime = dtDateTimeNew,
                                            OverOdd = dec_overOdds,
                                            UnderOdd = dec_underOdds,
                                            GoalHandicap = goalsHandicap
                                        };
                                        db.TblHandicap.Add(hd);
                                        db.SaveChanges();
                                    }//end of save database
                                }
                            }//end of check asian line data is null
                            //Console.WriteLine("Completed Data Result");
                        }//end of filter UCL
                          Console.WriteLine("Completed Data" + ii + "Result");
                    }//end of fetch one data
                     Console.WriteLine("Completed Page" + page + "Result");
                }//end of page
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //return View();
        }

        // Calculation Mix Handicap From Users
        public void MixCalculation()
        {
            int diff; int gdiff = 0;
            var isTrue = true;
            var isWin = true;
            CalculateComm comm = new CalculateComm();
            var result = (from g in db.TblGambling
                          join d in db.TblGamblingDetails
                          on g.GamblingId equals d.GamblingId
                          where g.Active == true && g.GamblingTypeId == 2
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
                              BetRapid = d.RapidEventId,
                              BetIsHomeOdds = (bool)d.IsHomeBodyOdd
                          }).ToList();
            if (result.Count != 0)
            {
                var resultGroupBy = from r in result.ToList()
                                    group r by r.BetGambling;

                List<TblGoalResult> goalResults = new List<TblGoalResult>();
                List<MixHelper> helpers = new List<MixHelper>();
                List<TblUser> users = new List<TblUser>();

                List<TblUserCommission> commissions = new List<TblUserCommission>();
                goalResults = db.TblGoalResult.ToList();
                users = db.TblUser.ToList();
                commissions = db.TblUserCommission.ToList();

                int balance = 0; decimal _userId = 0; int _count = 0;
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
                            _count = r.BetTeamCount;
                            var isFinished = goalResults.Any(a => Decimal.Parse(a.RapidEventId) == r.BetRapid);
                            if (isFinished)
                            {
                                var h_goal = (int)goalResults.Where(a => Decimal.Parse(a.RapidEventId) == r.BetRapid).FirstOrDefault().HomeResult;
                                var a_goal = (int)goalResults.Where(a => Decimal.Parse(a.RapidEventId) == r.BetRapid).FirstOrDefault().AwayResult;
                                details.Add(new Details
                                {
                                    GoalOdd = r.BetBody,
                                    BodyOdd = r.BetBody,
                                    Overs = r.BetOver,
                                    Under = r.BetUnder,
                                    HomeResult = h_goal,
                                    AwayResult = a_goal,
                                    IsHome = r.BetIsHome,
                                    IsHomeBodyOdds = r.BetIsHomeOdds,
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
                            User = _userId,
                            Count = _count,
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
                    //  var userrole = users.Where(a => a.UserId == data.User).FirstOrDefault().RoleId;
                    //  string no = "GW" + data.User + userrole + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString()
                    // + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()
                    //  + DateTime.Now.Second.ToString();

                    int goalUnitInt = 0;
                    decimal unit = 0;
                    decimal totalAmount = 0;
                    decimal originalAmount = data.Amount;
                    int winningAmount = data.Amount;
                    foreach (var @item in data.Details)
                    {
                        if (isWin == true)
                        {
                            if (@item.HomeResult != -1 && item.AwayResult != -1)
                            {
                                //Check betting is body or goal handicap
                                //----For body betting-----
                                if (@item.Overs == false && @item.Under == false)
                                {
                                    int[] body = @string.CutBodyHandicap(@item.BodyOdd);
                                    goalUnitInt = body[0];
                                    unit = body[1];

                                    //----Check bet team is over----
                                    if ((item.IsHomeBodyOdds == true && item.IsHome == true) || (item.IsHome == false && item.IsHomeBodyOdds == false))
                                    {
                                        if ((bool)@item.IsHome)
                                        {
                                            //HomeGoal - AwayGoal
                                            diff = @item.HomeResult - @item.AwayResult;
                                            totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, winningAmount);
                                            //Console.WriteLine(totalAmount);
                                        }
                                        else
                                        {
                                            //AwayGoal - HomeGoal
                                            diff = @item.AwayResult - @item.HomeResult;
                                            totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, winningAmount);
                                            //Console.WriteLine(totalAmount);
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
                                            //Console.WriteLine(totalAmount);

                                        }
                                        else
                                        {
                                            //HomeGoal - AwayGoal
                                            diff = @item.HomeResult - @item.AwayResult;
                                            unit *= -1;
                                            totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, winningAmount);
                                            //Console.WriteLine(totalAmount);
                                        }
                                    }//End of Over or Under 

                                }
                                //----For total goal betting-----
                                else
                                {
                                    gdiff = item.AwayResult + item.HomeResult;
                                    int[] goal = @string.CutBodyHandicap(@item.GoalOdd);
                                    goalUnitInt = goal[0];
                                    unit = goal[1];
                                    if (@item.Overs == true)
                                    {
                                        totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, gdiff, unit, winningAmount);
                                        //Console.WriteLine(totalAmount);
                                    }
                                    if (@item.Under == true)
                                    {
                                        unit *= -1;
                                        totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, gdiff, unit, winningAmount);
                                        // Console.WriteLine(totalAmount);
                                    }

                                }//End of body or goal handicap 
                            }
                            else
                            {
                                totalAmount = winningAmount;
                            }
                            if (totalAmount == 0) { isWin = false; winningAmount = 0; }
                            else { winningAmount = Convert.ToInt32(totalAmount); }
                        }
                    }
                    //End of gambling details

                    //Save Function and commission calculation function
                    isWin = SaveCommonFunc(originalAmount, winningAmount, data.GamblingId, data.Count, data.User);
                    bool os = comm.CalculateCommissionForWinLose(data.GamblingId);
                    Console.WriteLine("Complete 1");
                }
            }
            //return View();
        }

        //Fetch and Update Handicap From PreUpcomming Table
        public void UpdateHandicapFromPre()
        {
            //try
            //{
            //Filtering with today date
            DateTime dt = DateTime.Now;
            var today_event = db.TblPreUpcomingEvent.Where(a => a.Active == true && a.EventTime.Value>=dt).ToList();
            var today_league = db.TblConfirmLeague.Where(a => a.Active == true).ToList();
            var today_handicap = (from h in today_event
                                  join pre in db.TblHandicap
                                 on h.RapidEventId equals pre.RapidEventId
                                 select pre).ToList();

            foreach (var lgItem in today_league)
            {
                string date = DateTime.Now.Day.ToString();
                //Calling data from RapidApI 
                var client = new RestClient("https://betsapi2.p.rapidapi.com/v1/bet365/upcoming?sport_id=1&league_id=" + lgItem.RapidLeagueId + "&day=" + date);
                var request = new RestRequest(Method.GET);
                request.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                request.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
                IRestResponse response = client.Execute(request);
                Football main_data = JsonConvert.DeserializeObject<Football>(response.Content);

                //Get eventId of first array result
                var eventId = main_data.results[0].id;

                //Get total result from api data and calculate page
                var total = main_data.pager.total;
                int data_page = total / 50;
                if (total % 50 != 0)
                {
                    data_page += 1;
                }

                for (var page = 1; page <= data_page; page++)
                {
                    //Calling event data from RapidApI 
                    var client1 = new RestClient("https://betsapi2.p.rapidapi.com/v1/bet365/upcoming?sport_id=1&league_id=" + lgItem.RapidLeagueId + "&day=" + date);
                    var request1 = new RestRequest(Method.GET);
                    request1.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                    request1.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
                    IRestResponse response1 = client1.Execute(request1);
                    Football main_data1 = JsonConvert.DeserializeObject<Football>(response1.Content);

                    for (var ii = 0; ii < main_data1.results.Length; ii++)
                    {
                        //-------Adding Upcoming Event table----------
                        decimal decUp = Decimal.Parse(main_data.results[ii].id);
                        int status = Convert.ToInt32(main_data1.results[ii].time_status);
                    
                        //Filter eventId
                        var upValue = today_event.Any(a => a.RapidEventId == decUp);
                        if (upValue == true)
                        {
                            if (status == 0)
                            {
                                //Fetch  Odds data from RapidApI 
                                eventId = main_data1.results[ii].id;
                                var resultString = String.Concat("https://betsapi2.p.rapidapi.com/v3/bet365/prematch?FI=", eventId);
                                var client2 = new RestClient(resultString);
                                var request2 = new RestRequest(Method.GET);
                                request2.AddHeader("x-rapidapi-key", "4344a0e9c3mshdcf753076fef263p11670fjsne4f7ed9cd500");
                                request2.AddHeader("x-rapidapi-host", "betsapi2.p.rapidapi.com");
                                IRestResponse response2 = client2.Execute(request2);
                                Odds odd_data = JsonConvert.DeserializeObject<Odds>(response2.Content);
                                Handicap hadicap_data = JsonConvert.DeserializeObject<Handicap>(response2.Content);

                                //Change timestamp to local time
                                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                var time_stamp = Convert.ToDouble(main_data.results[ii].time);
                                dtDateTime = dtDateTime.AddSeconds(time_stamp).ToLocalTime();
                                DateTime dtDateTimeNew = dtDateTime.AddHours(6).AddMinutes(30);
                                var shortdate = dtDateTimeNew.ToShortDateString();
                                var shorttime = dtDateTimeNew.ToShortTimeString();

                                //Update if data exist 
                                var id = today_event.Where(a => a.RapidEventId == decUp).FirstOrDefault().PreUpcommingEventId;
                                var upcoming = today_event.FirstOrDefault(s => s.PreUpcommingEventId.Equals(id));
                                upcoming.RapidEventId = decUp;
                                upcoming.EventDate = DateTime.Parse(shortdate);
                                upcoming.EventTime = dtDateTime;
                                db.SaveChanges();

                                //---Adding handicap table----
                                decimal dec_overOdds = 0;
                                decimal dec_underOdds = 0;
                                string goalsHandicap = "0";
                                decimal dec_home_odds = 0;
                                decimal dec_away_odds = 0;
                                string value_h = "0";
                                string value = "0";
                                if (odd_data.results[0].asian_lines != null)
                                {
                                    if (odd_data.results[0].asian_lines.sp != null)
                                    { //sp
                                        decimal decHd = Decimal.Parse(odd_data.results[0].FI);
                                        if (odd_data.results[0].asian_lines.sp.asian_handicap != null && odd_data.results[0].asian_lines.sp.goal_line != null)
                                        {
                                            dec_home_odds = Decimal.Parse(odd_data.results[0].asian_lines.sp.asian_handicap.odds[0].odds);
                                            dec_away_odds = Decimal.Parse(odd_data.results[0].asian_lines.sp.asian_handicap.odds[1].odds);
                                            value_h = odd_data.results[0].asian_lines.sp.asian_handicap.odds[0].name;
                                            value = odd_data.results[0].asian_lines.sp.asian_handicap.odds[1].name;
                                            //Check class name of api whether it is name or handicap
                                            if (value == null && value_h == null)
                                            {
                                                value_h = hadicap_data.results[0].asian_lines.sp.asian_handicap.odds[0].handicap;
                                                value = hadicap_data.results[0].asian_lines.sp.asian_handicap.odds[1].handicap;
                                            }
                                            // }
                                            //if (data2.results[0].asian_lines.sp.goal_line != null)
                                            //
                                            dec_overOdds = Decimal.Parse(hadicap_data.results[0].asian_lines.sp.goal_line.odds[0].odds);
                                            dec_underOdds = Decimal.Parse(hadicap_data.results[0].asian_lines.sp.goal_line.odds[1].odds);
                                            goalsHandicap = hadicap_data.results[0].asian_lines.sp.goal_line.odds[0].name;
                                            if (goalsHandicap == null)
                                            {
                                                goalsHandicap = hadicap_data.results[0].asian_lines.sp.goal_line.odds[0].handicap;
                                            }
                                        }
                                        //Filter eventId
                                        var hanValue = today_handicap.Any(a => a.RapidEventId == decHd);
                                        if (hanValue == true)
                                        {
                                            //Update if data exist 
                                            var hid = today_handicap.Where(a => a.RapidEventId == decHd).FirstOrDefault().HandicapId;
                                            var handicap = today_handicap.FirstOrDefault(s => s.HandicapId.Equals(hid));
                                            handicap.RapidEventId = decHd;
                                            handicap.HomeOdd = dec_home_odds;
                                            handicap.HomeHandicap = value_h;
                                            handicap.AwayOdd = dec_away_odds;
                                            handicap.AwayHandicap = value;
                                            handicap.OverOdd = dec_overOdds;
                                            handicap.UnderOdd = dec_underOdds;
                                            handicap.GoalHandicap = goalsHandicap;
                                            handicap.EventDatetime = dtDateTimeNew;
                                            db.SaveChanges();
                                        }//end of save database
                                        else
                                        {
                                            TblHandicap hd = new TblHandicap
                                            {
                                                RapidEventId = decHd,
                                                HomeOdd = dec_home_odds,
                                                HomeHandicap = value_h,
                                                AwayOdd = dec_away_odds,
                                                AwayHandicap = value,
                                                EventDatetime = dtDateTimeNew,
                                                OverOdd = dec_overOdds,
                                                UnderOdd = dec_underOdds,
                                                GoalHandicap = goalsHandicap
                                            };
                                            db.TblHandicap.Add(hd);
                                            db.SaveChanges();
                                        }//end of save database
                                    }
                                }//end of check asian line data is null
                            }//end of filter UCL
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    Console.WriteLine(ex.Message);
                    //}
                }
            }
        }

        //-----------------------------------------------------------Call Back Methods-------------------

        // Calculation Body Handicap From Users
        public bool BodyCalculation(int hgoal, int agoal, decimal rapid)
        {
            StringConcat @string = new StringConcat();
            Betting bettingCal = new Betting();
            CalculateComm comm = new CalculateComm();
            List<TblUser> users = new List<TblUser>();
            users = db.TblUser.ToList();
            int goalUnitInt = 0;
            decimal unit = 0;
            decimal totalAmount = 0;
            var betting = (from g in db.TblGambling
                           join d in db.TblGamblingDetails
                           on g.GamblingId equals d.GamblingId
                           where d.RapidEventId == rapid
                           where g.Active == true/*&& g.CreatedDate.Value.Date == DateTime.Now.Date*/ && g.GamblingTypeId == 1
                           select new BetInfo
                           {
                               BetType = g.GamblingTypeId,
                               BetGambling = g.GamblingId,
                               BetTeamCount = (int)g.TeamCount,
                               BetTeam = (decimal)d.FootballTeamId,
                               BetOver = (bool)d.Overs,
                               BetUnder = (bool)d.Under,
                               BetIsHome = d.IsHome,
                               BetAmount = g.Amount,
                               BetBody = d.BodyOdd,
                               BetGoal = d.GoalOdd,
                               BetUser = g.UserId,
                               BetIsHomeOdds = (bool)d.IsHomeBodyOdd
                           }).ToList();
            if (betting.Count != 0)
            {
                foreach (var item in betting)
                {
                    int diff;
                    //Check event is postponed or cancelled
                    if (hgoal != -1 && agoal != -1)
                    {
                        //Check betting is body or goal betting
                        //----For body betting-----
                        if (item.BetOver == false && item.BetUnder == false)
                        {
                            int[] body = @string.CutBodyHandicap(item.BetBody);
                            goalUnitInt = body[0];
                            unit = body[1];

                            //----Check bet team is over----
                            if ((item.BetIsHomeOdds == true && item.BetIsHome == true) || (item.BetIsHomeOdds == false && item.BetIsHome == false))
                            {
                                if (item.BetIsHome == true)
                                {
                                    //HomeGoal - AwayGoal
                                    diff = hgoal - agoal;
                                    totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, item.BetAmount);
                                    //Console.WriteLine(totalAmount);
                                }
                                else
                                {
                                    //AwayGoal - HomeGoal
                                    diff = agoal - hgoal;
                                    totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, item.BetAmount);
                                    //Console.WriteLine(totalAmount);
                                }
                            }
                            //----Check bet team is under----
                            else
                            {
                                if (item.BetIsHome == true)
                                {
                                    //AwayGoal - HomeGoal
                                    diff = agoal - hgoal;
                                    unit *= -1;
                                    totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, item.BetAmount);
                                    //Console.WriteLine(totalAmount);
                                }
                                else
                                {
                                    //HomeGoal - AwayGoal
                                    diff = hgoal - agoal;
                                    unit *= -1;
                                    totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, item.BetAmount);
                                    // Console.WriteLine(totalAmount);
                                }
                            }//End of Over or Under 
                        }
                        //----For total goal betting-----
                        else
                        {
                            int[] goal = @string.CutBodyHandicap(item.BetGoal);
                            goalUnitInt = goal[0];
                            unit = goal[1];
                            diff = hgoal + agoal;
                            if (item.BetOver == true)
                            {
                                totalAmount = bettingCal.WinOrLoseOver(goalUnitInt, diff, unit, item.BetAmount);
                                //Console.WriteLine(totalAmount);
                            }
                            if (item.BetUnder == true)
                            {
                                unit *= -1;
                                totalAmount = bettingCal.WinOrLoseUnder(goalUnitInt, diff, unit, item.BetAmount);
                                //Console.WriteLine(totalAmount);
                            }
                        }//End of body or goal betting

                        //Save Function and commission calculation function
                        var isWin = SaveCommonFunc((decimal)item.BetAmount, totalAmount, item.BetGambling, item.BetTeamCount, (decimal)item.BetUser);
                        bool isa = comm.CalculateCommissionForWinLose(item.BetGambling);
                    }
                    else
                    {
                        var isWin = SavePostponedFunc((decimal)item.BetAmount, item.BetGambling, (decimal)item.BetUser);
                    }//End of event is postponed check
                }//end of foreach loop
            }//end of one event result
            return false;
        }

        //Save Method For Mix and Body Calculation
        public bool SaveCommonFunc(decimal og, decimal wa, decimal gid, int c, decimal uid)
        {
            bool result = false;
            var maxAmount = og;
            List<TblUser> users = new List<TblUser>();
            List<TblGambling> tblGamblings = new List<TblGambling>();
            users = db.TblUser.ToList();
            tblGamblings = db.TblGambling.Where(a => a.Active == true).ToList();
            DateTime today = DateTime.Now.AddHours(6).AddMinutes(30);
            var userrole = users.Where(a => a.UserId == uid).FirstOrDefault().RoleId;
            string no = "GW" + uid + userrole + today.Year.ToString() + today.Month.ToString()
                        + today.Day.ToString() + today.Hour.ToString() + today.Minute.ToString()
                        + today.Second.ToString();
            for (int i = 1; i <= c; i++)
            {
                maxAmount *= 2;
            }

            decimal tempdata = Math.Round((wa / maxAmount) * 100, 2);
            TblUserPosting userPosting = new TblUserPosting();
            TblUserPosting userPosting_parent = new TblUserPosting();
            TblGamblingWin gamblingWin = new TblGamblingWin();

            var parentUser = users.Where(a => a.UserId == uid).First().CreatedBy;
            if (wa >= og)
            {
                gamblingWin.WinAmount = wa;
                gamblingWin.LoseAmount = 0;
                gamblingWin.Wlpercent = tempdata;
            }
            else
            {
                gamblingWin.WinAmount = 0;
                gamblingWin.LoseAmount = og;
                gamblingWin.Wlpercent = tempdata;
            }
            gamblingWin.Active = true;
            gamblingWin.GamblingId = gid;
            gamblingWin.GamblingTypeId = 2;
            gamblingWin.UserId = uid;
            gamblingWin.GoalResultId = 0;
            gamblingWin.BetAmount = og;
            db.TblGamblingWin.Add(gamblingWin);
            db.SaveChanges();

            if (wa != 0)
            {
                userPosting.Inward = wa;
                userPosting.Outward = 0;
                userPosting.TransactionTypeId = 7;
                userPosting_parent.Inward = 0;
                userPosting_parent.Outward = wa;
                userPosting_parent.TransactionTypeId = 8;
            }
            else
            {
                userPosting.Inward = 0;
                userPosting.Outward = og;
                userPosting.TransactionTypeId = 8;
                userPosting_parent.Inward = og;
                userPosting_parent.Outward = 0;
                userPosting_parent.TransactionTypeId = 7;
                result = true;
            }
            userPosting.Active = true;
            userPosting.CreatedBy = uid;
            userPosting.CreatedDate = today;
            userPosting.PostingNo = no;
            userPosting.UserId = uid;
            userPosting.GamblingId = gid;
            db.TblUserPosting.Add(userPosting);
            db.SaveChanges();

            userPosting_parent.Active = true;
            userPosting_parent.CreatedBy = uid;
            userPosting_parent.CreatedDate = today;
            userPosting_parent.PostingNo = no;
            userPosting_parent.GamblingId = gid;
            userPosting_parent.UserId = parentUser;
            db.TblUserPosting.Add(userPosting_parent);
            db.SaveChanges();

            var gamId = tblGamblings.Where(a => a.GamblingId == gid).FirstOrDefault();
            gamId.Active = false;
            db.SaveChanges();
            return result;
        }

        //Save Method For Event is PostPoned or Cancelled
        public bool SavePostponedFunc(decimal og, decimal gid, decimal uid)
        {
            bool result = false;
            List<TblUser> users = new List<TblUser>();
            List<TblGambling> tblGamblings = new List<TblGambling>();
            users = db.TblUser.ToList();
            tblGamblings = db.TblGambling.Where(a => a.Active == true).ToList();
            DateTime today = DateTime.Now.AddHours(6).AddMinutes(30);
            var userrole = users.Where(a => a.UserId == uid).FirstOrDefault().RoleId;
            string no = "GW" + uid + userrole + today.Year.ToString() + today.Month.ToString()
                        + today.Day.ToString() + today.Hour.ToString() + today.Minute.ToString()
                        + today.Second.ToString();

            TblUserPosting userPosting = new TblUserPosting();
            TblUserPosting userPosting_parent = new TblUserPosting();

            var parentUser = users.Where(a => a.UserId == uid).First().CreatedBy;

            userPosting.Inward = og;
            userPosting.Outward = 0;
            userPosting.TransactionTypeId = 7;
            userPosting.Active = true;
            userPosting.CreatedBy = uid;
            userPosting.CreatedDate = today;
            userPosting.PostingNo = no;
            userPosting.UserId = uid;
            userPosting.GamblingId = gid;
            db.TblUserPosting.Add(userPosting);
            db.SaveChanges();

            userPosting_parent.Inward = 0;
            userPosting_parent.Outward = og;
            userPosting_parent.TransactionTypeId = 8;
            userPosting_parent.Active = true;
            userPosting_parent.CreatedBy = uid;
            userPosting_parent.CreatedDate = today;
            userPosting_parent.PostingNo = no;
            userPosting_parent.GamblingId = gid;
            userPosting_parent.UserId = parentUser;
            db.TblUserPosting.Add(userPosting_parent);
            db.SaveChanges();

            var gamId = tblGamblings.Where(a => a.GamblingId == gid).FirstOrDefault();
            gamId.Active = false;
            db.SaveChanges();
            return result;
        }

        public IActionResult Test()
        {
            var person = (from p in db.TblUpcomingEvent
                          join e in db.TblHandicap
                          on p.RapidEventId equals e.RapidEventId
                          //where e.HomeHandicap.Length == 3 && e.AwayHandicap.Length == 3 && p.Active == true
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
                              preEventId = p.UpcomingEventId
                          }).ToList();
            return View();
        }
    }
}
