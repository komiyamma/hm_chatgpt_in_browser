using System.ComponentModel.DataAnnotations;

namespace HmChatGptInBrowser.Data
{
    public class SearchModel
    {
        [Required]
        public string SearchText { get; set; } = "";

        [Required]
        public int NoOfResults { get; set; } = 2;

        [Required]
        public int MaxTokens { get; set; } = 2000;

        public string System { get; set; } = "";

        public string Assistant { get; set; } = "";
    }
         
}