using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Bloometa.Models;
using Bloometa.Models.TrackViewModels;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Bloometa.Controllers
{
    public class TrackController : Controller
    {
        public List<string> NetworkOptions = new List<string>();
        public MConnectionStrings Configuration { get; }
        public TrackController(IOptions<MConnectionStrings> Cnf)
        {
            Configuration = Cnf.Value;

            NetworkOptions.Add("Instagram");
        }

        public ActionResult Index(IndexViewModel Model)
        {
            using (SqlConnection dbConn = new SqlConnection())
            {
                dbConn.ConnectionString = Configuration.DB;
                //dbConn.ConnectionString = ConfigurationManager.ConnectionStrings["Bloometa_DB"].ConnectionString;
                dbConn.Open();

                SqlCommand RetrieveAccount =
                    new SqlCommand(@"
                        SELECT TOP(20)
                            [AccID], [Network], [Username], [Added]
                        FROM UAccounts
                        WHERE
                            [Removed] = 0
                        ORDER BY [Added] DESC", dbConn);
                using (SqlDataReader results = RetrieveAccount.ExecuteReader())
                {
                    Model.Recent = new List<UAccount>();
                    if (results.HasRows)
                    {
                        while (results.Read())
                        {
                            UAccount Account = new UAccount();
                            Account.AccID = (Guid)results["AccID"];
                            Account.Network = (string)results["Network"];
                            Account.Username = (string)results["Username"];
                            Account.Added = (DateTime)results["Added"];

                            Model.Recent.Add(Account);
                        }
                    }
                }
            }

            return View(Model);
        }

        // GET: Track/5
        public ActionResult Account(AccountViewModel Model, string ID)
        {
            Model.AccDetails = new UAccount();

            try
            {
                Model.AccDetails.AccID = new Guid(ID);
            }
            catch
            {
                return RedirectToAction("Index");
            }

            using (SqlConnection dbConn = new SqlConnection())
            {
                dbConn.ConnectionString = Configuration.DB;
                //dbConn.ConnectionString = ConfigurationManager.ConnectionStrings["Bloometa_DB"].ConnectionString;
                dbConn.Open();

                SqlCommand RetrieveAccount =
                    new SqlCommand("SELECT TOP(1) [Network], [Username], [Added] FROM UAccounts WHERE [AccID] = @AccID AND [Removed] = 0", dbConn);
                RetrieveAccount.Parameters.Add("@AccID", SqlDbType.UniqueIdentifier).Value = Model.AccDetails.AccID;

                using (SqlDataReader results = RetrieveAccount.ExecuteReader())
                {
                    if (results.HasRows)
                    {
                        ViewData["AccountFound"] = true;
                        while (results.Read())
                        {
                            Model.AccDetails.Network = (string)results["Network"];
                            Model.AccDetails.Username = (string)results["Username"];
                            Model.AccDetails.Added = (DateTime)results["Added"];
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }

                DateTime StartDate = DateTime.UtcNow.AddMonths(-1);

                SqlCommand RetrieveData =
                    new SqlCommand(@"
                        WITH ReportingPeriod AS (
                            SELECT Data.[AccID], CONVERT(DATETIME, CONVERT(DATE, Data.[Run])) as [Run], Data.[FollowCount], Data.[FollowerCount],
                                ROW_NUMBER() OVER(PARTITION BY CONVERT(DATETIME, CONVERT(DATE, Data.[Run])) ORDER BY Data.[Run] DESC) as [RowKey]
                            FROM UData Data
                            WHERE Data.AccID = @AccID
                            AND Data.[Run] BETWEEN @StartDate AND getutcdate()
                        ) SELECT ReportingPeriod.*
                        FROM ReportingPeriod
                        WHERE
                            ReportingPeriod.[RowKey] = 1
                        ORDER BY ReportingPeriod.[Run] ASC", dbConn);
                RetrieveData.Parameters.Add("@AccID", SqlDbType.UniqueIdentifier).Value = Model.AccDetails.AccID;
                RetrieveData.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = StartDate;

                Model.Reporting = new List<UData>();
                Model.ReportingFollowers = new List<int>();
                Model.ReportingFollowing = new List<int>();
                Model.ReportingDays = new List<DateTime>();
                using (SqlDataReader results = RetrieveData.ExecuteReader())
                {
                    int i = 0;
                    while (results.Read())
                    {
                        UData DataItem = new UData();
                        DataItem.Run = (DateTime)results["Run"];
                        DataItem.FollowCount = (int)results["FollowCount"];
                        DataItem.FollowerCount = (int)results["FollowerCount"];

                        Model.Reporting.Add(DataItem);
                        Model.ReportingFollowers.Add(DataItem.FollowerCount);
                        Model.ReportingFollowing.Add(DataItem.FollowCount);
                        Model.ReportingDays.Add(DataItem.Run);

                        if (i != 0)
                        {
                            Model.Reporting[i].FollowDifference =
                                DataItem.FollowCount - Model.Reporting[i - 1].FollowCount;
                            Model.Reporting[i].FollowerDifference =
                                DataItem.FollowerCount - Model.Reporting[i - 1].FollowerCount;
                        }
                        else
                        {
                            Model.Reporting[i].FollowDifference = 0;
                            Model.Reporting[i].FollowerDifference = 0;
                        }

                        i++;
                    }
                }
            }
            
            Model.Reporting.Reverse();
            return View(Model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewData["NetworkOptions"] = NetworkOptions;
            return View();
        }

        // POST: Track/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel Model, string Username, string Network)
        {
            ViewData["NetworkOptions"] = NetworkOptions;
            using (SqlConnection dbConn = new SqlConnection())
            {
                dbConn.ConnectionString = Configuration.DB;
                //dbConn.ConnectionString = ConfigurationManager.ConnectionStrings["Bloometa_DB"].ConnectionString;
                dbConn.Open();

                SqlCommand RetrieveAccount =
                    new SqlCommand(@"
                        SELECT TOP(1) [AccID]
                        FROM UAccounts
                        WHERE
                            [Username] = @Username
                            AND [Network] = @Network
                            AND [Removed] = 0", dbConn);
                RetrieveAccount.Parameters.Add("@Username", SqlDbType.NVarChar).Value = Username;
                RetrieveAccount.Parameters.Add("@Network", SqlDbType.NVarChar).Value = Network.ToLower();

                using (SqlDataReader results = RetrieveAccount.ExecuteReader())
                {
                    if (results.HasRows)
                    {
                        while (results.Read())
                        {
                            return RedirectToAction("Account", new { ID = results["AccID"].ToString() });
                        }
                    }
                }

                int FollowCount, FollowerCount;

                // Validate the new user exists
                switch (Network.ToLower())
                {
                    case "instagram":
                        using (WebClient wClient = new WebClient())
                        {
                            // If this try completes successfully, the account exists
                            try
                            {
                                var wcResponse = wClient.DownloadString(String.Format("https://instagram.com/{0}/?__a=1", Username));
                                JToken wcData = JObject.Parse(wcResponse).SelectToken("user");

                                FollowCount = Int32.Parse((string)wcData.SelectToken("follows")
                                    .SelectToken("count"));
                                FollowerCount = Int32.Parse((string)wcData.SelectToken("followed_by")
                                    .SelectToken("count"));
                            }
                            catch (WebException e)
                            {
                                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                                {
                                    ViewData["AccResponse"] = "Could not find a user by that name.";
                                }
                                else
                                {
                                    ViewData["AccResponse"] = "An unknown error occurred while connecting to Instagram.";
                                }

                                return View(Model);
                            }
                        }
                        break;
                    default:
                        ViewData["AccResponse"] = "Invalid network selected.";
                        return View(Model);
                }

                // Everything is all good so far! Add the account to DB to be tracked
                using(SqlCommand InsertAccount = new SqlCommand(@"
                    INSERT INTO UAccounts
                        ([Username], [Network])
                        OUTPUT INSERTED.[AccID]
                    VALUES
                        (@Username, @Network)", dbConn))
                {
                    InsertAccount.Parameters.Add("@Username", SqlDbType.NVarChar).Value = Username;
                    InsertAccount.Parameters.Add("@Network", SqlDbType.NVarChar).Value = Network.ToLower();

                    Guid AccID = new Guid();
                    using (SqlDataReader results = InsertAccount.ExecuteReader())
                    {
                        if (results.HasRows)
                        {
                            while (results.Read())
                            {
                                AccID = (Guid)results["AccID"];
                            }
                        }
                        else
                        {
                            ViewData["AccResponse"] = "An error occurred while writing the data.";
                            return View(Model);
                        }
                    }

                    // May as well insert the data we got earlier, rather than waste a net request.
                    // No need to manually enqueue, in this case.
                    if (AccID.ToString() != "00000000-0000-0000-0000-000000000000")
                    {
                        using (SqlCommand InsertDataRow = new SqlCommand(@"
                            INSERT INTO UData
                                ([AccID], [FollowCount], [FollowerCount])
                            VALUES
                                (@AccID, @FollowCount, @FollowerCount)", dbConn))
                        {
                            InsertDataRow.Parameters.Add("@AccID", SqlDbType.UniqueIdentifier).Value = AccID;
                            InsertDataRow.Parameters.Add("@FollowCount", SqlDbType.Int).Value = FollowCount;
                            InsertDataRow.Parameters.Add("@FollowerCount", SqlDbType.Int).Value = FollowerCount;

                            InsertDataRow.ExecuteNonQuery();

                            return RedirectToAction("Account", new { ID = AccID });
                        }
                    }
                    else
                    {
                        ViewData["AccResponse"] = "An error occurred while writing the data.";
                            return View(Model);
                    }
                }
            }
        }
    }
}