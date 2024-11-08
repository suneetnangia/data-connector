namespace Http.Mqtt.Connector.Svc
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

        public static string GenerateHash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
