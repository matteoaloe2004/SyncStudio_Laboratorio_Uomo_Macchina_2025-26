using System.Collections.Generic;
using Template.Services.Shared;

namespace Template.Web.Features.Home
{
    public class HomeViewModel
    {
        public List<CorsoDTO> Corsi { get; set; }
        public List<StanzaStudioDTO> Stanze { get; set; }
        public string NickName { get; set; }
    }
}
