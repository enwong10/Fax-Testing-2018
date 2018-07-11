using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FaxTesting
{
    class Program
    {
        public static string access_id;
        public static string access_pwd;
        private static string serverUrl = "https://www.srfax.com/SRF_SecWebSvc.php";

        private static bool lastStatus;
        private static string lastResponse;

        static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            int action;

            access_id = "";
            access_pwd = "";
            parameters.Add("sCallerID", "(111)111-111");
            parameters.Add("sSenderEmail", "");
            parameters.Add("sFaxType", "SINGLE");
            parameters.Add("sToFaxNumber", "1-(123)456-7890");
            parameters.Add("sFileName_1", "Test.pdf");
            parameters.Add("sRetries", "6");
            parameters.Add("sFaxFormat", "PDF");
            parameters.Add("sDirection", "IN");
            parameters.Add("sFileContent_1", Convert.ToBase64String(System.IO.File.ReadAllBytes(@"c:\Users\enochw\Desktop\TEST.pdf")));
            parameters.Add("sFaxFromHeader", "TEST TEST TEST");

            do
            {
                Console.Write("(1) Queue Fax \n(2) Get Fax Status \n(3) Get Fax Inbox \n(4) Retrieve Fax \n(0) Exit \nPlease enter a command: ");
                action = int.Parse(Console.ReadLine());

                switch (action)
                {
                    case 1:
                        Queue_Fax(parameters);
                        var result = JsonConvert.DeserializeObject<FaxQueueStatus>(lastResponse);
                        Console.WriteLine("Status: " + result.Status);
                        Console.WriteLine("Fax ID: " + result.Result);
                        Console.ReadLine();
                        break;
                    case 2:
                        Console.Write("Please enter Fax ID: ");
                        string ID = Console.ReadLine();

                        if (parameters.ContainsKey("sFaxDetailsID") == false)
                            parameters.Add("sFaxDetailsID", ID);

                        else
                            parameters["sFaxDetailsID"] = ID;

                        Get_FaxStatus(parameters);

                        try
                        {
                            var status = JsonConvert.DeserializeObject<FaxStatus>(lastResponse);
                            Console.WriteLine();
                            Console.WriteLine("Status: " + status.Status);
                            Console.WriteLine("Sent Status: " + status.Result.SentStatus);
                            Console.WriteLine("Filename: " + status.Result.FileName);
                            Console.WriteLine("Date Queued: " + status.Result.DateQueued);
                            Console.WriteLine("Date Sent: " + status.Result.DateSent);
                            Console.WriteLine("Pages: " + status.Result.Pages);
                            Console.WriteLine("Duration: " + status.Result.Duration);
                            Console.WriteLine("RemoteID: " + status.Result.RemoteID);
                        }

                        catch
                        {
                            var status = JsonConvert.DeserializeObject<FaxQueueStatus>(lastResponse);
                            Console.WriteLine();
                            Console.WriteLine("Error: " + status.Result);
                        }

                        Console.ReadLine();
                        break;
                    case 3:
                        if (parameters.ContainsKey("sFaxDetailsID") == true)
                            parameters.Remove("sFaxDetailsID");

                        Get_Fax_Inbox(parameters);

                        try
                        {
                            var response = JsonConvert.DeserializeObject<FaxInbox>(lastResponse);
                            Output(response.Result);
                        }

                        catch
                        {
                            var status = JsonConvert.DeserializeObject<FaxQueueStatus>(lastResponse);
                            Console.WriteLine("Error: " + status.Result);
                        }
                        break;
                    case 4:
                        Console.Write("Please enter Fax ID: ");
                        string faxID = Console.ReadLine();

                        if (parameters.ContainsKey("sFaxDetailsID") == false)
                            parameters.Add("sFaxDetailsID", faxID);
                        else
                            parameters["sFaxDetailsID"] = faxID;

                        Retrieve_Fax(parameters);

                        var faxResponse = JsonConvert.DeserializeObject<FaxQueueStatus>(lastResponse);

                        if (faxResponse.Status == "Success")
                            Console.WriteLine("Base64 Encoded Contents: " + faxResponse.Result);
                        else
                            Console.WriteLine("Error: " + faxResponse.Result);
                        break;
                }
            }
            while (action != 0);
        }

        public static void Output(FaxDetails[] Result)
        {
            try
            {
                for (int i = 0; i < 51; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine("FileName: " + Result[i].FileName);
                    Console.WriteLine("ReceiveStatus: " + Result[i].ReceiveStatus);
                    Console.WriteLine("Date: " + Result[i].Date);
                    Console.WriteLine("EpochTime: " + Result[i].EpochTime);
                    Console.WriteLine("CallerID: " + Result[i].CallerID);
                    Console.WriteLine("RemoteID: " + Result[i].RemoteID);
                    Console.WriteLine("Pages: " + Result[i].Pages);
                    Console.WriteLine("Size: " + Result[i].Size);
                    Console.WriteLine("ViewedStatus: " + Result[i].ViewedStatus);
                }
            }

            catch
            {
                Console.ReadLine();
            }
        }

        public static bool Queue_Fax(Dictionary<string, string> parameters)
        {
            string[] requiredFields = { "sCallerID", "sSenderEmail", "sFaxType", "sToFaxNumber" };
            string[] optionalFields = {"sResponseFormat", "sAccountCode", "sRetries", "sCoverPage", "sCPFromName", "sCPToName", "sCPOrganization",
                                   "sCPSubject", "sCPComments", "sFileName_*", "sFileContent_*", "sNotifyURL", "sFaxFromHeader", "sQueueFaxDate", "sQueueFaxTime" };

            _validateRequiredVariables(requiredFields, parameters);

            Dictionary<string, string> postVariables = _preparePostVariables(requiredFields, optionalFields, parameters);

            postVariables.Add("action", "Queue_Fax");
            postVariables.Add("access_id", access_id);
            postVariables.Add("access_pwd", access_pwd);

            string result = _processRequest(postVariables);

            _processResponse(result);

            return lastStatus;
        }

        public static bool Get_FaxStatus(Dictionary<string, string> parameters)
        {
            string[] requiredFields = { "sFaxDetailsID" };
            string[] optionalFields = { "sResponseFormat" };

            _validateRequiredVariables(requiredFields, parameters);

            Dictionary<string, string> postVariables = _preparePostVariables(requiredFields, optionalFields, parameters);

            postVariables.Add("action", "Get_FaxStatus");
            postVariables.Add("access_id", access_id);
            postVariables.Add("access_pwd", access_pwd);

            string result = _processRequest(postVariables);

            _processResponse(result);

            return lastStatus;
        }

        public static bool Get_Fax_Inbox(Dictionary<string, string> parameters)
        {

            string[] requiredFields = { };
            string[] optionalFields = { "sResponseFormat", "sPeriod", "sStartDate", "sEndDate", "sViewedStatus", "sIncludeSubUsers", "sFaxDetailsID" };


            Dictionary<string, string> postVariables = _preparePostVariables(requiredFields, optionalFields, parameters);

            postVariables.Add("action", "Get_Fax_Inbox");
            postVariables.Add("access_id", access_id);
            postVariables.Add("access_pwd", access_pwd);

            string result = _processRequest(postVariables);

            _processResponse(result);

            return lastStatus;

        }

        public static bool Retrieve_Fax(Dictionary<string, string> parameters)
        {
            string[] requiredFields = { "sFaxFileName|sFaxDetailsID", "sDirection" };
            string[] optionalFields = { "sFaxFormat", "sMarkasViewed", "sResponseFormat", "sSubUserID" };

            _validateRequiredVariables(requiredFields, parameters);

            Dictionary<string, string> postVariables = _preparePostVariables(requiredFields, optionalFields, parameters);

            postVariables.Add("action", "Retrieve_Fax");
            postVariables.Add("access_id", access_id);
            postVariables.Add("access_pwd", access_pwd);

            string result = _processRequest(postVariables);

            _processResponse(result);

            return lastStatus;
        }

        private static void _processResponse(string response)
        {
            if (response.IndexOf("Success") != -1)
            {
                lastStatus = true;
            }
            else
            {
                lastStatus = false;
            }

            lastResponse = response;

            return;
        }

        private static string _processRequest(Dictionary<string, string> postVariables)
        {
            string queryString = _prepareQueryString(postVariables);
            string result = "";
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                result = wc.UploadString(serverUrl, queryString);
            }

            return result;
        }


        private static string _prepareQueryString(Dictionary<string, string> postVariables)
        {
            string queryString = "";

            foreach (KeyValuePair<string, string> entry in postVariables)
            {
                queryString += entry.Key.ToString() + "=" + UrlEncode(entry.Value.ToString()) + "&";
            }

            // remove last &
            queryString = queryString.Remove(queryString.Length - 1);

            return queryString;
        }

        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();

            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str);

            for (int i = 0; i < byStr.Length; i++)

            {

                sb.Append(@"%" + byStr[i].ToString("X2"));

            }

            return (sb.ToString());
        }

        private static Dictionary<string, string> _preparePostVariables(string[] requiredFields, string[] optionalFields, Dictionary<string, string> parameters)
        {
            Dictionary<string, string> postVariables = new Dictionary<string, string>();

            List<string> list = new List<string>();
            list.AddRange(requiredFields);
            list.AddRange(optionalFields);

            string[] inputVariables = list.ToArray();


            foreach (string field in inputVariables)
            {
                if (field.EndsWith("*") && field.IndexOf('|') == -1) // non-piped wildcard
                {
                    string fieldPrefix = field.Replace("*", "");
                    Dictionary<string, string> wildCards = _getWildcardVaribles(fieldPrefix, parameters);
                    postVariables = _mergeDictionaries(postVariables, wildCards);

                }
                else
                {
                    if (field.IndexOf('|') != -1) // piped, non-wildcard
                    {
                        string[] pipedFields = field.Split('|');

                        foreach (string pipedField in pipedFields)
                        {
                            if (pipedField.EndsWith("*")) // piped wildcard
                            {
                                string fieldPrefix = pipedField.Replace("*", "");
                                Dictionary<string, string> wildCards = _getWildcardVaribles(fieldPrefix, parameters);
                                postVariables = _mergeDictionaries(postVariables, wildCards);
                            }
                            else
                            {
                                if (parameters.ContainsKey(pipedField))
                                {
                                    string value = parameters[pipedField];
                                    if (value.Length > 0)
                                    {
                                        postVariables.Add(pipedField, value);
                                    }
                                }
                            }
                        }


                    }
                    else //non-special fieldname
                    {
                        if (parameters.ContainsKey(field))
                        {
                            postVariables.Add(field, parameters[field]);
                        }
                    }
                }
            }

            return postVariables;
        }


        private static Dictionary<string, string> _getWildcardVaribles(string fieldPrefix, Dictionary<string, string> parameters)
        {
            Dictionary<string, string> wildCards = new Dictionary<string, string>();
            bool done = false;
            int suffix = 1;

            while (!done)
            {
                string field = fieldPrefix + suffix;

                if (parameters.ContainsKey(field))
                {
                    string value = parameters[field];

                    if (value.Length > 0) // add variable to the collection
                    {
                        wildCards.Add(field, value);
                    }
                    else // field value is empty, so finish
                    {
                        done = true;
                    }
                }
                else
                {
                    done = true;
                }

                suffix++;

                // fail safe to ensure no infinite loops
                if (suffix > 1000)
                {
                    done = true;
                }

            }

            return wildCards;
        }

        private static void _validateRequiredVariables(string[] requiredVariables, Dictionary<string, string> parameters)
        {
            foreach (string field in requiredVariables)
            {
                string error = "";

                if (field.EndsWith("*") && field.IndexOf("|") == -1) // non piped wildcard variable.  check for first instance
                {
                    string fieldPrefix = field.Replace("*", "");
                    string wildCard = fieldPrefix + "1";

                    if (!parameters.ContainsKey(wildCard))
                    {
                        error = "Required Field missing.  No values for " + fieldPrefix;
                    }
                    else
                    {
                        string value = parameters[wildCard];
                        if (value.Length <= 0)
                        {
                            error = "Required Field missing.  No values for " + fieldPrefix;
                        }
                    }
                }

                else
                {

                    if (field.IndexOf("|") != -1) // piped separated variable.  At lease 1 must be present.
                    {
                        string[] pipedFields = field.Split('|');
                        bool checkSuccessful = false;

                        foreach (string pipedField in pipedFields)
                        {
                            string trimmedPipedField = pipedField.Trim();

                            if (trimmedPipedField.EndsWith("*")) // piped value has a wildcard, look for first value
                            {
                                string prefix = trimmedPipedField.Replace("*", "");
                                string wildcard = prefix + "1";

                                if (parameters.ContainsKey(wildcard)) // parameter exists, check to make sure it has a value
                                {
                                    string pVal = parameters[wildcard];
                                    if (pVal.Length > 0)
                                    {
                                        checkSuccessful = true;
                                    }
                                }
                            }

                            else
                            {
                                if (parameters.ContainsKey(trimmedPipedField))
                                {
                                    string pVal = parameters[trimmedPipedField];
                                    if (pVal.Length > 0)
                                    {
                                        checkSuccessful = true;
                                    }
                                }
                            }
                        }

                        if (!checkSuccessful)
                        {
                            error = "Required field missing.  You must provide at lease 1 of the following: " + string.Join(",", pipedFields);
                        }

                    }

                    else // standard field, check if it exists
                    {
                        if (!parameters.ContainsKey(field))
                        {
                            error = "Required field " + field + " is missing!";
                        }

                        else // ensure field value is not empty
                        {
                            string value = parameters[field];
                            if (value.Length <= 0)
                            {
                                error = "Required field " + field + " is missing!";
                            }
                        }
                    }
                }

                if (error.Length > 0)
                {
                    throw (new Exception(error));
                }
            }

            return;
        }

        // merges 2 dictionaries into one, stops duplicates
        private static Dictionary<string, string> _mergeDictionaries(Dictionary<string, string> d1, Dictionary<string, string> d2)
        {
            Dictionary<string, string> mergedDictionary = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> entry in d1)
            {
                if (!mergedDictionary.ContainsKey(entry.Key))
                {
                    mergedDictionary.Add(entry.Key, entry.Value);
                }
            }

            foreach (KeyValuePair<string, string> entry in d2)
            {
                if (!mergedDictionary.ContainsKey(entry.Key))
                {
                    mergedDictionary.Add(entry.Key, entry.Value);
                }
            }

            return mergedDictionary;
        }
    }
}
