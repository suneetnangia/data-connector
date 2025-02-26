namespace Data.Connector.Svc
{
    public static class UriExtensions
    {
        private static readonly string[] Macros = { "{yyyy-mm-dd}", "{yyyy-mm}" };

        public static Uri Normalize(this Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri, nameof(uri));

            foreach (var macro in Macros)
            {
                switch (macro)
                {
                    case "{yyyy-mm-dd}":
                        uri = new Uri(uri.ToString().Replace(macro, DateTime.Now.ToString("yyyy-MM-dd"), StringComparison.InvariantCulture), UriKind.Relative);
                        break;
                    case "{yyyy-mm}":
                        uri = new Uri(uri.ToString().Replace(macro, DateTime.Now.ToString("yyyy-MM"), StringComparison.InvariantCulture), UriKind.Relative);
                        break;
                    default:
                        throw new NotSupportedException($"Macro {macro} is not supported.");
                }
            }

            return uri;
        }
    }
}
