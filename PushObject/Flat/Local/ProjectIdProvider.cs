using System;

namespace PushObject.Flat.Local
{
    internal class ProjectIdProvider : IProjectIdProvider
    {
        public string Id => Environment.GetEnvironmentVariable("GCP_PROJECT"); //"parlr-342110"
    }
}