using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Bloometa.Models;

namespace Bloometa.Controllers
{
    [Route("/API/1.0/Account")]
    public class APIv1_0__AccountController : Controller
    {
        public MConnectionStrings Configuration { get; }
        public APIv1_0__AccountController(IOptions<MConnectionStrings> Cnf)
        {
            Configuration = Cnf.Value;
        }

        public List<string> AcceptableNetworks { get {
            return new List<string> {
                "twitter", "instagram"
            };
        } }

        [HttpGet]
        [Route("/API/1.0/Account/Lookup")]
        public async Task<APIAccountResponse> Lookup([FromQuery] Guid[] a)
        {
            List<UAccount> Accounts = new List<UAccount>();
            List<UError> Errors = new List<UError>();

            if (a.Length > 100)
            {
                Errors.Add(new UError() {
                    ID = 010,
                    Message = @"Number of accounts to lookup was out of bounds,
                                lookups are limited to 100 accounts. Results
                                will be truncated."
                });
            } else
            {
                IEnumerable<Guid> DistinctAccs = a.Distinct();
                foreach (Guid Account in DistinctAccs)
                {
                    Accounts.Add(new UAccount() {
                        AccID = Guid.NewGuid(),
                        Network = "Twitter",
                        Username = Account.ToString(),
                        Added = DateTime.UtcNow
                    });
                }

                using (SqlConnection dbConn = new SqlConnection())
                {
                    dbConn.ConnectionString = Configuration.DB;
                    await dbConn.OpenAsync();

                    SqlCommand LookupAccounts =
                        new SqlCommand(@"
                            SELECT TOP(100)
                                [AccID], [Network], [Username], [Added]
                            FROM UAccounts
                            WHERE
                                [AccID] IN (@AccID)
                                AND [Removed] = 0", dbConn);
                    
                    LookupAccounts.Parameters.Add("@AccID", SqlDbType.NVarChar).Value = String.Join(",", DistinctAccs.Select(x => x.ToString()));

                    using (SqlDataReader results = await LookupAccounts.ExecuteReaderAsync())
                    {
                        if (results.HasRows)
                        {
                            while (await results.ReadAsync())
                            {
                                Accounts.Add(new UAccount() {
                                    AccID = (Guid)results["AccID"],
                                    Network = (string)results["Network"],
                                    Username = (string)results["Username"],
                                    Added = (DateTime)results["Added"]
                                });
                            }
                        }
                    }

                    /* WITH ReportingPeriod AS (
                        SELECT Data.[AccID], CONVERT(DATETIME, CONVERT(DATE, Data.[Run])) as [Run], Data.[FollowCount], Data.[FollowerCount],
                            ROW_NUMBER() OVER(PARTITION BY CONVERT(DATETIME, CONVERT(DATE, Data.[Run])) ORDER BY Data.[Run] DESC) as [RowKey]
                        FROM UData Data
                        WHERE Data.AccID IN (@AccID)
                        AND Data.[Run] BETWEEN @StartDate AND @EndDate
                    ) SELECT
                        ReportingPeriod.*, Account.[AccID]
                    FROM ReportingPeriod, UAccounts Account
                    WHERE
                        ReportingPeriod.[RowKey] = 1
                        AND Account.[AccID] IN (@AccID)
                        AND Account.[Removed] = 0
                    ORDER BY ReportingPeriod.[Run] ASC */
                }
            }
            
            return new APIAccountResponse()
            {
                ResponseErrors = Errors,
                Accounts = Accounts
            };
        }

        [HttpGet]
        [Route("/API/1.0/Account/Search")]
        public async Task<APIAccountResponse> Search(
            [FromQuery] string a,
            [FromQuery] string network,
            [FromQuery] bool socialid = false)
        {
            List<UAccount> Accounts = new List<UAccount>();
            List<UError> Errors = new List<UError>();

            if (String.IsNullOrEmpty(a))
            {
                Errors.Add(new UError() {
                    ID = 020,
                    Message = "Username required."
                });
            } else
            {
                if (a.Length < 3)
                {
                    Errors.Add(new UError() {
                        ID = 030,
                        Message = "Username too short."
                    });
                } else
                {
                    if (socialid)
                    {
                        if (String.IsNullOrEmpty(network))
                        {
                            Errors.Add(new UError() {
                                ID = 040,
                                Message = @"Network is required when looking-up
                                            by social ID."
                            });
                        } else
                        {
                            network = network.ToLower();
                            if (!AcceptableNetworks.Contains(network))
                            {
                                Errors.Add(new UError() {
                                    ID = 050,
                                    Message = @"Specified network is not valid
                                                on this service."
                                });
                            } else
                            {
                                using (SqlConnection dbConn = new SqlConnection())
                                {
                                    dbConn.ConnectionString = Configuration.DB;
                                    await dbConn.OpenAsync();

                                    SqlCommand SearchAccount =
                                        new SqlCommand(@"
                                            SELECT TOP(1)
                                                [AccID], [Network], [Username], [Added]
                                            FROM UAccounts
                                            WHERE
                                                [UserID] = @UserID
                                                AND [Network] = @Network
                                                AND [Removed] = 0", dbConn);
                                    SearchAccount.Parameters.Add("@UserID", SqlDbType.NVarChar).Value = a;
                                    SearchAccount.Parameters.Add("@Network", SqlDbType.NVarChar).Value = network;

                                    using (SqlDataReader results = await SearchAccount.ExecuteReaderAsync())
                                    {
                                        if (results.HasRows)
                                        {
                                            while (await results.ReadAsync())
                                            {
                                                Accounts.Add(new UAccount() {
                                                    AccID = (Guid)results["AccID"],
                                                    Network = (string)results["Network"],
                                                    Username = (string)results["Username"],
                                                    Added = (DateTime)results["Added"]
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } else
                    {
                        if (String.IsNullOrEmpty(network))
                        {
                            using (SqlConnection dbConn = new SqlConnection())
                            {
                                dbConn.ConnectionString = Configuration.DB;
                                await dbConn.OpenAsync();

                                SqlCommand SearchAccount =
                                    new SqlCommand(@"
                                        SELECT TOP(100)
                                            [AccID], [Network], [Username], [Added]
                                        FROM UAccounts
                                        WHERE
                                            [Username] LIKE @Username
                                            AND [Removed] = 0", dbConn);
                                SearchAccount.Parameters.Add("@Username", SqlDbType.NVarChar).Value = "%" + a + "%";

                                using (SqlDataReader results = await SearchAccount.ExecuteReaderAsync())
                                {
                                    if (results.HasRows)
                                    {
                                        while (await results.ReadAsync())
                                        {
                                            Accounts.Add(new UAccount() {
                                                AccID = (Guid)results["AccID"],
                                                Network = (string)results["Network"],
                                                Username = (string)results["Username"],
                                                Added = (DateTime)results["Added"]
                                            });
                                        }
                                    }
                                }
                            }
                        } else
                        {
                            network = network.ToLower();
                            if (!AcceptableNetworks.Contains(network))
                            {
                                Errors.Add(new UError() {
                                    ID = 050,
                                    Message = @"Specified network is not valid
                                                on this service."
                                });
                            } else
                            {
                                using (SqlConnection dbConn = new SqlConnection())
                                {
                                    dbConn.ConnectionString = Configuration.DB;
                                    await dbConn.OpenAsync();

                                    SqlCommand SearchAccount =
                                        new SqlCommand(@"
                                            SELECT TOP(100)
                                                [AccID], [Network], [Username], [Added]
                                            FROM UAccounts
                                            WHERE
                                                [Username] LIKE @Username
                                                AND [Network] = @Network
                                                AND [Removed] = 0", dbConn);
                                    SearchAccount.Parameters.Add("@Username", SqlDbType.NVarChar).Value = "%" + a + "%";
                                    SearchAccount.Parameters.Add("@Network", SqlDbType.NVarChar).Value = network;

                                    using (SqlDataReader results = await SearchAccount.ExecuteReaderAsync())
                                    {
                                        if (results.HasRows)
                                        {
                                            while (await results.ReadAsync())
                                            {
                                                Accounts.Add(new UAccount() {
                                                    AccID = (Guid)results["AccID"],
                                                    Network = (string)results["Network"],
                                                    Username = (string)results["Username"],
                                                    Added = (DateTime)results["Added"]
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return new APIAccountResponse()
            {
                ResponseErrors = Errors,
                Accounts = Accounts
            };
        }
    }
}
