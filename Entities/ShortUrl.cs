using Microsoft.AspNetCore.WebUtilities;

namespace UrlShortenerMCH.Entities
{
    public class ShortUrl
    {
        public int Id { get; protected set; }
        public string Url { get; protected set; }
        public string? CustomUrl { get; protected set; }
        public string UrlChunk => CustomUrl ?? WebEncoders.Base64UrlEncode(BitConverter.GetBytes(Id));

        public ShortUrl(Uri url, string? customUrl = null)
        {
            Url = url.ToString();
            if (customUrl != null && !string.IsNullOrEmpty(customUrl))
                CustomUrl = customUrl;
        }
    }
}
