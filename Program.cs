using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace QCS
{
    class Whitelist
    {
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "QCS Whitelister.";

        static string WHITELIST_FILEPATH = "/opt/minecraft-new/whitelist.json";
        static int RUN_MINS = 5;
        async static Task Main(string[] args)
        {
            Console.WriteLine("QCS Minecraft Whitelister\nIn startup");

            //Load spreadsheet IDs from file
            String[] spData = File.ReadAllLines("spreadsheets.txt");
            String formSheetID = spData[0];
            String membersSheetID = spData[1];
            String emailValidationOverrideCode = spData[2].ToLower();

            do
            {
                Console.WriteLine("Beginning run");
                UserCredential credential;
                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });


                //Get form responses
                String formRange = "whitelist!B2:E";
                SpreadsheetsResource.ValuesResource.GetRequest formRequest =
                        service.Spreadsheets.Values.Get(formSheetID, formRange);


                ValueRange formResponse = formRequest.Execute();
                IList<IList<Object>> formValues = formResponse.Values;

                //Get current member emails
                String currentMembersRange = "FULL LIST (FOR SU)!D2:F";
                SpreadsheetsResource.ValuesResource.GetRequest currentMembersRequest =
                        service.Spreadsheets.Values.Get(membersSheetID, currentMembersRange);


                ValueRange currentMembersResponse = currentMembersRequest.Execute();
                IList<IList<Object>> currentMemberEmails = currentMembersResponse.Values;

                if (formValues != null && formValues.Count > 0 && currentMemberEmails != null && currentMemberEmails.Count > 0)
                {
                    //Create a list of Whitelist Requests
                    List<WhiteListRequest> usersToCheck = new List<WhiteListRequest>();
                    foreach (var row in formValues)
                    {
                        if (row.Count != 3)
                        {
                            if (!row[0].Sanitise().IsNullOrEmpty() && !row[1].Sanitise().IsNullOrEmpty() && !row[2].Sanitise().IsNullOrEmpty())
                            {

                                usersToCheck.Add(new WhiteListRequest(row[1].Sanitise(), row[0].Sanitise(), row[2].Sanitise()));


                            }
                            else
                            {
                                if (row[0].Sanitise().IsNullOrEmpty() && row[1].Sanitise().IsNullOrEmpty() && row[2].Sanitise().IsNullOrEmpty())
                                {
                                    //We've reached the end. Break;
                                    break;
                                }
                                Console.WriteLine("Error processing row " + (row[0].Sanitise().IsNullOrEmpty()) + (row[1].Sanitise().IsNullOrEmpty()) + (row[2].Sanitise().IsNullOrEmpty()));
                            }
                        }
                    }
                    //Transfer the emails and student numbers  into a List
                    List<String[]> studentRecords = new List<String[]>();
                    currentMemberEmails.ToList().ForEach(el =>
                    {

                        if (el.Count > 0)
                        {
                            string[] data = new string[2];


                            if ((string)el[1] == "Not Provided")
                            {

                                data[0] = el[2].Sanitise();
                            }
                            else
                            {
                                data[0] = el[1].Sanitise();

                            }
                            int studentNo;
                            if (Int32.TryParse(el[0].Sanitise(), out studentNo))
                            {
                                if (studentNo != 0)
                                    data[1] = studentNo.ToString();
                                else data[1] = "-1";
                            }
                            else
                            {
                                data[1] = "-1";
                            }

                            studentRecords.Add(data);

                        }
                        else
                        {
                            Console.WriteLine("Empty email detected. Please check spreadsheet");
                        }
                    });


                    //Check each whitelist request to see if the email exists within our DB

                    List<WhitelistApproved> usersToWhitelist = new List<WhitelistApproved>();

                    foreach (WhiteListRequest wr in usersToCheck)
                    {
                        if (wr.Email == emailValidationOverrideCode)
                        {
                            usersToWhitelist.Add(new WhitelistApproved(wr));

                        }
                        else
                        {




                            if (studentRecords.Find(e => e[0] == wr.Email) != null)
                            {
                                //Email exists, we are good to go.
                                usersToWhitelist.Add(new WhitelistApproved(wr));
                            }
                            else if (wr.studentNumber != "-1" && studentRecords.Find(e => e[1] == wr.studentNumber) != null)
                            {
                                usersToWhitelist.Add(new WhitelistApproved(wr));
                            }
                            else
                            {
                                //User does not exist
                                Console.WriteLine(String.Format("Could not find {0} in the member database ({1})", wr.Email, wr.Username));
                            }

                        }

                    }

                    //Just for security
                    usersToWhitelist = usersToWhitelist.OrderBy(e => e.Username).ToList();

                    //Remove duplicates

                    List<WhitelistApproved> usersToWhiteList_noDuplicates = new List<WhitelistApproved>();
                    usersToWhitelist.ForEach(usr =>
                     {
                         if (!usersToWhiteList_noDuplicates.Any(u => u.isSameUser(usr.Username)))
                         {
                             usersToWhiteList_noDuplicates.Add(usr);
                         }
                     });


                    //Now, we need to get the UUIDs of the player names and then generate the appropiate JSON.

                    List<String> jsonData = new List<string>();
                    foreach (WhitelistApproved wLA in usersToWhiteList_noDuplicates)
                    {
                        try
                        {

                            jsonData.Add(await wLA.GetJSON());
                        }
                        catch
                        {
                            Console.WriteLine("Error Processing user " + wLA.Username);
                        }
                    }
                    String json = "[" + string.Join(',', jsonData) + "]";
                    Console.WriteLine(json);

                    //Output generated JSON to file
                    await File.WriteAllTextAsync(WHITELIST_FILEPATH, json);

                    //Ask minecraft to reload the server
                    "screen -S minecraft -p 0 -X stuff \"whitelist reload^M\"".Bash();

                }
                else
                    Console.WriteLine("No data found.");

                Console.WriteLine("Run Complete.\nWaiting...");
                await Task.Delay(RUN_MINS * 60000);
            } while (true);
        }
    }




}