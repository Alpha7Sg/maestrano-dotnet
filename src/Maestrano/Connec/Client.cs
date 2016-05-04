using Maestrano.Api;
using Maestrano.Helpers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Maestrano.Connec
{
    public class Client
    {
        private string groupId;
        private string presetName;
        private RestClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupId">The customer group id</param>
        public Client (string groupId = null)
        {
            this.New(groupId);
        }

        /// <summary>
        /// Return a Client for a specific preset
        /// New scoped clients can be initialized with: Client.With("preset").New("group-id")
        /// </summary>
        /// <param name="presetName"></param>
        /// <returns></returns>
        public static Client With(string presetName = "maestrano") {
            Client scopedClient = new Client();
            scopedClient.presetName = presetName;
            return scopedClient;
        }

        /// <summary>
        /// Reinitialize a Client with a new group id, but without modifying
        /// any existing configuration preset
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public Client New(string groupId)
        {
            this.groupId = groupId;
            this._client = null;
            if (presetName == null)
            {
                presetName = "maestrano";
            }
            return this;
        }

        private RestClient InternalClient()
        {
            if (_client == null)
            {
                var connecBasePath = MnoHelper.With(presetName).Connec.BasePath;
                var connecScopedPath = connecBasePath + "/" + groupId;

                // silverlight friendly way to get current version
                var assembly = Assembly.GetExecutingAssembly();
                AssemblyName assemblyName = new AssemblyName(assembly.FullName);
                var version = assemblyName.Version;

                _client = new RestClient();
                _client.UserAgent = "maestrano-dotnet/" + version;
                _client.AddDefaultHeader("Accept", "application/vnd.api+json");
                _client.AddDefaultHeader("Content-Type", "application/vnd.api+json");

                _client.Authenticator = new HttpBasicAuthenticator(
                    MnoHelper.With(presetName).Api.Id, 
                    MnoHelper.With(presetName).Api.Key
                    );

                _client.BaseUrl = String.Format("{0}{1}", 
                    MnoHelper.With(presetName).Connec.Host, 
                    connecScopedPath
                    );
                
            }

            return _client;
        }

        /// <summary>
        /// Return a configured RestClient
        /// </summary>
        /// <param name="groupId">The customer group id</param>
        public static RestClient GetRestClient(string groupId) {
            return (RestClient)(new Client(groupId)).InternalClient();
        }

        private IRestRequest PrepareRequest(Method method, String path, NameValueCollection parameters, String body)
        {
            var request = new RestRequest();
            request.Resource = path;
            request.Method = method;
            request.AddHeader("Accept", "application/vnd.api+json");
            request.AddHeader("Content-Type", "application/vnd.api+json");
            request.Parameters.Clear();

            if (body != null)
                request.AddParameter("application/vnd.api+json", body, ParameterType.RequestBody);

            if (parameters != null)
            {
                foreach (var k in parameters.AllKeys)
                    request.AddParameter(StringExtensions.ToSnakeCase(k), parameters[k]);
            }

            return request;
        }

        public RestResponse GetReport(string path)
        {
            //override base url to get report
            var myClient = InternalClient();
            var connecBasePath = MnoHelper.With(presetName).Connec.BasePath;
            var reportBasePath = MnoHelper.With(presetName).Connec.ReportBasePath;
            var connecScopedPath = connecBasePath + "/" + groupId;

            myClient.BaseUrl = String.Format("{0}{1}",
                    MnoHelper.With(presetName).Connec.Host,
                    reportBasePath + "/" + groupId
                    );
            // send request
            var request = PrepareRequest(Method.GET, path, null, null);
            var response = (RestResponse)myClient.Execute(request);

            //reset correct url 
            myClient.BaseUrl = String.Format("{0}{1}",
                    MnoHelper.With(presetName).Connec.Host,
                    connecScopedPath
                    );

            //return report 
            return response;
        }

        public RestResponse Get(string path)
        {
            return (RestResponse)InternalClient().Execute(PrepareRequest(Method.GET, path, null, null));
        }

        public IRestResponse Get(string path, NameValueCollection parameters)
        {
            return (RestResponse)InternalClient().Execute(PrepareRequest(Method.GET, path, parameters, null));
        }

        public RestResponse<T> Get<T>(string path) where T : new()
        {
            return (RestResponse<T>)InternalClient().Execute<T>(PrepareRequest(Method.GET, path, null, null));
        }

        public RestResponse<T> Get<T>(string path, NameValueCollection parameters) where T : new()
        {
            return (RestResponse<T>)InternalClient().Execute<T>(PrepareRequest(Method.GET, path, parameters, null));
        }



        public IRestResponse Post(string path, string jsonBody)
        {
            return (RestResponse)InternalClient().Execute(PrepareRequest(Method.POST, path, null, jsonBody));
        }

        public RestResponse<T> Post<T>(string path, string jsonBody) where T : new()
        {
            return (RestResponse<T>)InternalClient().Execute<T>(PrepareRequest(Method.POST, path, null, jsonBody));
        }


        public IRestResponse Put(string path, string jsonBody)
        {
            return (RestResponse)InternalClient().Execute(PrepareRequest(Method.PUT, path, null, jsonBody));
        }

        public RestResponse<T> Put<T>(string path, string jsonBody) where T : new()
        {
            return (RestResponse<T>)InternalClient().Execute<T>(PrepareRequest(Method.PUT, path, null, jsonBody));
        }
    }
}
