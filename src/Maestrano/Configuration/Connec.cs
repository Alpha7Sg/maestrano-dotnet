using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestrano.Configuration
{
    public class Connec : ConfigurationSection
    {
        private const string ProdDefaultApiHost = "https://api-connec.maestrano.com";
        private const string ProdDefaultBasePath = "/api/v2";
        private const string ProdDefaultReportBasePath = "/api/reports";
        private const string TestDefaultApiHost = "http://api-sandbox.maestrano.io";
        private const string TestDefaultBasePath = "/connec/api/v2";
        private const string TestDefaultReportBasePath = "/api/reports";

        private string presetName;

        /// <summary>
        /// Load configuration into a Connec configuration object
        /// </summary>
        /// <returns>A Connec configuration object</returns>
        public static Connec Load(string preset = "maestrano")
        {
            ConfigurationManager.RefreshSection(preset + "/connec");
            var config = ConfigurationManager.GetSection(preset + "/connec") as Connec;
            if (config == null) config = new Connec();
            config.presetName = preset;

            return config;
        }

        /// <summary>
        /// Return the Connec! API Host
        /// </summary>
        [ConfigurationProperty("host", DefaultValue = null, IsRequired = false)]
        public string Host
        {
            get
            {
                var _host = (String)this["host"];
                if (string.IsNullOrEmpty(_host))
                {
                    if (MnoHelper.With(this.presetName).isProduction())
                    {
                        return ProdDefaultApiHost;
                    }
                    else
                    {
                        return TestDefaultApiHost;
                    }
                }
                return _host;
            }

            set { this["host"] = value; }
        }

        /// <summary>
        /// Return the Connec! API Base Path
        /// </summary>
        [ConfigurationProperty("base-path", DefaultValue = null, IsRequired = false)]
        public string BasePath
        {
            get
            {
                var _path = (String)this["base-path"];
                if (string.IsNullOrEmpty(_path))
                {
                    if (MnoHelper.With(this.presetName).isProduction())
                    {
                        return ProdDefaultBasePath;
                    }
                    else
                    {
                        return TestDefaultBasePath;
                    }
                }
                return _path;
            }

            set { this["base-path"] = value; }
        }

        /// <summary>
        /// Return the Connec! Report Base Path
        /// </summary>
        [ConfigurationProperty("report-base-path", DefaultValue = null, IsRequired = false)]
        public string ReportBasePath
        {
            get
            {
                var _path = (String)this["report-base-path"];
                if (string.IsNullOrEmpty(_path))
                {
                    if (MnoHelper.With(this.presetName).isProduction())
                    {
                        return ProdDefaultReportBasePath;
                    }
                    else
                    {
                        return TestDefaultReportBasePath;
                    }
                }
                return _path;
            }

            set { this["report-base-path"] = value; }
        }
    }
}
