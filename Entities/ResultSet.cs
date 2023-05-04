using UrlShortenerMCH.Constants;

namespace UrlShortenerMCH.Entities
{
    public class ResultSet
    {
        public bool HasError { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        private string? DescriptionCatch;
        public string? Description
        {
            get
            {
                if (DescriptionCatch == null && StatusCode == StatusCodes.Status200OK) return ConstantStrings.SuccessfulMessage;
                return DescriptionCatch;
            }
            set
            {
                DescriptionCatch = value;
            }
        }
        public string Url { get; set; } = string.Empty;
    }
}
