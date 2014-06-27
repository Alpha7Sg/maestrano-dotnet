﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Maestrano.Sso
{
    public class Session
    {

        public string Uid { get; set; }
        public string GroupUid { get; set; }
        public DateTime Recheck { get; set; }
        public string SessionToken { get; set; }
        public HttpSessionState HttpSession { get; set; }

        /// <summary>
        /// Contructor retrieving maestrano session from
        /// http session
        /// </summary>
        /// <param name="httpSessionObj"></param>
        public Session(HttpSessionState httpSessionObj)
        {
            HttpSession = httpSessionObj;

            if (HttpSession != null && HttpSession["maestrano"] != null)
            {
                var enc = System.Text.Encoding.UTF8;
                JObject sessionObject = new JObject();
                try {
                    string decryptedMnoSession = enc.GetString(Convert.FromBase64String(HttpSession["maestrano"].ToString()));
                    sessionObject = JObject.Parse(decryptedMnoSession);
                }
                catch (Exception){ }
                
                // Assign attributes
                Uid = sessionObject.Value<String>("uid");
                GroupUid = sessionObject.Value<String>("group_uid");
                SessionToken = sessionObject.Value<String>("session");

                // Session Recheck
                try
                {
                    Recheck = sessionObject.Value<DateTime>("session_recheck");
                }
                catch (Exception) { }
                

                if (Recheck == null)
                    Recheck = DateTime.UtcNow.AddMinutes(-1);
            }
        }

        /// <summary>
        /// Contructor retrieving maestrano session from user
        /// </summary>
        /// <param name="httpSessionObj"></param>
        /// <param name="user"></param>
        public Session(HttpSessionState httpSessionObj, User user)
        {
            HttpSession = httpSessionObj;

            if (user != null)
            {
                Uid = user.Uid;
                GroupUid = user.GroupUid;
                SessionToken = user.SsoSession;
                Recheck = user.SsoSessionRecheck;
            }
        }

        /// <summary>
        /// Returns whether the session needs to be checked
        /// remotely from maestrano or not
        /// </summary>
        /// <returns></returns>
        public Boolean isRemoteCheckRequired()
        {
            if (Uid != null && SessionToken != null && Recheck != null)
            {
                return (Recheck.CompareTo(DateTime.UtcNow) <= 0);
            }
            return true;
        }

        /// <summary>
        /// Check whether the remote maestrano session is still
        /// valid
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public Boolean PerformRemoteCheck(RestClient client)
        {
            if (Uid != null && SessionToken != null && Uid.Length > 0 && SessionToken.Length > 0)
            {
                // Prepare request
                var request = new RestRequest("api/v1/auth/saml/{id}", Method.GET);
                request.AddUrlSegment("id", Uid);
                request.AddParameter("session", SessionToken);
                JObject resp = new JObject();
                try {
                    resp = JObject.Parse(client.Execute(request).Content);
                }
                catch (Exception) { }

                bool valid = Convert.ToBoolean(resp.Value<String>("valid"));
                string dateStr = resp.Value<String>("recheck");
                if ( valid && dateStr != null && dateStr.Length > 0)
                {
                    Recheck = DateTime.Parse(dateStr);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check whether the remote maestrano session is still
        /// valid
        /// </summary>
        /// <returns></returns>
        public Boolean PerformRemoteCheck()
        {
            var client = new RestClient(Maestrano.Sso.Idp);
            return PerformRemoteCheck(client);
        }

        /// <summary>
        /// Return wether the session is valid or not. Perform
        /// remote check to maestrano if recheck is overdue.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ifSession"></param>
        /// <returns></returns>
        public Boolean IsValid(RestClient client, Boolean ifSession = false)
        {
            // Return true automatically if SLO is disabled
            if (!Maestrano.Sso.SloEnabled)
                return true;

            // Return true if maestrano session not set
            // and ifSession option enabled
            if (ifSession && (HttpSession == null || HttpSession["maestrano"] == null))
                return true;

            // Return false if HttpSession is nil
            if (HttpSession == null)
                return false;

            if (isRemoteCheckRequired())
            {
                if (PerformRemoteCheck(client))
                {
                    Save();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Return wether the session is valid or not. Perform
        /// remote check to maestrano if recheck is overdue.
        /// </summary>
        /// <param name="ifSession">If set to true then session return false ONLY if maestrano session exists and is invalid</param>
        /// <returns></returns>
        public Boolean IsValid(Boolean ifSession = false)
        {
            var client = new RestClient(Maestrano.Sso.Idp);
            return IsValid(client,ifSession);
        }

        /// <summary>
        /// Save the Maestrano session in
        /// HTTP Session
        /// </summary>
        public void Save()
        {
            var enc = System.Text.Encoding.UTF8;
            JObject sessionObject = new JObject(
                new JProperty("uid",Uid),
                new JProperty("session",SessionToken),
                new JProperty("session_recheck",Recheck.ToString("s")),
                new JProperty("group_uid",GroupUid));

            // Finally store the maestrano session
            HttpSession["maestrano"] = Convert.ToBase64String(enc.GetBytes(sessionObject.ToString()));
        }

    }
}
